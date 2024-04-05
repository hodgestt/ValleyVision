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
        public void OnGet (int userid)
        {
            HttpContext.Session.SetInt32("EditedUserID", userid);
            
            loadData();
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
    }
}
