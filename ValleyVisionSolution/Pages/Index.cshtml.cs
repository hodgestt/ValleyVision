using ValleyVisionSolution.Pages.DataClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ValleyVisionSolution.Pages.DB;
using System.Data.SqlClient;

namespace ValleyVisionSolution.Pages
{
    public class IndexModel : PageModel
    {
        public List<FileMeta> PublishedResources { get; set; } = new List<FileMeta>();

        public void OnGet()
        {
            HttpContext.Session.Remove("InitName");
            SqlDataReader reader = DBClass.PublishReader();
            while (reader.Read())
            {
                PublishedResources.Add(new FileMeta
                {
                    FileMetaID = int.Parse(reader["FileMetaID"].ToString()),
                    FileName_ = reader["FileName_"].ToString(),
                    FilePath = reader["FilePath"].ToString(),
                    FileType = reader["FileType"].ToString(),
                    UploadedDateTime = DateTime.Parse(reader["UploadedDateTime"].ToString()),
                    userID = int.Parse(reader["userID"].ToString()),
                });
            }
            reader.Close();
            DBClass.ValleyVisionConnection.Close();
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
