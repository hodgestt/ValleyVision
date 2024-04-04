using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.ProposedDevelopments
{
    public class DevelopmentPageModel : PageModel
    {
        public List<FileMeta> DevFilesList { get; set; }
        public string DevName { get; set; }
        public string DevDescription { get; set; }
        public string DevImpactLevel { get; set; }
        public DateTime UploadTime { get; set; }
        public List<FileMeta> EconFileList { get; set; }

        public DevelopmentPageModel() 
        { 

            DevFilesList = new List<FileMeta>();
            EconFileList = new List<FileMeta>();

        }


        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True" )
            {
                loadData();
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public void loadData()
        {
            int? devID = HttpContext.Session.GetInt32("devID");

            SqlDataReader reader2 = DBClass.DevelopmentReader2((int)devID);
            while (reader2.Read())
            {
                DevName = reader2["devName"].ToString();
                DevDescription = reader2["devDescription"].ToString() ;
                DevImpactLevel = reader2["devImpactLevel"].ToString();
                UploadTime = Convert.ToDateTime(reader2["uploadedDateTime"]);

            }
            DBClass.ValleyVisionConnection.Close();



            SqlDataReader reader = DBClass.DevDetailsReader((int)devID);
            while (reader.Read())
            {
                DevFilesList.Add(new FileMeta
                {
                    FileMetaID = int.Parse(reader["FileMetaID"].ToString()),
                    FileName_ = reader["FileName_"].ToString(),
                    FilePath = reader["FilePath"].ToString(),
                    FileType = reader["FileType"].ToString(),
                    UploadedDateTime = Convert.ToDateTime(reader["UploadedDateTime"]),
                    FirstName = reader["firstName"].ToString(),
                    LastName = reader["lastName"].ToString()
                });
            }
            DBClass.ValleyVisionConnection.Close();

            //SqlDataReader reader3 = DBClass.ResourceReader(2);
            //while (reader3.Read())
            //{

            //}
        }
    }
}
