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
        public List<FullProfile> AllProfiles { get; set; }
        public int? ViewedProfile { get; set; }

        [BindProperty]
        public FullProfile ProfileToUpdate { get; set; }

        public EditProfilesPageModel()
        {
            ProfileToUpdate = new FullProfile();
            AllProfiles = new List<FullProfile>();
        }

        public void OnGet (int userid)
        {
            HttpContext.Session.SetInt32("UserID", userid);
            ViewedProfile = HttpContext.Session.GetInt32("UserID"); 


            SqlDataReader singleUser= DBClass.SingleProfilesReader(userid);
                while (singleUser.Read())
                {
                    ProfileToUpdate.UserID = userid;
                    ProfileToUpdate.UserName = singleUser["UserName"].ToString();
                    ProfileToUpdate.Password = singleUser["Password"].ToString();
                    ProfileToUpdate.FirstName = singleUser["FirstName"].ToString();
                    ProfileToUpdate.LastName = singleUser["LastName"].ToString();
                    ProfileToUpdate.Email = singleUser["Email"].ToString();
                    ProfileToUpdate.Phone = singleUser["Phone"].ToString();
                    ProfileToUpdate.Street = singleUser["Street"].ToString();
                    ProfileToUpdate.Apartment = singleUser["Apartment"].ToString();
                    ProfileToUpdate.City = singleUser["City"].ToString();
                    ProfileToUpdate.State = singleUser["State"].ToString();
                    ProfileToUpdate.Zip = Int32.Parse(singleUser["Zip"].ToString());
                    ProfileToUpdate.Country = singleUser["UserName"].ToString();
                }
                DBClass.ValleyVisionConnection.Close();

            //foreach (var profile in AllProfiles)
            //{
            //    if (profile.UserID == ViewedProfile)
            //    {
            //        UserName = profile.UserName;
            //    }
            //}
        }

       
        public IActionResult OnPost()
        {
            DBClass.UpdateProfile(ProfileToUpdate);
            DBClass.ValleyVisionConnection.Close();
            return RedirectToPage("Index");
        }
    }
}
