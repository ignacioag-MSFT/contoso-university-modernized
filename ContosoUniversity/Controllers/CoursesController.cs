using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly TeachingMaterialBlobStorageService teachingMaterialBlobStorageService;

        public CoursesController(SchoolContext context, NotificationService notifications, TeachingMaterialBlobStorageService blobStorageService)
            : base(context, notifications)
        {
            teachingMaterialBlobStorageService = blobStorageService;
        }

        // GET: Courses
        public ActionResult Index()
        {
            var courses = db.Courses.Include(c => c.Department);
            return View(courses.ToList());
        }

        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).Single();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // GET: Courses/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(new Course());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile teachingMaterialImage)
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
                        // Generate unique filename
                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";

                        // Save file to Azure Blob Storage
                        course.TeachingMaterialImagePath = await teachingMaterialBlobStorageService.UploadAsync(teachingMaterialImage, fileName);
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
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = db.Courses.Find(id);
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
        public async Task<ActionResult> Edit([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile teachingMaterialImage)
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
                        // Generate unique filename
                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";

                        // Delete old blob if exists
                        if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
                        {
                            await teachingMaterialBlobStorageService.DeleteIfExistsAsync(course.TeachingMaterialImagePath);
                        }

                        // Save new file to Azure Blob Storage
                        course.TeachingMaterialImagePath = await teachingMaterialBlobStorageService.UploadAsync(teachingMaterialImage, fileName);
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
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).Single();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Course course = db.Courses.Find(id);
            var courseTitle = course.Title;
            
            // Delete associated image blob if it exists
            if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
            {
                try
                {
                    await teachingMaterialBlobStorageService.DeleteIfExistsAsync(course.TeachingMaterialImagePath);
                }
                catch (Exception ex)
                {
                    // Log the error but don't prevent deletion of the course
                    // In a production application, you would log this error properly
                    System.Diagnostics.Debug.WriteLine($"Error deleting blob: {ex.Message}");
                }
            }
            
            db.Courses.Remove(course);
            db.SaveChanges();
            
            // Send notification for course deletion
            SendEntityNotification("Course", id.ToString(), courseTitle, EntityOperation.DELETE);
            
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> TeachingMaterial(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return NotFound();
            }

            var teachingMaterial = await teachingMaterialBlobStorageService.DownloadAsync(blobName);
            if (teachingMaterial == null)
            {
                return NotFound();
            }

            return File(teachingMaterial.Content, teachingMaterial.ContentType);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Base class will dispose db and notificationService
            }
            base.Dispose(disposing);
        }
    }
}
