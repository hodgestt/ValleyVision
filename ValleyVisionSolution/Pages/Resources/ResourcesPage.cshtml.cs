using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.IO;
using System.Threading.Tasks;
using ValleyVisionSolution.Services;
using System.Collections.Generic;
using System.Linq;

namespace ValleyVisionSolution.Pages.Resources
{
    public class ResourcesPageModel : PageModel
    {
        public List<FileMeta> ResourceList { get; set; } = new List<FileMeta>();
        public List<FileMeta> FileUpload { get; set; } = new List<FileMeta>();
        public string CurrentInitiativeName { get; set; }

        private readonly IBlobService _blobService;

        // Modified to include IBlobService through Dependency Injection
        public ResourcesPageModel(IBlobService blobService)
        {
            _blobService = blobService;
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
                // Generate a unique file name to avoid overwriting existing files
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;

                // Use the IBlobService to upload the file
                using (var fileStream = file.OpenReadStream())
                {
                    await _blobService.UploadFileBlobAsync(uniqueFileName, fileStream, file.ContentType);
                }

                // Prepare the file metadata
                var fileMeta = new FileMeta
                {
                    FileName_ = fileName + fileExtension,
                    FilePath = uniqueFileName,
                    FileType = fileExtension,
                    UploadedDateTime = DateTime.Now,
                    userID = HttpContext.Session.GetInt32("UserID")
                };

                // Update the database with the file metadata
                DBClass.UploadFile(initID, fileMeta);

                return RedirectToPage("./ResourcesPage");
            }

            ViewData["ErrorMessage"] = "You must select a file.";
            return Page();
        }



        public async Task<IActionResult> OnGetDownloadFileAsync(string filePath, string fileName)
        {
            // Assuming 'filePath' is the unique blob name used when the file was uploaded
            Stream blobStream = await _blobService.DownloadFileBlobAsync(filePath);

            if (blobStream == null)
            {
                return NotFound();
            }

            // Determine the content type
            // You might want to have a better way to determine the content type based on the file extension
            string contentType = "application/octet-stream";

            // Return the file to the user
            // 'fileName' is the original file name that the user uploaded
            return File(blobStream, contentType, fileName);
        }


        public async Task<IActionResult> OnPostDeleteFileAsync(int fileId, string filePath)
        {
            // Since 'filePath' should contain the unique blob name,
            // use the IBlobService to delete the file from Azure Blob Storage
            await _blobService.DeleteFileBlobAsync(filePath);

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
