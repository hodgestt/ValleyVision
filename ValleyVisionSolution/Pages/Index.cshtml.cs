using System.Data.SqlClient;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using Microsoft.AspNetCore.Mvc;


namespace ValleyVisionSolution.Pages
{
    public class IndexModel : PageModel
    {
        public List<FileMeta> PublishedResources { get; set; } = new List<FileMeta>();

        private readonly IWebHostEnvironment _environment;

        public IndexModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // Now, use _environment.WebRootPath or _environment.ContentRootPath depending on your needs


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
                };

                // If a search term is provided, only add files that match the term.
                if (string.IsNullOrEmpty(searchTerm) || file.FileName_.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    PublishedResources.Add(file);
                }
            }
            reader.Close();
            DBClass.ValleyVisionConnection.Close();

            // Sort the list of published resources by UploadedDateTime in descending order
            PublishedResources = PublishedResources.OrderByDescending(file => file.UploadedDateTime).ToList();
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
        //}
        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }



        public async Task<IActionResult> OnGetDownloadFileAsync(string filePath, string fileName)
        {
            // Assuming the 'Uploads' folder is in the root directory of your web application, not inside 'wwwroot'
            var fileDirectory = Path.Combine(_environment.ContentRootPath, "Uploads");

            var absoluteFilePath = Path.Combine(fileDirectory, filePath);

            if (!System.IO.File.Exists(absoluteFilePath))
            {
                return NotFound();
            }

            string contentType = "application/octet-stream";
            var bytes = await System.IO.File.ReadAllBytesAsync(absoluteFilePath);
            return File(bytes, contentType, fileName);
        }


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
