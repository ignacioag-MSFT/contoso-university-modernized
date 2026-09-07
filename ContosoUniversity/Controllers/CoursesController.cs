using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AzureBlobStorageService _blobStorageService;

        public CoursesController(
            SchoolContext context,
            NotificationService notificationService,
            IWebHostEnvironment environment,
            AzureBlobStorageService blobStorageService)
            : base(context, notificationService)
        {
            _environment = environment;
            _blobStorageService = blobStorageService;
        }

        // GET: Courses
        public IActionResult Index()
        {
            var courses = db.Courses.Include(c => c.Department);
            return View(courses.ToList());
        }

        // GET: Courses/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course? course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).SingleOrDefault();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(new Course());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if an image is provided
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(teachingMaterialImage.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Please upload a valid image file (jpg, jpeg, png, gif, bmp).");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    // Validate file size (max 5MB)
                    if (teachingMaterialImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "File size must be less than 5MB.");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    try
                    {
                        // MIGRATION NOTE: Replaced local file system operations with Azure Blob Storage.
                        // Generate unique blob name with course ID and GUID to avoid collisions
                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";

                        // Upload to Azure Blob Storage
                        using (var stream = teachingMaterialImage.OpenReadStream())
                        {
                            course.TeachingMaterialImagePath = await _blobStorageService.UploadFileAsync(
                                stream,
                                fileName,
                                teachingMaterialImage.ContentType);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Error uploading file: " + ex.Message);
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                }

                db.Courses.Add(course);
                db.SaveChanges();
                
                // Send notification for course creation
                SendEntityNotification("Course", course.CourseID.ToString(), course.Title, EntityOperation.CREATE);
                
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course? course = db.Courses.Find(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Course course, IFormFile? teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if a new image is provided
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(teachingMaterialImage.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Please upload a valid image file (jpg, jpeg, png, gif, bmp).");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    // Validate file size (max 5MB)
                    if (teachingMaterialImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "File size must be less than 5MB.");
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    try
                    {
                        // MIGRATION NOTE: Replaced local file system operations with Azure Blob Storage.
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
                        {
                            await _blobStorageService.DeleteFileAsync(course.TeachingMaterialImagePath);
                        }

                        // Generate unique blob name with course ID and GUID to avoid collisions
                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";

                        // Upload to Azure Blob Storage
                        using (var stream = teachingMaterialImage.OpenReadStream())
                        {
                            course.TeachingMaterialImagePath = await _blobStorageService.UploadFileAsync(
                                stream,
                                fileName,
                                teachingMaterialImage.ContentType);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Error uploading file: " + ex.Message);
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                }

                db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();
                
                // Send notification for course update
                SendEntityNotification("Course", course.CourseID.ToString(), course.Title, EntityOperation.UPDATE);
                
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course? course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).SingleOrDefault();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = db.Courses.Find(id);
            if (course != null)
            {
                var courseTitle = course.Title;
                
                // Delete associated image file if it exists
                if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
                {
                    try
                    {
                        // MIGRATION NOTE: Replaced local file system delete with Azure Blob Storage delete.
                        // DeleteFileAsync handles non-existent blobs gracefully.
                        await _blobStorageService.DeleteFileAsync(course.TeachingMaterialImagePath);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't prevent deletion of the course
                        System.Diagnostics.Debug.WriteLine($"Error deleting file from blob storage: {ex.Message}");
                    }
                }
                
                db.Courses.Remove(course);
                db.SaveChanges();
                
                // Send notification for course deletion
                SendEntityNotification("Course", id.ToString(), courseTitle, EntityOperation.DELETE);
            }
            
            return RedirectToAction("Index");
        }
    }
}



