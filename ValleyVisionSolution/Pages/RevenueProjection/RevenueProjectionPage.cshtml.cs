using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
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
            SqlDataReader reader = DBClass.LatestRevenueYearReader();
            if (reader.Read())
            {
                LatestRevenue = new Revenue
                {
                    Year = int.Parse(reader["Year_"].ToString()),
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
            // Generate a random number between 0.95 and 1
            Random random = new Random();
            double randomNum = 0.95 + (random.NextDouble() * 0.05);
            decimal randomNumber = (decimal)randomNum;

            //list of the projected revenues
            Revenue[] ProjectedRevenues = new Revenue[NumProjectionYears];

            for (int i = 0; i < NumProjectionYears; i++)
            {
                ProjectedRevenues[i] = new Revenue
                {
                    RealEstateTax = LatestRevenue.RealEstateTax * (1 + (RealEstateTaxGrowth / 100)) * randomNumber,
                    PersonalPropertyTax = LatestRevenue.PersonalPropertyTax * (1 + (PersonalPropertyTaxGrowth / 100)) * randomNumber,
                    FeesLicensesTax = LatestRevenue.FeesLicensesTax * (1 + (FeesLicensesTaxGrowth / 100)) * randomNumber,
                    StateFunding = StateFundingAmount
                };
                ProjectedRevenues[i].TotalRevenue = ProjectedRevenues[i].RealEstateTax +
                                                    ProjectedRevenues[i].PersonalPropertyTax +
                                                    ProjectedRevenues[i].FeesLicensesTax +
                                                    ProjectedRevenues[i].StateFunding;
            }

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
