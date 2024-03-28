using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class RevenueProjectionPageModel : PageModel
    {
        public Revenue LatestRevenue { get; set; }
        public Revenue[] ProjectedRevenues { get; set; }
        public bool DefaultLoad { get; set; }
        


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
            DefaultLoad = true;
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
            loadData();

            // Generate a random number between 0.975 and 1.025
            Random random = new Random();
            double randomNum = 0.975 + (random.NextDouble() * 0.05);
            decimal randomNumber = (decimal)randomNum;

            //list of the projected revenues
            ProjectedRevenues = new Revenue[NumProjectionYears + 1];

            ProjectedRevenues[0] = new Revenue
            {
                RealEstateTax = LatestRevenue.RealEstateTax,
                PersonalPropertyTax = LatestRevenue.PersonalPropertyTax,
                FeesLicensesTax = LatestRevenue.FeesLicensesTax,
                StateFunding = LatestRevenue.StateFunding,
                TotalRevenue = LatestRevenue.TotalRevenue,
                Year = LatestRevenue.Year
            };

            // Calculate projections for subsequent years
            for (int i = 1; i <= NumProjectionYears; i++)
            {
                ProjectedRevenues[i] = new Revenue
                {
                    RealEstateTax = Math.Round(ProjectedRevenues[i - 1].RealEstateTax * (1 + (RealEstateTaxGrowth / 100)) * randomNumber, 2),
                    PersonalPropertyTax = Math.Round(ProjectedRevenues[i - 1].PersonalPropertyTax * (1 + (PersonalPropertyTaxGrowth / 100)) * randomNumber, 2),
                    FeesLicensesTax = Math.Round(ProjectedRevenues[i - 1].FeesLicensesTax * (1 + (FeesLicensesTaxGrowth / 100)) * randomNumber, 2),
                    StateFunding = Math.Round(StateFundingAmount, 2),
                    Year = ProjectedRevenues[i - 1].Year + 1
                };

                ProjectedRevenues[i].TotalRevenue = Math.Round(
                    ProjectedRevenues[i].RealEstateTax +
                    ProjectedRevenues[i].PersonalPropertyTax +
                    ProjectedRevenues[i].FeesLicensesTax +
                    ProjectedRevenues[i].StateFunding, 2);
            }

            DefaultLoad = false;

            return Page();
        }
        public string GetChartDataAsJson()
        {
            return JsonSerializer.Serialize(ProjectedRevenues);
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
