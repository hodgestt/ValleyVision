using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;

namespace ValleyVisionSolution.Pages.RevenueProjection
{

    public class ChartModel : PageModel
    {
        [BindProperty]
        public List<Revenue> DataList { get; set; }

        public void OnGet()
        {
            DataList = new List<Revenue>();

            string connectionString = "Server=Localhost;Database=Main;Trusted_Connection=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT year_, realEstateTax, personalPropertyTax, feesLicensesTax, stateFunding FROM DataFile_2";
                SqlCommand command = new SqlCommand(sqlQuery, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Revenue dataItem = new Revenue();
                    dataItem.Year = reader["year_"].ToString();
                    dataItem.RealEstateTax = Convert.ToInt32(reader["realEstateTax"]);
                    dataItem.PersonalPropertyTax = Convert.ToInt32(reader["personalPropertyTax"]);
                    dataItem.FeesLicensesTax = Convert.ToInt32(reader["feesLicensesTax"]);
                    dataItem.StateFunding = Convert.ToInt32(reader["stateFunding"]);
                    DataList.Add(dataItem);
                }
                connection.Close();
            }
        }
        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
