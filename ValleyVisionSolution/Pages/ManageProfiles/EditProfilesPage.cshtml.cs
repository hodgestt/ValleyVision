using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;


namespace ValleyVisionSolution.Pages.ManageProfiles
{
    public class EditProfilesPageModel : PageModel
    {
        
        [BindProperty]
        public FullProfile ProfileToUpdate { get; set; }

        public EditProfilesPageModel()
        {
            ProfileToUpdate = new FullProfile();
        }

        public void loadData()
        {
            ProfileToUpdate = DBClass.SingleProfilesReader(HttpContext.Session.GetInt32("EditedUserID"));
        }

        public IActionResult OnGet(int userid)
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                HttpContext.Session.SetInt32("EditedUserID", userid);
                loadData();
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public IActionResult OnPostUpdateProfile()
        {
            if (ProfileToUpdate.Apartment == null)
            {
                ProfileToUpdate.Apartment = "";
            }
            DBClass.UpdateProfile(ProfileToUpdate);
            DBClass.ValleyVisionConnection.Close();
            return RedirectToPage("/ManageProfiles/ManageProfilesPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
