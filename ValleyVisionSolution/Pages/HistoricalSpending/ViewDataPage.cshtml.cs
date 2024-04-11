using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.HistoricalSpending
{
    public class ViewDataPageModel : PageModel
    {

        public List<Expenditure> HistoricalExpenditureDataList { get; set; }
        public bool OpenAddDataModal { get; set; }

        [BindProperty]
        public Expenditure NewHistoricalSpendingData { get; set; }


        public ViewDataPageModel()
        {
            HistoricalExpenditureDataList = new List<Expenditure>();
            OpenAddDataModal = false; //keeps the modal open on page reload if input validations were not successful
        }

        public void loadData()
        {
            SqlDataReader reader = DBClass.HistoricalExpendituresReader();
            while (reader.Read())
            {

                HistoricalExpenditureDataList.Add(new Expenditure
                {
                    Year = int.Parse(reader["Year_"].ToString()),
                    InflationRate = Decimal.Parse(reader["InflationRate"].ToString()),
                    InterestRate = Decimal.Parse(reader["InterestRate"].ToString()),
                    PublicSafety = Decimal.Parse(reader["PublicSafety"].ToString()),
                    School = Decimal.Parse(reader["School"].ToString()),
                    Anomaly = Decimal.Parse(reader["Anomaly"].ToString()),
                    Other = Decimal.Parse(reader["Other"].ToString()),
                    TotalExpenditure = Decimal.Parse(reader["TotalExpenditure"].ToString())
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

        public IActionResult OnPostDelete(int year)
        {
            DBClass.DeleteHistoricalSpendData(year);
            return RedirectToPage("/HistoricalSpending/ViewDataPage");
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
            DBClass.AddHistoricalSpendingData(NewHistoricalSpendingData);
            loadData();
            ModelState.Clear();
            NewHistoricalSpendingData = new Expenditure();
            return Page();
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }

}


