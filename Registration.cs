using System;
using System.Collections.Generic;

namespace UniversityApi;

public partial class Registration
{
    public int Id { get; set; }
    public int? UserId { get; set; }

    public int? CourseId { get; set; }

}
