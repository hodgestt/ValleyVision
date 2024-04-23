using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.ManageProfiles
{
    public class ManageProfilesPageModel : PageModel
    {
        public List<FullProfile> FullProfileList {get ;set;}
        public bool OpenAddProfileModal { get; set; }

        [BindProperty]
        public FullProfile NewProfile { get; set; }
       

        public ManageProfilesPageModel()
        {
            FullProfileList = new List<FullProfile>();
            OpenAddProfileModal = false; //keeps the modal open on page reload if input validations were not successful
        }


        public void loadData() 
        {
            //populate FullProfilesList with FullProfiles
            FullProfileList = DBClass.FullProfilesReader();
            
        }


        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True" && DBClass.CheckUserType((int)HttpContext.Session.GetInt32("UserID")) == "Admin")
            {
                HttpContext.Session.Remove("InitName");
                loadData();
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }

        }

        public IActionResult OnPostNewProfile()
        {
            if (!ModelState.IsValid)
            {
                // Model state is not valid, return the page with validation errors
                loadData();
                OpenAddProfileModal = true;
                return Page();
            }
            if (NewProfile.Apartment == null)
            {
                NewProfile.Apartment = "";
            }

            DBClass.AddUser(NewProfile);
            loadData();
            ModelState.Clear();
            NewProfile = new FullProfile();

            return Page();
        }

        public IActionResult OnPostDeleteProfile(int userid)
        {
            DBClass.DeleteUser(userid);
            return RedirectToPage("/ManageProfiles/ManageProfilesPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostPopulateHandler()
        {
            ModelState.Clear();
            NewProfile.UserName = "JamesMad";
            NewProfile.Password = "1234!";
            NewProfile.FirstName = "James";
            NewProfile.LastName = "Madison";
            NewProfile.Email = "maddy@dukes.edu";
            NewProfile.Phone = "5034567980";
            NewProfile.Street = "MLK Way";
            NewProfile.Apartment = "";
            NewProfile.City = "Harrisonburg";
            NewProfile.State = "VA";
            NewProfile.Zip = 22801;
            NewProfile.Country = "U.S.A";

            OpenAddProfileModal = true;

            loadData();


            return Page();
        }

        //public IActionResult OnPostPopulateHandler()
        //{
        //    ModelState.Clear();
        //    PrepareDefaultProfileData();
        //    OpenAddProfileModal = true;
        //    loadData();
        //    return Page();
        //}

        //private void PrepareDefaultProfileData()
        //{
        //    NewProfile.UserName = "JamesMad";
        //    NewProfile.Password = "1234!";
        //    NewProfile.FirstName = "James";
        //    NewProfile.LastName = "Madison";
        //    NewProfile.Email = "maddy@dukes.edu";
        //    NewProfile.Phone = "5034567980";
        //    NewProfile.Street = "MLK Way";
        //    NewProfile.Apartment = "";
        //    NewProfile.City = "Harrisonburg";
        //    NewProfile.State = "VA";
        //    NewProfile.Zip = 22801;
        //    NewProfile.Country = "U.S.A";
        //}


    }
}
