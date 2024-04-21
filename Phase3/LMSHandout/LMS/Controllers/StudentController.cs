using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using static System.Formats.Asn1.AsnWriter;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from enrolled in db.EnrollmentGrades
                        where enrolled.Student == uid
                        join c in db.Classes on enrolled.ClassId equals c.ClassId
                        join course in db.Courses on c.CourseId equals course.CourseId
                        select new
                        {
                            subject = course.Department,
                            number = course.Number,
                            name = course.Name,
                            season = c.Season,
                            year = c.SemesterYear,
                            grade = enrolled.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from assignment in db.Assignments
                        join cat in db.AssignmentCategories
                            on assignment.CategoryId equals cat.CategoryId
                        join c in db.Classes
                            on cat.ClassId equals c.ClassId
                        where c.SemesterYear == year &&
                                c.Season == season
                        join course in db.Courses
                            on c.CourseId equals course.CourseId
                        where course.Department == subject &&
                                course.Number == num
                        select new
                        {
                            AssignmentId = assignment.AssignmentId,
                            aname = assignment.Name,
                            cname = cat.Name,
                            due = assignment.Due
                        };

            var scores = from q in query
                         join s in db.Submissions
                            on new { A = q.AssignmentId, B = uid } equals new { A = s.AssignmentId, B = s.Student }
                         into joined
                         from j in joined.DefaultIfEmpty()
                         select new
                         {
                             aname = q.aname,
                             cname = q.cname,
                             due = q.due,
                             score = j != null ? j.Score : 0
                         };

            return Json(scores.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {


            var query = from assignment in db.Assignments
                        join cat in db.AssignmentCategories
                            on assignment.CategoryId equals cat.CategoryId
                        where cat.Name == category && assignment.Name == asgname
                        join c in db.Classes
                            on cat.ClassId equals c.ClassId
                        where c.SemesterYear == year &&
                                c.Season == season
                        join course in db.Courses
                            on c.CourseId equals course.CourseId
                        where course.Department == subject &&
                                course.Number == num
                        join submission in db.Submissions
                           on new { A = assignment.AssignmentId, B = uid } equals new { A = submission.AssignmentId, B = submission.Student }
                        select submission;



            if (query.Count() > 0)
            {
                foreach (Submission s in query)
                {
                    s.Contents = contents;
                    s.SubmissionDate = DateTime.Now;
                }

            }
            else
            {
                Submission submission = new Submission();
                submission.SubmissionDate = DateTime.Now;
                submission.Contents = contents;
                submission.Score = 0;
                submission.Student = uid;
                submission.AssignmentId = (from assignment in db.Assignments
                                           join cat in db.AssignmentCategories
                                               on assignment.CategoryId equals cat.CategoryId
                                           where cat.Name == category && assignment.Name == asgname
                                           join c in db.Classes
                                               on cat.ClassId equals c.ClassId
                                           where c.SemesterYear == year &&
                                                   c.Season == season
                                           join course in db.Courses
                                               on c.CourseId equals course.CourseId
                                           where course.Department == subject &&
                                                   course.Number == num
                                           select assignment.AssignmentId).First();

                db.Submissions.Add(submission);
            }


            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }



        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            EnrollmentGrade enrolled = new EnrollmentGrade();
            enrolled.Grade = "--";
            enrolled.Student = uid;
            enrolled.ClassId = (from c in db.Classes
                                where c.Season == season && c.SemesterYear == year
                                join course in db.Courses on c.CourseId equals course.CourseId
                                where course.Department == subject && course.Number == num
                                select c.ClassId).First();

            db.EnrollmentGrades.Add(enrolled);

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = from e in db.EnrollmentGrades
                        join s in db.Students
                        on e.StudentNavigation.UId equals s.UId
                        where s.UId == uid
                        select e;




            if (query.Count() == 0)
            {
                return Json(new { gpa = 0.0 });
            }
            else
            {
                double sumOfGrades = 0.0;
                int numberOfNoGrades = 0;
                foreach (EnrollmentGrade e in query)
                {
                    if (e.Grade == "A")
                    {
                        sumOfGrades += 4;
                    }
                    else if (e.Grade == "A-")
                    {
                        sumOfGrades += 3.7;
                    }
                    else if (e.Grade == "B+")
                    {
                        sumOfGrades += 3.3;
                    }
                    else if (e.Grade == "B")
                    {
                        sumOfGrades += 3;
                    }
                    else if (e.Grade == "B-")
                    {
                        sumOfGrades += 2.7;
                    }
                    else if (e.Grade == "C+")
                    {
                        sumOfGrades += 2.3;
                    }
                    else if (e.Grade == "C")
                    {
                        sumOfGrades += 2;
                    }
                    else if (e.Grade == "C-")
                    {
                        sumOfGrades += 1.7;
                    }
                    else if (e.Grade == "D+")
                    {
                        sumOfGrades += 1.3;
                    }
                    else if (e.Grade == "D")
                    {
                        sumOfGrades += 1;
                    }
                    else if (e.Grade == "D-")
                    {
                        sumOfGrades += 0.7;
                    }
                    else if (e.Grade == "E")
                    {
                        sumOfGrades += 0.0;
                    }
                    else
                    {
                        numberOfNoGrades++;
                    }

                }

                double denom = query.Count() - numberOfNoGrades;
                double returnGPA = 0.0;
                if (denom > 0)
                {
                    returnGPA = sumOfGrades / denom;
                }

                return Json(new { gpa = returnGPA });
            }
        }

        /*******End code to modify********/

    }
}

