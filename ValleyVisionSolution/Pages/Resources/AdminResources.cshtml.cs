using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.Resources
{
    public class AdminResourcesModel : PageModel
    {
        public List<FileMeta> ResourceList { get; set; }
        public List<FileMeta> FileUpload { get; set; }
        public List <Initiative> InitativeList { get; set; }
        public string CurrentInitiativeName { get; set; }
        public List<FileMeta> PublishedFiles { get; set; } = new List<FileMeta>();


        public AdminResourcesModel()
        {
            ResourceList = new List<FileMeta>();
            FileUpload = new List<FileMeta> { };
            InitativeList = new List<Initiative>();
        }


        public void loadData(string searchTerm = null)
        {
            int? UserID = HttpContext.Session.GetInt32("UserID");

            SqlDataReader reader2 = DBClass.InitiativesReader((int)UserID);
            while (reader2.Read())
            {
                InitativeList.Add(new Initiative
                {
                    InitID = int.Parse(reader2["initID"].ToString()),
                    InitName = reader2["initName"].ToString()
                });
            }
            DBClass.ValleyVisionConnection.Close();

            foreach (Initiative init in InitativeList)
            {
                using (var reader = DBClass.ResourceReader((int)init.InitID))
                {
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
                            LastName = reader["lastName"].ToString(),
                            published = reader["published"].ToString(),
                            InitiativeName = init.InitName
                        });
                    }
                    DBClass.ValleyVisionConnection.Close();
                } 
            }



            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                ResourceList = ResourceList.Where(file =>
                    file.FileName_.ToLower().Contains(searchTerm) ||
                    file.FileType.ToLower().Contains(searchTerm) ||
                    file.UploadedDateTime.ToString().ToLower().Contains(searchTerm) ||
                    $"{file.FirstName} {file.LastName}".ToLower().Contains(searchTerm) ||
                    file.InitiativeName.ToLower().Contains(searchTerm)
                ).ToList();
            }
        }


        public IActionResult OnGet(string searchTerm)
        {
            HttpContext.Session.Remove("InitName");
            int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                loadData(searchTerm); // Pass searchTerm to loadData
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

        public IActionResult OnPostPublishFileAsync(int fileId)
        {
            DBClass.PublishFile(fileId);
            return RedirectToPage(); // Redirect back to the same page to refresh the list and show the updated publish status
        }

        public async Task<IActionResult> OnGetDownloadFileAsync(string filePath, string fileName)
        {
            var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            var absoluteFilePath = Path.Combine(fileDirectory, filePath);

            if (!System.IO.File.Exists(absoluteFilePath))
            {
                return NotFound();
            }

            string contentType = "application/octet-stream";
            var bytes = await System.IO.File.ReadAllBytesAsync(absoluteFilePath);
            return File(bytes, contentType, fileName);
        }


        public class DeleteRequest
        {
            public int FileId { get; set; }
        }



    }
}
