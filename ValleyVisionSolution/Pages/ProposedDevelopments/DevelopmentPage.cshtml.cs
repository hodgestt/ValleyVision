using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
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

        [BindProperty]
        public List<int> NewDevelopmentFiles { get; set; }

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

            SqlDataReader reader3 = DBClass.DevDetailsReader((int)devID);
            while (reader3.Read())
            {
                DevFilesList.Add(new FileMeta
                {
                    FileMetaID = int.Parse(reader3["fileMetaID"].ToString()),
                    FileName_ = reader3["fileName_"].ToString(),
                    FilePath = reader3["filePath"].ToString(),
                    FileType = reader3["fileType"].ToString(),
                    UploadedDateTime = Convert.ToDateTime(reader3["uploadedDateTime"]),
                    FirstName = reader3["firstName"].ToString(),
                    LastName = reader3["lastName"].ToString()
                });
            }
            DBClass.ValleyVisionConnection.Close();

            
            
        }

        

    }
}
