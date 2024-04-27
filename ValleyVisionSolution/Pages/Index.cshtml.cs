using System.Data.SqlClient;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using Microsoft.AspNetCore.Mvc;
using ValleyVisionSolution.Services;


namespace ValleyVisionSolution.Pages
{
    public class IndexModel : PageModel
    {
        public List<FileMeta> PublishedResources { get; set; } = new List<FileMeta>();
        private readonly IBlobService _blobService;
        public IndexModel(IBlobService blobService)
        {
            _blobService = blobService;
        }

        public void OnGet(string searchTerm)
        {
            HttpContext.Session.Remove("InitName");
            SqlDataReader reader = DBClass.PublishReader();
            while (reader.Read())
            {
                var file = new FileMeta
                {
                    FileMetaID = int.Parse(reader["FileMetaID"].ToString()),
                    FileName_ = reader["FileName_"].ToString(),
                    FilePath = reader["FilePath"].ToString(),
                    FileType = reader["FileType"].ToString(),
                    UploadedDateTime = DateTime.Parse(reader["UploadedDateTime"].ToString()),
                    userID = int.Parse(reader["userID"].ToString()),
                    publishdate = DateTime.Parse(reader["publishdate"].ToString()),
                };

                // If a search term is provided, only add files that match the term.
                if (string.IsNullOrEmpty(searchTerm) || file.FileName_.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    file.publishdate.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    PublishedResources.Add(file);
                }
            }
            reader.Close();
            DBClass.ValleyVisionConnection.Close();

            // Sort the list of published resources by UploadedDateTime in descending order
            PublishedResources = PublishedResources.OrderByDescending(file => file.UploadedDateTime).ToList();
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

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}





