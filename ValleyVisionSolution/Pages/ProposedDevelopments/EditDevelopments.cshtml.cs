using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace ValleyVisionSolution.Pages.ProposedDevelopments
{
    public class EditDevelopmentsModel : PageModel
    {
        [BindProperty]
        public DevelopmentArea EditedDev { get; set; }
        [BindProperty]
        public List<int> EditedDevUsers { get; set; }

        public EditDevelopmentsModel()
        {
            EditedDev = new DevelopmentArea();
            EditedDevUsers = new List<int>();
        }

        public void loadData()
        {
            int? devID = HttpContext.Session.GetInt32("devID");

            //Populate AllDevs list
            SqlDataReader reader = DBClass.DevelopmentReader2((int)devID);
            while (reader.Read())
            {
                EditedDev = new DevelopmentArea
                {
                    devID = Int32.Parse(devID.ToString()),
                    devName = reader["devName"].ToString(),
                    devDescription = reader["devDescription"].ToString(),
                    devImpactLevel = reader["devImpactLevel"].ToString(),
                    uploadedDateTime = Convert.ToDateTime(reader["uploadedDateTime"]),
                    userID = Int32.Parse(reader["userID"].ToString())
                };
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
        }

        public void OnGet()
        {
            int? devID = HttpContext.Session.GetInt32("devID");
            HttpContext.Session.SetInt32("EditedDevID", (int)devID);
            loadData();
        }

        public IActionResult OnPostUpdateDev()
        {
            if (!ModelState.IsValid)
            {
                // Model state is not valid, return the page with validation errors
                loadData();
                return Page();
            }
            int? devID = HttpContext.Session.GetInt32("devIDZ");
            // Model state is valid, continue with processing
            DBClass.EditDev(EditedDev);
            return RedirectToPage("/ProposedDevelopments/ProposedDevelopmentsPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
