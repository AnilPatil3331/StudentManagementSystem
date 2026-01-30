using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using System.Web;


namespace StudentManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

       
        // ------------------- INDEX -------------------
        public ActionResult Index(string search, string course, string sortOrder, int page = 1)
        {
            int pageSize = 5;

        
            var students = _studentService.GetStudents(search, course).AsQueryable();

          
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParam = sortOrder == "Date" ? "date_desc" : "Date";

            switch (sortOrder)
            {
                case "name_desc": students = students.OrderByDescending(s => s.Name); break;
                case "Date": students = students.OrderBy(s => s.DateOfBirth); break;
                case "date_desc": students = students.OrderByDescending(s => s.DateOfBirth); break;
                default:
                   
                    students = students.OrderBy(s => s.Id);
                    break;
            }

            var studentList = students.ToList();
            int totalRecords = studentList.Count;

            var pagedData = studentList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.Search = search;
            ViewBag.Course = course;
            ViewBag.SortOrder = sortOrder;

            ViewBag.Courses = Enum.GetValues(typeof(Course)).Cast<Course>()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString(),
                    Selected = (course == c.ToString())
                }).ToList();

            return View(pagedData);
        }
        // ------------------- CREATE -------------------
        public ActionResult Create()
        {
            ViewBag.Courses = Enum.GetValues(typeof(Course))
                                  .Cast<Course>()
                                  .Select(c => new SelectListItem
                                  {
                                      Value = c.ToString(),
                                      Text = c.ToString()
                                  }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student)
        {
            if (_studentService.GetStudents(null, null).Any(x => x.Email == student.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (ModelState.IsValid)
            {
                _studentService.Create(student);
                TempData["Success"] = "Student created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Courses = Enum.GetValues(typeof(Course))
                                  .Cast<Course>()
                                  .Select(c => new SelectListItem
                                  {
                                      Value = c.ToString(),
                                      Text = c.ToString()
                                  }).ToList();
            return View(student);
        }

        // ------------------- EDIT -------------------
        public ActionResult Edit(int id)
        {
            var student = _studentService.Get(id);
            if (student == null)
                return HttpNotFound();

            ViewBag.Courses = Enum.GetValues(typeof(Course))
                                  .Cast<Course>()
                                  .Select(c => new SelectListItem
                                  {
                                      Value = c.ToString(),
                                      Text = c.ToString(),
                                      Selected = (student.Course == c)
                                  }).ToList();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
       public ActionResult Edit(Student student)
        {
            if (_studentService.GetStudents(null, null)
                               .Any(x => x.Email == student.Email && x.Id != student.Id))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (ModelState.IsValid)
            {
                _studentService.Edit(student);
                TempData["Success"] = "Student updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Courses = Enum.GetValues(typeof(Course))
                                  .Cast<Course>()
                                  .Select(c => new SelectListItem
                                  {
                                      Value = c.ToString(),
                                      Text = c.ToString(),
                                      Selected = (student.Course == c)
                                  }).ToList();
            return View(student);
        }

        // ------------------- DELETE -------------------
        public ActionResult Delete(int id)
        {
            var student = _studentService.Get(id);
            if (student == null)
                return HttpNotFound();

            _studentService.Remove(id);
            TempData["Success"] = "Student deleted successfully!";
            return RedirectToAction("Index");
        }


        
        // ------------------- EXPORT TO EXCEL -------------------
        public ActionResult Export(string search, string course, string sortOrder)
        {
            var studentsQuery = _studentService.GetStudents(search, course).AsQueryable();

            switch (sortOrder)
            {
                case "name_desc": studentsQuery = studentsQuery.OrderByDescending(s => s.Name); break;
                case "Date": studentsQuery = studentsQuery.OrderBy(s => s.DateOfBirth); break;
                case "date_desc": studentsQuery = studentsQuery.OrderByDescending(s => s.DateOfBirth); break;
                default:
                  
                    studentsQuery = studentsQuery.OrderBy(s => s.Id);
                    break;
            }

            var students = studentsQuery.ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Students");

                // Headers
                string[] headers = { "Name", "Email", "Date of Birth", "Course", "Mobile Number" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[1, i + 1].Value = headers[i];
                }

                // Header Style
                using (var range = ws.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Data Fill
                int row = 2;
                foreach (var s in students)
                {
                    ws.Cells[row, 1].Value = s.Name;
                    ws.Cells[row, 2].Value = s.Email;
                    ws.Cells[row, 3].Value = s.DateOfBirth.ToString("yyyy-MM-dd");
                    ws.Cells[row, 4].Value = s.Course.ToString();
                    ws.Cells[row, 5].Value = s.MobileNumber;
                    row++;
                }

                
                var courseValidation = ws.DataValidations.AddListValidation(ws.Cells[2, 4, 1000, 4].Address);
                courseValidation.Error = "Please select a course from the list.";
                courseValidation.ErrorTitle = "Invalid Course";
                courseValidation.ShowErrorMessage = true;

                foreach (var c in Enum.GetNames(typeof(Course)))
                {
                    courseValidation.Formula.Values.Add(c);
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                var fileContents = package.GetAsByteArray();

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentsList.xlsx");
            }
        }

        // ------------------- IMPORT FROM EXCEL (No ID expected) -------------------
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return RedirectToAction("Index");
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var ws = package.Workbook.Worksheets.FirstOrDefault();
                    if (ws == null || ws.Dimension == null)
                    {
                        TempData["Error"] = "The Excel file is empty.";
                        return RedirectToAction("Index");
                    }

                    int successCount = 0;
                    int skippedCount = 0;
                    int rows = ws.Dimension.Rows;

                    for (int r = 2; r <= rows; r++)
                    {
                        // Column Index: 1:Name, 2:Email, 3:DOB, 4:Course, 5:Mobile
                        string name = ws.Cells[r, 1].Text.Trim();
                        string email = ws.Cells[r, 2].Text.Trim();

                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                        {
                            skippedCount++;
                            continue;
                        }

                        // Duplicate Email Check
                        if (_studentService.GetStudents(null, null).Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                        {
                            skippedCount++;
                            continue;
                        }

                        // Course Parsing
                        string courseText = ws.Cells[r, 4].Text.Trim();
                        if (!Enum.TryParse(courseText, true, out Course parsedCourse))
                        {
                            parsedCourse = Course.BSc; 
                        }

                        var student = new Student
                        {
                            Name = name,
                            Email = email,
                            DateOfBirth = DateTime.TryParse(ws.Cells[r, 3].Text.Trim(), out DateTime dt) ? dt : DateTime.Now,
                            Course = parsedCourse,
                            MobileNumber = ws.Cells[r, 5].Text.Trim()
                        };

                        _studentService.Create(student);
                        successCount++;
                    }

                    TempData["Success"] = $"Import success! Added: {successCount}, Skipped: {skippedCount}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
