using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Formats.Asn1.AsnWriter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from enrolled in db.EnrollmentGrades
                        join c in db.Classes on enrolled.ClassId equals c.ClassId
                        where c.Season == season && c.SemesterYear == year
                        join course in db.Courses on c.CourseId equals course.CourseId
                        where course.Department == subject && course.Number == num
                        join s in db.Students on enrolled.Student equals s.UId
                        select new
                        {
                            fname = s.FirstName,
                            lname = s.LastName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = enrolled.Grade
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category == null)
            {
                var query = from assign in db.Assignments
                            join cat in db.AssignmentCategories on assign.CategoryId equals cat.CategoryId
                            join c in db.Classes on cat.ClassId equals c.ClassId
                            where c.Season == season &&
                                  c.SemesterYear == year
                            join course in db.Courses on c.CourseId equals course.CourseId
                            where course.Department == subject &&
                                  course.Number == num
                            select new
                            {
                                aname = assign.Name,
                                cname = cat.Name,
                                due = assign.Due,
                                submissions = assign.Submissions.Count()
                            };

                return Json(query.ToArray());
            }
            else
            {
                var query = from assign in db.Assignments
                            join cat in db.AssignmentCategories on assign.CategoryId equals cat.CategoryId
                            where cat.Name == category
                            join c in db.Classes on cat.ClassId equals c.ClassId
                            where c.Season == season &&
                                  c.SemesterYear == year
                            join course in db.Courses on c.CourseId equals course.CourseId
                            where course.Department == subject &&
                                  course.Number == num
                            select new
                            {
                                aname = assign.Name,
                                cname = cat.Name,
                                due = assign.Due,
                                submissions = assign.Submissions.Count()
                            };

                return Json(query.ToArray());
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from cat in db.AssignmentCategories
                        join c in db.Classes on cat.ClassId equals c.ClassId
                        where c.Season == season &&
                              c.SemesterYear == year
                        join course in db.Courses on c.CourseId equals course.CourseId
                        where course.Department == subject &&
                              course.Number == num
                        select new
                        {
                            name = cat.Name,
                            weight = cat.Weight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var query = from course in db.Courses
                        where course.Department == subject &&
                              course.Number == num
                        join c in db.Classes on course.CourseId equals c.CourseId
                        where c.Season == season &&
                              c.SemesterYear == year
                        select c.ClassId;

            AssignmentCategory cat = new AssignmentCategory();
            cat.Name = category;
            cat.Weight = (uint) catweight;
            cat.ClassId = query.ToArray().First();

            db.AssignmentCategories.Add(cat);

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
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            Assignment assign = new Assignment();
            assign.Name = asgname;
            assign.Points = (uint) asgpoints;
            assign.Due = asgdue;
            assign.Contents = asgcontents;
            assign.CategoryId = (from cat in db.AssignmentCategories
                                 where cat.Name == category
                                 join c in db.Classes on cat.ClassId equals c.ClassId
                                 where c.Season == season &&
                                       c.SemesterYear == year
                                 join course in db.Courses on c.CourseId equals course.CourseId
                                 where course.Department == subject &&
                                       course.Number == num
                                 select cat.CategoryId).First();

            db.Assignments.Add(assign);

            try
            {
                var query = from course in db.Courses
                            join class_ in db.Classes
                            on course.CourseId equals class_.CourseId
                            where class_.Season == season &&
                                       class_.SemesterYear == year &&
                                       course.Department == subject &&
                                       course.Number == num
                            join enroll in db.EnrollmentGrades
                            on class_.ClassId equals enroll.ClassId
                            join student in db.Students
                            on enroll.StudentNavigation.UId equals student.UId
                            select student;



                foreach (Student student in query.ToArray())
                {
                    calculateStudentGrade(student.UId);
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
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
                           on assignment.AssignmentId equals submission.AssignmentId
                        join student in db.Students
                           on submission.Student equals student.UId
                        select new
                        {
                            fname = student.FirstName,
                            lname = student.LastName,
                            uid = student.UId,
                            time = submission.SubmissionDate,
                            score = submission.Score
                        };


            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
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
                           on assignment.AssignmentId equals submission.AssignmentId
                        join student in db.Students
                           on submission.Student equals student.UId
                        where student.UId == uid
                        select submission;


            foreach (Submission submission in query)
            {
                submission.Score = (uint)score;
            }

            try
            {
                db.SaveChanges();

                calculateStudentGrade(uid);

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }

        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes
                        where c.TaughtBy == uid
                        join course in db.Courses on c.CourseId equals course.CourseId
                        select new
                        {
                            subject = course.Department,
                            number = course.Number,
                            name = course.Name,
                            season = c.Season,
                            year = c.SemesterYear
                        };

            return Json(query.ToArray());
        }

        public void calculateStudentGrade(string sId)
        {

            var query = from s in db.Students
                        join enroll in db.EnrollmentGrades
                        on s.UId equals enroll.StudentNavigation.UId
                        where s.UId == sId
                        select enroll.ClassId;




            foreach (uint classID in query.ToArray())
            {
                var query2 = from ac in db.AssignmentCategories
                             join c in db.Classes
                             on ac.ClassId equals c.ClassId
                             where ac.ClassId == classID
                             select ac;

                double totalScaled = 0;
                double catagoryWeights = 0;
                double maxPoints = 0;
                double earnedPoints = 0;



                foreach (AssignmentCategory catagory in query2.ToArray())
                {

                    maxPoints = 0;
                    earnedPoints = 0;

                    var query3 = from a in db.Assignments
                                 join ac in db.AssignmentCategories
                                 on a.CategoryId equals ac.CategoryId
                                 where ac.CategoryId == catagory.CategoryId
                                 select a;

                    if (query3.ToArray().Length == 0)
                    {
                        continue;
                    }
                    catagoryWeights += catagory.Weight;

                    foreach (Assignment assignment in query3.ToArray())
                    {
                        var query4 = from s in db.Submissions
                                     join a in db.Assignments
                                     on s.AssignmentId equals a.AssignmentId
                                     where a.AssignmentId == assignment.AssignmentId
                                     select s;
                        if (query4.ToArray().Length == 0)
                        {
                            maxPoints += assignment.Points;
                            earnedPoints += 0;
                            continue;
                        }

                        foreach (Submission submission in query4.ToArray())
                        {
                            if (submission.Student == sId)
                            {
                                maxPoints += assignment.Points;
                                earnedPoints += submission.Score;
                            }

                        }

                    }
                    double scale = (earnedPoints / maxPoints);

                    if (double.IsNaN(scale))
                    {
                        totalScaled += 0;
                        continue;
                    }

                    totalScaled += (earnedPoints / maxPoints) * catagory.Weight;

                }

                var queryx = from s in db.Students
                             join e in db.EnrollmentGrades
                             on s.UId equals e.StudentNavigation.UId
                             where s.UId == sId
                             join classs_ in db.Classes
                             on e.ClassId equals classs_.ClassId
                             where classs_.ClassId == classID
                             select e;

                foreach (EnrollmentGrade e in queryx.ToArray())
                {
                    double scaling_factor = 100 / catagoryWeights;

                    double grade = scaling_factor * totalScaled;
                    e.Grade = convertToString(grade);
                }

            }


            db.SaveChanges();

        }

        public string convertToString(double percentScore)
        {

            if (percentScore < 60)
            {
                return "E";
            }
            if (percentScore >= 60 && percentScore < 63)
            {
                return "D-";
            }
            if (percentScore >= 63 && percentScore < 67)
            {
                return "D";
            }
            if (percentScore >= 67 && percentScore < 70)
            {
                return "D+";
            }
            if (percentScore >= 70 && percentScore < 73)
            {
                return "C-";
            }
            if (percentScore >= 73 && percentScore < 77)
            {
                return "C";
            }
            if (percentScore >= 77 && percentScore < 80)
            {
                return "C+";
            }
            if (percentScore >= 80 && percentScore < 83)
            {
                return "B-";
            }
            if (percentScore >= 83 && percentScore < 87)
            {
                return "B";
            }
            if (percentScore >= 87 && percentScore < 90)
            {
                return "B+";
            }
            if (percentScore >= 90 && percentScore < 93)
            {
                return "A-";
            }
            if (percentScore >= 93 && percentScore <= 100)
            {
                return "A";
            }

            return "--";


        }

        /*******End code to modify********/
    }
}

