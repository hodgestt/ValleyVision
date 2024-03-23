using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.Initiatives
{
    public class InitiativesPageModel : PageModel
    {
        public List<Initiative>? InitiativesList { get; set; }

        public InitiativesPageModel()
        {
            InitiativesList = new List<Initiative>();
        }

        public void loadData()
        {

            //get userID from session
            int? UserID = HttpContext.Session.GetInt32("UserID");

            //populate InitiativesList with initiatives that the user is a part of
            SqlDataReader reader = DBClass.InitiativesReader((int)UserID);
            while (reader.Read())
            {
                InitiativesList.Add(new Initiative
                {
                    InitID = int.Parse(reader["InitID"].ToString()),
                    InitName = reader["InitName"].ToString(),
                    InitDateTime = Convert.ToDateTime(reader["InitDateTime"])
                });
            }

            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

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

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        
    }
}
