using UniversityApi;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Formats.Asn1;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddDbContext<CourseDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CourseDB");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
var mapperConfig = new MapperConfiguration(cfg =>
{
    // cfg.CreateMap<UserToAdd, User>().ReverseMap();
    // cfg.CreateMap<UserToUpdateDTO, User>().ReverseMap();
});

var mapper = new Mapper(mapperConfig);
builder.Services.AddSingleton(mapper);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
 {
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Course API V1");
 });
}
app.UseCors();

app.MapGet("/", () => "Hello Unibersity!");

//------------------------------- user endpoints

//login
app.MapPost(
    "/api/login",
    async (CourseDbContext context, string username, string password) =>
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Name.Equals(username));
        if (user is null)
            return Results.NotFound("שם משתמש לא קיים");
        if (user.Password != password)
            return Results.Unauthorized();
        return Results.Ok(user);
    }
);
//sign in
app.MapPost("/api/register", async (CourseDbContext context, User newUser) =>
{
    var existUser = await context.Users.FirstOrDefaultAsync(u => u.Name.Equals(newUser.Name));
    if (existUser is not null)
        return Results.Conflict("שם משתמש כבר קיים");

    // var userEntity = mapper.Map<UserToAdd, User>(newUser);
    var addedUser = await context.Users.AddAsync(newUser);
    await context.SaveChangesAsync();

    return Results.Ok(addedUser.Entity);
});
//update profile
app.MapPut("/api/user/{id}", async (int id, CourseDbContext context, User updateUser) =>
{
    var user = await context.Users.FindAsync(id);
    if (user == null)
        return Results.NotFound("משתמש לא קיים");

    var existingUserWithSameName = await context.Users.FirstOrDefaultAsync(u => u.Name.Equals(updateUser.Name));
    if (existingUserWithSameName is not null && existingUserWithSameName.Id != updateUser.Id)
        return Results.Conflict("שם משתמש כבר קיים");

    // Update only the properties from the updateUser object to the user entity
    mapper.Map(updateUser, user);

    context.Users.Update(user);
    await context.SaveChangesAsync();

    return Results.Ok(user);
});
//get by id
app.MapGet("/api/user/{id}", async (int id, CourseDbContext context) =>
{
    var user = await context.Users.FindAsync(id);
    if (user is null)
        return Results.NotFound("קוד משתמש לא קיים");
    return Results.Ok(user);
});

//--------------------------category

//get categories
app.MapGet("/api/category", async (CourseDbContext context) =>
{
    var categories = await context.Categories.ToListAsync();
    return Results.Ok(categories);
});
//add category
app.MapPost("/api/category", async (CourseDbContext context, Category newCategory) =>
{
    var category = await context.Categories.FirstOrDefaultAsync(c => c.Name.Equals(newCategory.Name));
    if (category is not null)
        return Results.Conflict("קטגוריה קיימת");
    await context.Categories.AddAsync(newCategory);
    await context.SaveChangesAsync();
    var categories = await context.Categories.ToListAsync();
    return Results.Ok(categories);
});

// course ----------------------------------------

//get all courses 
app.MapGet("/api/course", async (CourseDbContext context) =>
{
    var courses = await context.Courses.ToListAsync();
    return Results.Ok(courses);
});
//get course by id
app.MapGet("/api/course/{id}", async (int id, CourseDbContext context) =>
{
    var course = await context.Courses.FindAsync(id);
    if (course is null)
        return Results.NotFound();
    return Results.Ok(course);
});

//add course
app.MapPost("/api/course", async (CourseDbContext context, Course courseToAdd) =>
{
    var courseAddedEntry = await context.Courses.AddAsync(courseToAdd);
    var courseAdded = courseAddedEntry.Entity;
    var teacher = await context.Users.FindAsync(courseToAdd.LecturerId);
    if (teacher is null || teacher.Role != 1)
        return Results.Conflict("קוד מרצה לא תקין");
    await context.SaveChangesAsync();
    return Results.Ok(courseAdded);
});
// delete course
app.MapDelete("/api/course/{id}", async (int id, CourseDbContext context) =>
{
    var course = await context.Courses.FindAsync(id);
    if (course is null)
        return Results.NotFound();
    context.Courses.Remove(course);
    await context.SaveChangesAsync();
    return Results.Ok();
});
// edit course
app.MapPut("/api/course/{id}", async (int id, CourseDbContext context, Course newCourse) =>
{
    var existCourse = await context.Courses.FindAsync(id);
    if (existCourse is null)
        return Results.NotFound();
    existCourse.Name = newCourse.Name;
    existCourse.CategoryId = newCourse.CategoryId;
    existCourse.ImgLink = newCourse.ImgLink;
    existCourse.LearningType = newCourse.LearningType;
    existCourse.LessonsAmount = newCourse.LessonsAmount;
    existCourse.StartLearning = newCourse.StartLearning;
    existCourse.Syllabus = newCourse.Syllabus;
    context.Courses.Update(existCourse);
    await context.SaveChangesAsync();
    return Results.Ok(existCourse);
});

//Sign up for a course
app.MapPost("/api/course/{courseId}", async (int courseId, CourseDbContext context, int userId) =>
{
    var user = await context.Users.FindAsync(userId);
    var course = await context.Courses.FindAsync(courseId);
    if (user is null)
        return Results.NotFound("קוד משתמש לא תקין");
    if (user.Role != 0)
        return Results.Conflict("כדי להירשם לקורס עליך להזדהות כסטודנט");
    if (course is null)
        return Results.NotFound("קוד קורס לא תקין");
    if (await context.Registrations.AnyAsync(r => r.CourseId == courseId && r.UserId == userId))
        return Results.Conflict("כבר נרשמת לקורס זה");
    Registration registration = new Registration() { Id = 0, UserId = userId, CourseId = courseId };
    var reg = await context.Registrations.AddAsync(registration);
    await context.SaveChangesAsync();
    return Results.Ok();
});

//כל הקורסים של סטודנט/מרצה
app.MapGet("/api/course/user/{id}", async (int id, CourseDbContext context) =>
{
    var user = await context.Users.FindAsync(id);
    if (user is null)
        return Results.NotFound("קוד משתמש לא קיים");
    var coursesByUserId = await context.Registrations
        .Where(r => r.UserId == id)
        .Join(context.Courses,
            r => r.CourseId,
            c => c.Id,
            (r, c) => c)
        .ToListAsync();

    return Results.Ok(coursesByUserId);
});
app.Run();
