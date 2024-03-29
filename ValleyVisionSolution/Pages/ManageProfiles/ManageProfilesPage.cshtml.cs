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
            SqlDataReader reader = DBClass.FullProfilesReader();
            while (reader.Read())
            {
                FullProfileList.Add(new FullProfile
                {
                    UserID = int.Parse(reader["UserID"].ToString()),
                    UserName = reader["UserName"].ToString(),
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    UserType = reader["UserType"].ToString(),
                    Street = reader["Street"].ToString(),
                    Apartment = reader["Apartment"].ToString(),
                    City = reader["City"].ToString(),
                    State = reader["State_"].ToString(),
                    Zip = int.Parse(reader["Zip"].ToString()),
                    Country = reader["Country"].ToString()
                });
            }

            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
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

            // Model state is valid, continue with processing
            DBClass.AddUser(NewProfile);
            loadData();
            ModelState.Clear();
            NewProfile = new FullProfile();
            return Page();
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

    }
}
