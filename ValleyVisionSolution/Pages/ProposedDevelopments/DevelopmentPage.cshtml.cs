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

        public DevelopmentPageModel() 
        { 

            DevFilesList = new List<FileMeta>();

        }


        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True" && DBClass.CheckUserType((int)HttpContext.Session.GetInt32("UserID")) == "Admin")
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


        }
    }
}
