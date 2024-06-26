using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.ProposedDev
{
    public class ProposedDevPageModel : PageModel
    {
        public List<DevelopmentArea> HighImpact { get; set; }
        public List<DevelopmentArea> MidImpact { get; set; }
        public List<DevelopmentArea> LowImpact { get; set; }
        [BindProperty]
        public DevelopmentArea NewDevelopmentArea { get; set; }

        public bool openaddProposedDevelopment { get; set; }

        public ProposedDevPageModel() 
        {
            HighImpact = new List<DevelopmentArea>();
            MidImpact = new List<DevelopmentArea>();
            LowImpact = new List<DevelopmentArea>();
            openaddProposedDevelopment = false;
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

        public void loadData()
        {
            try
            {
                using (var reader = DBClass.DevelopmentReader())
                {
                    while (reader.Read())
                    {
                        var development = new DevelopmentArea
                        {
                            devID = int.Parse(reader["devID"].ToString()),
                            devName = reader["devName"].ToString(),
                            devDescription = reader["devDescription"].ToString(),
                            devImpactLevel = reader["devImpactLevel"].ToString(), // Fixed this line
                            uploadedDateTime = Convert.ToDateTime(reader["uploadedDateTime"]),
                            userID = int.Parse(reader["userID"].ToString())
                            
                        };

                        switch (development.devImpactLevel)
                        {
                            case "High":
                                HighImpact.Add(development);
                                break;
                            case "Medium":
                                MidImpact.Add(development);
                                break;
                            case "Low":
                                LowImpact.Add(development);
                                break;
                        }
                    }
                } // Using block ensures the reader is closed even if an exception occurs
            }
            catch (Exception ex)
            {
                // Log the exception details
                // Consider showing an error message to the user if appropriate
            }
            finally
            {
                DBClass.ValleyVisionConnection.Close();
            }

        }

        public IActionResult OnPostNewDevelopmentArea()
        {
            if (!ModelState.IsValid)
            {
                // Model state is not valid, return the page with validation errors
                openaddProposedDevelopment = true;
                loadData();
                return Page();
            }

            DBClass.AddDevelopmentArea(NewDevelopmentArea, HttpContext.Session.GetInt32("UserID"));
            loadData();
            ModelState.Clear();

            NewDevelopmentArea = new DevelopmentArea();

            return Page();
        }

        public IActionResult OnPostViewDetails(int devID)
        {
            HttpContext.Session.SetInt32("devID", devID);

            // Redirect to the DevelopmentPage
            return RedirectToPage("/ProposedDevelopments/DevelopmentPage");
        }

        public IActionResult OnPostEdit(int devID) 
        {
            HttpContext.Session.SetInt32("devID", devID);
            return RedirectToPage("/ProposedDevelopments/EditDevelopments");
        }


        public IActionResult OnPostDelete(int devid)
        {
            DBClass.DeleteDevelopmentArea(devid);
            return RedirectToPage("/ProposedDevelopments/ProposedDevelopmentsPage");
        }


        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

    }
}
