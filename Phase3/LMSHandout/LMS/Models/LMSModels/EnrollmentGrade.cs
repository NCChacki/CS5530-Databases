using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class EnrollmentGrade
    {
        public string Grade { get; set; } = null!;
        public string Student { get; set; } = null!;
        public uint ClassId { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual Student StudentNavigation { get; set; } = null!;
    }
}
