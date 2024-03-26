using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using System;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.Resources
{
    public class ResourcesPageModel : PageModel
    {
        public List<FileMeta> ResourceList { get; set; }

        public ResourcesPageModel() 
        {
            ResourceList = new List<FileMeta>();
        }

        public void loadData(int initID)
        {
            //get userID from session
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

        }

        public IActionResult OnGet()
        {
            int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                loadData(initID);
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
    }
}
