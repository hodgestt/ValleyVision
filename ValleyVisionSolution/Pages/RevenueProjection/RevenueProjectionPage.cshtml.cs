using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class RevenueProjectionPageModel : PageModel
    {
        public Revenue LatestRevenue { get; set; }

        
        [BindProperty]
        public decimal RealEstateTaxGrowth { get; set; }
        [BindProperty]
        public decimal PersonalPropertyTaxGrowth { get; set; }
        [BindProperty]
        public decimal FeesLicensesTaxGrowth { get; set; }
        [BindProperty]
        public decimal StateFundingAmount { get; set; }
        [BindProperty]
        public int NumProjectionYears { get; set; }

        public RevenueProjectionPageModel() 
        { 
            LatestRevenue = new Revenue();
        }

        public void loadData()
        {
            //Populate current year revenue data
            SqlDataReader reader = TempDBClass.LatestRevenueYearReader();
            if (reader.Read())
            {
                LatestRevenue = new Revenue
                {
                    Year = Int32.Parse(reader["Year_"].ToString()),
                    RealEstateTax = Decimal.Parse(reader["RealEstateTax"].ToString()),
                    PersonalPropertyTax = Decimal.Parse(reader["PersonalPropertyTax"].ToString()),
                    FeesLicensesTax = Decimal.Parse(reader["FeesLicensesTax"].ToString()),
                    StateFunding = Decimal.Parse(reader["StateFunding"].ToString()),
                    TotalRevenue = Decimal.Parse(reader["TotalRevenue"].ToString())
                };
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

        public IActionResult OnPostRunProjection()
        {
            loadData();
            return Page();
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
