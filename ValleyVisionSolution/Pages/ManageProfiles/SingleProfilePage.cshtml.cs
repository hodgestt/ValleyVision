using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.ManageProfiles
{
    public class ProfilePageModel : PageModel
    {
        public FullProfile profileInformation { get; set; }

        public ProfilePageModel()
        {
            profileInformation = new FullProfile();
        }

        public void loadData()
        {
            HttpContext.Session.Remove("InitName");
            profileInformation = DBClass.SingleProfilesReader(HttpContext.Session.GetInt32("UserID"));
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                loadData();
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        //Need a get Picture Path method in DB to get it from DB
        //Need to update SQL statements and upload images into the pictures file

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
