using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using System;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;


namespace ValleyVisionSolution.Pages.Resources
{
    public class ResourcesPageModel : PageModel
    {
        public List<FileMeta> ResourceList { get; set; }
        public List<FileMeta> FileUpload {  get; set; }
        public string CurrentInitiativeName { get; set; }

        public ResourcesPageModel() 
        {
            ResourceList = new List<FileMeta>();
            FileUpload = new List<FileMeta> { };
        }


        public void loadData(int initID, string searchTerm = null)
        {
            int? UserID = HttpContext.Session.GetInt32("UserID");

            SqlDataReader reader = DBClass.ResourceReader((int)initID);
            while (reader.Read())
            {
                ResourceList.Add(new FileMeta
                {
                    FileMetaID = int.Parse(reader["FileMetaID"].ToString()),
                    FileName_ = reader["FileName_"].ToString(),
                    FilePath = reader["FilePath"].ToString(),
                    FileType = reader["FileType"].ToString(),
                    UploadedDateTime = Convert.ToDateTime(reader["UploadedDateTime"]),
                    userID = int.Parse(reader["userID"].ToString()),
                    FirstName = reader["firstName"].ToString(),
                    LastName = reader["lastName"].ToString()
                });
            }
            DBClass.ValleyVisionConnection.Close();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                ResourceList = ResourceList.Where(file =>
                    file.FileName_.ToLower().Contains(searchTerm) ||
                    file.FileType.ToLower().Contains(searchTerm) ||
                    file.UploadedDateTime.ToString().ToLower().Contains(searchTerm) ||
                    $"{file.FirstName} {file.LastName}".ToLower().Contains(searchTerm)
                ).ToList();
            }
        }


        public IActionResult OnGet(string searchTerm)
        {
            int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                loadData(initID, searchTerm); // Pass searchTerm to loadData
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }


        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
        public async Task<IActionResult> OnPostAsync(IFormFile file)
        {
            int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
            if (file != null && file.Length > 0)
            {
                // The directory to save the files in (relative to wwwroot)
                var uploadsRelativePath = "/uploads"; // Relative path from wwwroot
                var uploadsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDirectoryPath))
                {
                    Directory.CreateDirectory(uploadsDirectoryPath);
                }

                // Generate a unique file name to avoid overwriting existing files
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;
                var filePath = Path.Combine(uploadsDirectoryPath, uniqueFileName); // For saving to disk
                var fileRelativePath = Path.Combine(uploadsRelativePath, uniqueFileName); // For saving in DB

                // Replace backslashes with forward slashes for web compatibility
                fileRelativePath = fileRelativePath.Replace('\\', '/');

                // Save the file to disk
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Create a FileMeta object with the relative path starting from wwwroot
                var fileMeta = new FileMeta
                {
                    FileName_ = fileName + fileExtension,
                    FilePath = fileRelativePath, // Save the relative path instead of the absolute path
                    FileType = fileExtension,
                    UploadedDateTime = DateTime.Now,
                    userID = HttpContext.Session.GetInt32("UserID")
                };

                // Save the file metadata to your database
                DBClass.UploadFile(initID, fileMeta);

                // Redirect to the ResourcesPage
                return RedirectToPage("./ResourcesPage");
            }

            // In case of no file selected or an error, reload the page showing an error message
            ViewData["ErrorMessage"] = "You must select a file.";
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteFileAsync(int fileId, string filePath)
        {
            // Sanitize the filePath to prevent directory traversal attacks
            var fileName = Path.GetFileName(filePath);

            // Combine the directory path with the sanitized fileName to get the absolute path
            var absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            if (System.IO.File.Exists(absoluteFilePath))
            {
                // Delete the file from the file system
                System.IO.File.Delete(absoluteFilePath);
            }

            // Now delete the record from the database
            DBClass.DeleteFile(fileId);

            // Redirect back to the page to refresh the list of files
            return RedirectToPage();
        }


 


        public class DeleteRequest
        {
            public int FileId { get; set; }
        }



    }
}
