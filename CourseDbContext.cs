
using Microsoft.EntityFrameworkCore;


namespace UniversityApi;

public partial class CourseDbContext : DbContext
{
    public CourseDbContext()
    {
    }

    public CourseDbContext(DbContextOptions<CourseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
           => optionsBuilder.UseMySql("name=CourseDB", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.36-mysql"));
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Category>(entity =>
 {
     entity.HasKey(e => e.Id).HasName("PRIMARY");
     entity.ToTable("category");

     entity.Property(e => e.Id)
         .ValueGeneratedOnAdd()
         .HasColumnName("id");
     entity.Property(e => e.Name)
         .HasMaxLength(100)
         .HasColumnName("name");
     entity.Property(e => e.IconRouting)
         .HasMaxLength(200)
         .HasColumnName("icon_link");
 });
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("course");

            entity.HasIndex(e => e.CategoryId, "category_id");

            entity.HasIndex(e => e.LecturerId, "lecturer_id");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ImgLink)
                .HasMaxLength(200)
                .HasColumnName("img_link");
            entity.Property(e => e.LearningType)
                .HasMaxLength(50)
                .HasColumnName("learning_type");
            entity.Property(e => e.LecturerId).HasColumnName("lecturer_id");
            entity.Property(e => e.LessonsAmount).HasColumnName("lessons_amount");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StartLearning).HasColumnName("start_learning");
            entity.Property(e => e.Syllabus)
                .HasColumnType("text")
                .HasColumnName("syllabus");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY"); // Adding id as the primary key
            entity.ToTable("registration");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            entity.HasIndex(e => e.CourseId, "course_id");
            entity.HasIndex(e => e.UserId, "user_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(120)
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(120)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(12)
                .HasColumnName("password");
            entity.Property(e => e.Role).HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
