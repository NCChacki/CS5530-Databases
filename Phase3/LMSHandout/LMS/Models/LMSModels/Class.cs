using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            EnrollmentGrades = new HashSet<EnrollmentGrade>();
        }

        public uint ClassId { get; set; }
        public uint SemesterYear { get; set; }
        public string Season { get; set; } = null!;
        public string Location { get; set; } = null!;
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public string TaughtBy { get; set; } = null!;
        public uint CourseId { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Professor TaughtByNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<EnrollmentGrade> EnrollmentGrades { get; set; }
    }
}
