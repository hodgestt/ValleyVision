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

        private readonly IWebHostEnvironment _environment;
        private readonly IBlobService _blobService;

        public IndexModel(IWebHostEnvironment environment, IBlobService blobService)
        {
            _environment = environment;
            _blobService = blobService;
        }

        // Now, use _environment.WebRootPath or _environment.ContentRootPath depending on your needs


        public void OnGet(string searchTerm)
        {
            HttpContext.Session.Remove("InitName");
            using (SqlDataReader reader = DBClass.PublishReader())
            {
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
                    };

                    if (string.IsNullOrEmpty(searchTerm) || file.FileName_.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        PublishedResources.Add(file);
                    }
                }
            } // The SqlDataReader is disposed here, which closes the reader and the underlying connection automatically
        }

        //public void OnGet(string searchTerm)
        //{
        //    HttpContext.Session.Remove("InitName");
        //    SqlDataReader reader = DBClass.PublishReader();
        //    while (reader.Read())
        //    {
        //        var file = new FileMeta
        //        {
        //            FileMetaID = int.Parse(reader["FileMetaID"].ToString()),
        //            FileName_ = reader["FileName_"].ToString(),
        //            FilePath = reader["FilePath"].ToString(),
        //            FileType = reader["FileType"].ToString(),
        //            UploadedDateTime = DateTime.Parse(reader["UploadedDateTime"].ToString()),
        //            userID = int.Parse(reader["userID"].ToString()),
        //        };

        //        // If a search term is provided, only add files that match the term.
        //        if (string.IsNullOrEmpty(searchTerm) || file.FileName_.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        //        {
        //            PublishedResources.Add(file);
        //        }
        //    }
        //    reader.Close();
        //    DBClass.ValleyVisionConnection.Close();

        //    // Sort the list of published resources by UploadedDateTime in descending order
        //    PublishedResources = PublishedResources.OrderByDescending(file => file.UploadedDateTime).ToList();
        //}

        //public void OnGet(string searchTerm)
        //{
        //    HttpContext.Session.Remove("InitName");
        //    SqlDataReader reader = DBClass.PublishReader();
        //    while (reader.Read())
        //    {
        //        var file = new FileMeta
        //        {
        //            FileMetaID = int.Parse(reader["FileMetaID"].ToString()),
        //            FileName_ = reader["FileName_"].ToString(),
        //            FilePath = reader["FilePath"].ToString(),
        //            FileType = reader["FileType"].ToString(),
        //            UploadedDateTime = DateTime.Parse(reader["UploadedDateTime"].ToString()),
        //            userID = int.Parse(reader["userID"].ToString()),
        //        };

        //        // If a search term is provided, only add files that match the term.
        //        if (string.IsNullOrEmpty(searchTerm) || file.FileName_.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        //        {
        //            PublishedResources.Add(file);
        //        }
        //    }
        //    reader.Close();
        //    DBClass.ValleyVisionConnection.Close();
        //}
        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnGetDownloadFileAsync(string filePath, string fileName)
        {
            try
            {
                // Use the IBlobService to download the file stream from Azure Blob Storage
                using (Stream originalStream = await _blobService.DownloadFileBlobAsync(filePath))
                {
                    if (originalStream == null)
                    {
                        return NotFound();
                    }

                    Stream fileStream;
                    // Check if the original stream is seekable
                    if (!originalStream.CanSeek)
                    {
                        // If not, copy it to a MemoryStream
                        fileStream = new MemoryStream();
                        await originalStream.CopyToAsync(fileStream);
                        // Reset the position of the MemoryStream to the beginning
                        fileStream.Position = 0;
                    }
                    else
                    {
                        // If the original stream is seekable, use it directly
                        fileStream = originalStream;
                    }

                    // Set the content type based on the file extension
                    // Consider using a more sophisticated method for determining the content type
                    string contentType = "application/octet-stream";

                    // Return the file stream to the user
                    return File(fileStream, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                // Log the error (ex) or handle accordingly
                return StatusCode(500, "Internal server error");
            }
        }



        //public async Task<IActionResult> OnGetDownloadFileAsync(string filePath, string fileName)
        //{
        //    // Assuming the 'Uploads' folder is in the root directory of your web application, not inside 'wwwroot'
        //    var fileDirectory = Path.Combine(_environment.ContentRootPath, "Uploads");

        //    var absoluteFilePath = Path.Combine(fileDirectory, filePath);

        //    if (!System.IO.File.Exists(absoluteFilePath))
        //    {
        //        return NotFound();
        //    }

        //    string contentType = "application/octet-stream";
        //    var bytes = await System.IO.File.ReadAllBytesAsync(absoluteFilePath);
        //    return File(bytes, contentType, fileName);
        //}


        //public async Task<IActionResult> OnGetDownloadFileAsync(string filePath, string fileName)
        //{
        //    var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        //    var absoluteFilePath = Path.Combine(fileDirectory, filePath);

        //    if (!System.IO.File.Exists(absoluteFilePath))
        //    {
        //        return NotFound();
        //    }

        //    string contentType = "application/octet-stream";
        //    var bytes = await System.IO.File.ReadAllBytesAsync(absoluteFilePath);
        //    return File(bytes, contentType, fileName);
        //}



    }
}




//using ValleyVisionSolution.Pages.DataClasses;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using System.ComponentModel.DataAnnotations;
//using ValleyVisionSolution.Pages.DB;
//using System.Data.SqlClient;

//namespace ValleyVisionSolution.Pages
//{
//    public class IndexModel : PageModel
//    {
//        public List<FileMeta> PublishedResources { get; set; } = new List<FileMeta>();

//        public void OnGet()
//        {
//            HttpContext.Session.Remove("InitName");
//            SqlDataReader reader = DBClass.PublishReader();
//            while (reader.Read())
//            {
//                PublishedResources.Add(new FileMeta
//                {
//                    FileMetaID = int.Parse(reader["FileMetaID"].ToString()),
//                    FileName_ = reader["FileName_"].ToString(),
//                    FilePath = reader["FilePath"].ToString(),
//                    FileType = reader["FileType"].ToString(),
//                    UploadedDateTime = DateTime.Parse(reader["UploadedDateTime"].ToString()),
//                    userID = int.Parse(reader["userID"].ToString()),
//                });
//            }
//            reader.Close();
//            DBClass.ValleyVisionConnection.Close();
//        }

//        public IActionResult OnPostLogoutHandler()
//        {
//            HttpContext.Session.Clear();
//            return RedirectToPage("/Index");
//        }
//    }
//}
