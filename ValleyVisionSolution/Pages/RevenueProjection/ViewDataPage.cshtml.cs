using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class ViewDataPageModel : PageModel
    {
        public List<Revenue> RevenueDataList { get; set; }
        public bool OpenAddDataModal { get; set; }

        [BindProperty]
        public Revenue NewRevenueData { get; set; }
        
        
        public ViewDataPageModel()
        {
            RevenueDataList = new List<Revenue>();
            OpenAddDataModal = false; //keeps the modal open on page reload if input validations were not successful
        }

        public void loadData()
        {
            //populate FullProfilesList with FullProfiles
            SqlDataReader reader = DBClass.DataFileReader();
            while (reader.Read())
            {
                RevenueDataList.Add(new Revenue
                {
                    Year = reader["year_"].ToString(),
                    RealEstateTax = Decimal.Parse(reader["realEstateTax"].ToString()),
                    PersonalPropertyTax = Decimal.Parse(reader["personalPropertyTax"].ToString()),
                    FeesLicensesTax = Decimal.Parse(reader["feesLicensesTax"].ToString()),
                    StateFunding = Decimal.Parse(reader["stateFunding"].ToString()),
                    TotalRevenue = Decimal.Parse(reader["totalRevenue"].ToString())
                });
            }

            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
        }

        public IActionResult OnPostNewData()
        {
            if (!ModelState.IsValid)
            {
                // Model state is not valid, return the page with validation errors
                loadData();
                OpenAddDataModal = true;
                return Page();
            }
            
            // Model state is valid, continue with processing
            DBClass.AddRevenueData(NewRevenueData);
            loadData();
            ModelState.Clear();
            NewRevenueData = new Revenue();
            return Page();
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
