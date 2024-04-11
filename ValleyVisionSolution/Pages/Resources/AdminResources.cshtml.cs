using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using ValleyVisionSolution.Services;

namespace ValleyVisionSolution.Pages.Resources
{
    public class AdminResourcesModel : PageModel
    {
        public List<FileMeta> ResourceList { get; set; } = new List<FileMeta>();
        public List<FileMeta> FileUpload { get; set; } = new List<FileMeta> { };
        public List <Initiative> InitativeList { get; set; } = new List<Initiative>();
        public string CurrentInitiativeName { get; set; }
        public List<FileMeta> PublishedFiles { get; set; } = new List<FileMeta>();

        private readonly IBlobService _blobService;
        public AdminResourcesModel(IBlobService blobService)
        {
            _blobService = blobService;
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


        public IActionResult OnPostPublishFileAsync(int fileId)
        {
            DBClass.PublishFile(fileId);
            return RedirectToPage(); // Redirect back to the same page to refresh the list and show the updated publish status
        }

        public IActionResult OnPostUnpublishFileAsync(int fileId)
        {
            DBClass.UnPublishFile(fileId);
            return RedirectToPage();
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
