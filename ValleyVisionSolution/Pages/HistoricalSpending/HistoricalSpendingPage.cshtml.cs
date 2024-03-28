using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.Text.Json;

namespace ValleyVisionSolution.Pages.HistoricalSpending
{
    public class HistoricalSpendingPageModel : PageModel
    {
        public List<Expenditure> HistoricalExpenditures { get; set; }

        public HistoricalSpendingPageModel() 
        {

        }

        public void loadData()
        {
            //Populate current year revenue data
            SqlDataReader reader = DBClass.HistoricalExpendituresReader();
            while (reader.Read())
            {
                HistoricalExpenditures.Add(new Expenditure
                {
                    Year = int.Parse(reader["Year_"].ToString()),
                    InflationRate = Decimal.Parse(reader["InflationRate"].ToString()),
                    InterestRate = Decimal.Parse(reader["InterestRate"].ToString()),
                    PublicSafety = Decimal.Parse(reader["PublicSafety"].ToString()),
                    School = Decimal.Parse(reader["School"].ToString()),
                    Anomoly = Decimal.Parse(reader["Anomoly"].ToString()),
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

        public string GetChartDataAsJson()
        {
            return JsonSerializer.Serialize(HistoricalExpenditures);
        }


        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
