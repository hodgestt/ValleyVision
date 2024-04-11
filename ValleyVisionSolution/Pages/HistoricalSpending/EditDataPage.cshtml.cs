using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Bibliography;

namespace ValleyVisionSolution.Pages.HistoricalSpending
{
    public class EditDataPageModel : PageModel
    {
        [BindProperty]
        public Expenditure ExpenditureToUpdate{ get; set; }

        public bool OpenEditDataModal { get; set; } = true;

        public EditDataPageModel()
        {
            ExpenditureToUpdate = new Expenditure();
        }
        
        public void loadData()
        {
            SqlDataReader singleExpenditureYear = DBClass.singleExpenditureReader(HttpContext.Session.GetInt32("Year"));
            while (singleExpenditureYear.Read())
            {
                ExpenditureToUpdate.Year = Int32.Parse(singleExpenditureYear["year_"].ToString());
                ExpenditureToUpdate.InflationRate = Convert.ToDecimal(singleExpenditureYear["inflationRate"].ToString());
                ExpenditureToUpdate.InterestRate = Convert.ToDecimal(singleExpenditureYear["interestRate"].ToString());
                ExpenditureToUpdate.PublicSafety = Convert.ToDecimal(singleExpenditureYear["publicSafety"].ToString());
                ExpenditureToUpdate.School = Convert.ToDecimal(singleExpenditureYear["school"].ToString());
                ExpenditureToUpdate.Anomaly = Convert.ToDecimal(singleExpenditureYear["anomaly"].ToString());
                ExpenditureToUpdate.Other = Convert.ToDecimal(singleExpenditureYear["other"].ToString());

            }
            DBClass.ValleyVisionConnection.Close();
        }

        public void OnGet(int Year)
        {
            HttpContext.Session.SetInt32("Year", Year);
            loadData();
        }
        public IActionResult OnPostEditData()
        {
            int? year = HttpContext.Session.GetInt32("Year");
            DBClass.EditHistoricSpendingData(ExpenditureToUpdate, (int)year);
            DBClass.ValleyVisionConnection.Close();
            return RedirectToPage("/HistoricalSpending/ViewDataPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }



    }
}
