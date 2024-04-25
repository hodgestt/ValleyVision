using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.Login
{
    public class LoginPageModel : PageModel
    {

        //to store user inputs
        [BindProperty]
        [Required]
        public HashedCredential UserCredentials { get; set; }

        //to communicate with user
        public string LoginMessage { get; set; }
        public string LogoutMessage { get; set; }


        public void OnGet()
        {
            //when user logs out they return to this page, which will show message saying logout was succesfull
            if (HttpContext.Session.GetString("LoggedIn") == "False")
            {
                //LogoutMessage = "Logout was succesfull!";
            }
        }

        public IActionResult OnPostPopulateHandler(string option)
        {
            ModelState.Clear();
            // Populate the username and password fields based on the selected option
            switch (option)
            {
                case "option1":
                    UserCredentials.Username = "Ryan";
                    UserCredentials.Password = "Pass";
                    break;
                case "option2":
                    UserCredentials.Username = "Timothy";
                    UserCredentials.Password = "Pass";
                    break;
                default:
                    break;
            }

            DBClass.ValleyVisionConnection.Close();

            // Return to page
            return Page();
        }

        public IActionResult OnPostLoginHandler()
        {
            //checks if all inputs are filled in then invokes function from DBClass to check if credentials are right
            if (ModelState.IsValid)
            {
                if (DBClass.HashedParameterLogin(UserCredentials) != -1)
                {
                    UserCredentials.UserID = (DBClass.HashedParameterLogin(UserCredentials));
                    HttpContext.Session.SetInt32("UserID", UserCredentials.UserID);
                    HttpContext.Session.SetString("LoggedIn", "True");
                    HttpContext.Session.SetString("Username", UserCredentials.Username);

                    return RedirectToPage("/Initiatives/InitiativesPage");
                }
            }

            LoginMessage = "Login was unsuccessful!";
            return Page();

        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.SetInt32("UserID", -1);
            HttpContext.Session.SetString("LoggedIn", "False");
            return RedirectToPage("/Index");
        }
    }
}
