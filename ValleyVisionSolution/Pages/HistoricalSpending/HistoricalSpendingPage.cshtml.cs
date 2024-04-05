using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.Text.Json;
using ClosedXML.Excel;


namespace ValleyVisionSolution.Pages.HistoricalSpending
{
    public class HistoricalSpendingPageModel : PageModel
    {
        public List<Expenditure> HistoricalExpenditures { get; set; }

        public HistoricalSpendingPageModel() 
        {
            HistoricalExpenditures = new List<Expenditure>();
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
                    Anomaly = Decimal.Parse(reader["Anomaly"].ToString()),
                    Other = Decimal.Parse(reader["Other"].ToString()),
                    TotalExpenditure = Decimal.Parse(reader["TotalExpenditure"].ToString())
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
            var jsonData = JsonSerializer.Serialize(HistoricalExpenditures);
            HttpContext.Session.SetString("HistoricalExpenditures", jsonData);
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


        public IActionResult OnPostDownloadExcel()
        {
            // Check if HistoricalExpenditures are stored in session and deserialize them
            if (HttpContext.Session.GetString("HistoricalExpenditures") != null)
            {
                HistoricalExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("HistoricalExpenditures"));
            }
            else
            {
                // If not in session, load from DB (or handle as necessary)
                loadData();
            }

            // Path to your Excel template
            string templatePath = "Pages/HistoricalSpending/HistoricalSpendingTemplate.xlsx";

            // Open the template
            using (var workbook = new XLWorkbook(templatePath))
            {
                IXLWorksheet worksheet = workbook.Worksheets.FirstOrDefault(); // Assuming you want to use the first worksheet

                // Assuming your template's data range starts at row 2
                int currentRow = 6;
                foreach (var expenditure in HistoricalExpenditures)
                {
                    worksheet.Cell(currentRow, 15).Value = expenditure.Year;
                    worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
                    worksheet.Cell(currentRow, 17).Value = expenditure.InterestRate;
                    worksheet.Cell(currentRow, 18).Value = expenditure.PublicSafety;
                    worksheet.Cell(currentRow, 19).Value = expenditure.School;
                    worksheet.Cell(currentRow, 20).Value = expenditure.Anomaly;
                    worksheet.Cell(currentRow, 21).Value = expenditure.Other;
                    worksheet.Cell(currentRow, 22).Value = expenditure.TotalExpenditure;
                    currentRow++;
                }

                // Adjust column widths to content
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HistoricalSpendingReport.xlsx");
                }
            }
        }


        //public IActionResult OnPostDownloadExcel()
        //{
        //    // Check if HistoricalExpenditures are stored in session and deserialize them
        //    if (HttpContext.Session.GetString("HistoricalExpenditures") != null)
        //    {
        //        HistoricalExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("HistoricalExpenditures"));
        //    }
        //    else
        //    {
        //        // If not in session, load from DB (or handle as necessary)
        //        loadData();
        //    }

        //    using (var workbook = new XLWorkbook())
        //    {
        //        IXLWorksheet worksheet = workbook.Worksheets.Add("Historical Spending");

        //        // Define header
        //        worksheet.Cell(1, 1).Value = "Year";
        //        worksheet.Cell(1, 2).Value = "Inflation Rate";
        //        worksheet.Cell(1, 3).Value = "Interest Rate";
        //        worksheet.Cell(1, 4).Value = "Public Safety";
        //        worksheet.Cell(1, 5).Value = "School";
        //        worksheet.Cell(1, 6).Value = "Anomaly";
        //        worksheet.Cell(1, 7).Value = "Other";
        //        worksheet.Cell(1, 8).Value = "Total Expenditure";

        //        int currentRow = 2;
        //        foreach (var expenditure in HistoricalExpenditures)
        //        {
        //            worksheet.Cell(currentRow, 1).Value = expenditure.Year;
        //            worksheet.Cell(currentRow, 2).Value = expenditure.InflationRate;
        //            worksheet.Cell(currentRow, 3).Value = expenditure.InterestRate;
        //            worksheet.Cell(currentRow, 4).Value = expenditure.PublicSafety;
        //            worksheet.Cell(currentRow, 5).Value = expenditure.School;
        //            worksheet.Cell(currentRow, 6).Value = expenditure.Anomaly;
        //            worksheet.Cell(currentRow, 7).Value = expenditure.Other;
        //            worksheet.Cell(currentRow, 8).Value = expenditure.TotalExpenditure;
        //            currentRow++;
        //        }

        //        // Adjust column widths to content
        //        worksheet.Columns().AdjustToContents();

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            var content = stream.ToArray();

        //            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HistoricalSpendingReport.xlsx");
        //        }
        //    }
        //}



        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
