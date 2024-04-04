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

            SqlDataReader singleUser = DBClass.SingleProfilesReader(HttpContext.Session.GetInt32("EditedUserID"));
            while (singleUser.Read())
            {
                ProfileToUpdate.UserID = Int32.Parse(singleUser["UserID"].ToString());
                ProfileToUpdate.UserName = singleUser["UserName"].ToString();
                ProfileToUpdate.Password = singleUser["Password_"].ToString();
                ProfileToUpdate.FirstName = singleUser["FirstName"].ToString();
                ProfileToUpdate.LastName = singleUser["LastName"].ToString();
                ProfileToUpdate.Email = singleUser["Email"].ToString();
                ProfileToUpdate.Phone = singleUser["Phone"].ToString();
                ProfileToUpdate.UserType = singleUser["UserType"].ToString();
                ProfileToUpdate.Street = singleUser["Street"].ToString();
                ProfileToUpdate.Apartment = singleUser["Apartment"].ToString();
                ProfileToUpdate.City = singleUser["City"].ToString();
                ProfileToUpdate.State = singleUser["State_"].ToString();
                ProfileToUpdate.Zip = Int32.Parse(singleUser["Zip"].ToString());
                ProfileToUpdate.Country = singleUser["Country"].ToString();
            }
            DBClass.ValleyVisionConnection.Close();
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
