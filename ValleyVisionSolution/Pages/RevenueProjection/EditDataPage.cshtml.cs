using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class EditDataPageModel : PageModel
    {

        [BindProperty]
        public Revenue RevenueToUpdate { get; set; }

        public EditDataPageModel()
        {
            RevenueToUpdate = new Revenue();
        }

        public void loadData()
        {
            SqlDataReader singleRevenueYear = DBClass.SingleRevenueDataReader(HttpContext.Session.GetInt32("Year"));
            while (singleRevenueYear.Read())
            {
                RevenueToUpdate.Year = Int32.Parse(singleRevenueYear["year_"].ToString());
                RevenueToUpdate.RealEstateTax = Convert.ToDecimal(singleRevenueYear["realEstateTax"].ToString());
                RevenueToUpdate.PersonalPropertyTax = Convert.ToDecimal(singleRevenueYear["personalPropertyTax"].ToString());
                RevenueToUpdate.FeesLicensesTax = Convert.ToDecimal(singleRevenueYear["feesLicensesTax"].ToString());
                RevenueToUpdate.StateFunding = Convert.ToDecimal(singleRevenueYear["otherRevenue"].ToString());

            }
            DBClass.ValleyVisionConnection.Close();

        }

        public void OnGet(int Year)
        {
            HttpContext.Session.SetInt32("Year", Year);
            loadData();
        }

        public IActionResult OnPost()
        {
            DBClass.EditRevenueData(RevenueToUpdate);
            DBClass.ValleyVisionConnection.Close();
            return RedirectToPage("/RevenueProjection/ViewDataPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

    }
}
