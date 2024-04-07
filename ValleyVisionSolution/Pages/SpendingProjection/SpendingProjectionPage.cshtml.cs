using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using ClosedXML.Excel;

namespace ValleyVisionSolution.Pages.SpendingProjection
{
    public class SpendingProjectionPageModel : PageModel
    {
        public List<Expenditure> HistoricalExpenditures { get; set; }
        public Expenditure LatestHistoricalExpenditure { get; set; }
        public List<Expenditure> ProjectedExpenditures { get; set; }
        public decimal LastTotal = 0;
        public decimal ChangeInTotal = 0;
        public decimal SumChangeInTotal = 0;
        public decimal Counter = -1;
        public decimal AverageExpenditureChange = 0;

        [BindProperty]
        public int NumProjectionYears { get; set; }
        [BindProperty]
        public decimal ParameterInflationRate { get; set; }
        [BindProperty]
        public decimal ParameterInterestRate { get; set; }
        [BindProperty]
        public decimal ParameterAnomaly { get; set; }

        public SpendingProjectionPageModel() 
        { 
            HistoricalExpenditures = new List<Expenditure>();
            LatestHistoricalExpenditure = new Expenditure();
            ProjectedExpenditures = new List<Expenditure> ();
        }    

        public void loadData()
        {

            decimal InflationAdjustment = 0;
            decimal InterestAdjustment = 0;
            //Populate current year revenue data
            SqlDataReader reader = DBClass.HistoricalExpendituresReader();
            while (reader.Read())
            {
                
                HistoricalExpenditures.Add(new Expenditure
                {
                    Year = int.Parse(reader["Year_"].ToString()),
                    InflationRate = Decimal.Parse(reader["InflationRate"].ToString()) + InflationAdjustment,
                    InterestRate = Decimal.Parse(reader["InterestRate"].ToString()) + InterestAdjustment,
                    PublicSafety = Decimal.Parse(reader["PublicSafety"].ToString()),
                    School = Decimal.Parse(reader["School"].ToString()),
                    Anomaly = Decimal.Parse(reader["Anomaly"].ToString()),
                    Other = Decimal.Parse(reader["Other"].ToString()),
                    TotalExpenditure = Decimal.Parse(reader["TotalExpenditure"].ToString())
                });
                //InflationAdjustment = Decimal.Parse(reader["InflationRate"].ToString()) + InflationAdjustment;
                //InterestAdjustment = Decimal.Parse(reader["InterestRate"].ToString()) + InterestAdjustment;
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

            LastTotal = HistoricalExpenditures[0].TotalExpenditure;

            foreach (var e in HistoricalExpenditures)
            {
                ChangeInTotal = e.TotalExpenditure - LastTotal;
                SumChangeInTotal += ChangeInTotal;
                Counter++;
                LastTotal = e.TotalExpenditure;
            }

            AverageExpenditureChange = SumChangeInTotal / Counter;

            //Populate current year revenue data
            SqlDataReader reader2 = DBClass.LatestHistoricalExpenditureReader();
            while (reader2.Read())
            {
                LatestHistoricalExpenditure = new Expenditure
                {
                    Year = int.Parse(reader2["Year_"].ToString()),
                    InflationRate = Decimal.Parse(reader2["InflationRate"].ToString()),
                    InterestRate = Decimal.Parse(reader2["InterestRate"].ToString()),
                    PublicSafety = Decimal.Parse(reader2["PublicSafety"].ToString()),
                    School = Decimal.Parse(reader2["School"].ToString()),
                    Anomaly = Decimal.Parse(reader2["Anomaly"].ToString()),
                    Other = Decimal.Parse(reader2["Other"].ToString()),
                    TotalExpenditure = Decimal.Parse(reader2["TotalExpenditure"].ToString())
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

        public IActionResult OnPostProjectExpenditures()
        {
            loadData();
            ProjectedExpenditures.Add(LatestHistoricalExpenditure);
            ParameterInflationRate = ParameterInflationRate / 100;
            decimal Adjustment = 0;
            ParameterInterestRate = ParameterInterestRate / 100;

            int nextYear = LatestHistoricalExpenditure.Year + 1;

            for (int i = 1; i <= NumProjectionYears; i++)
            {
                // Extracting data from the list into arrays
                double[] inflationRate = HistoricalExpenditures.Select(e => (double)e.InflationRate).ToArray();
                double[] interestRate = HistoricalExpenditures.Select(e => (double)e.InterestRate).ToArray();
                double[] publicSafety = HistoricalExpenditures.Select(e => (double)e.PublicSafety).ToArray();
                double[] school = HistoricalExpenditures.Select(e => (double)e.School).ToArray();
                double[] other = HistoricalExpenditures.Select(e => (double)e.Other).ToArray();


                // Create the design matrix with an intercept column
                var designMatrixPublicSafety = Matrix<double>.Build.DenseOfColumnArrays(
                    new double[publicSafety.Length].Populate(1), // Intercept column of 1's
                    inflationRate,
                    interestRate
                );

                var designMatrixSchool = Matrix<double>.Build.DenseOfColumnArrays(
                    new double[school.Length].Populate(1), // Intercept column of 1's
                    inflationRate,
                    interestRate
                );

                var designMatrixOther = Matrix<double>.Build.DenseOfColumnArrays(
                    new double[other.Length].Populate(1), // Intercept column of 1's
                    inflationRate,
                    interestRate
                );

                // Create the vector for the dependent variable
                var publicSafetyVector = Vector<double>.Build.Dense(publicSafety);
                var schoolVector = Vector<double>.Build.Dense(school);
                var otherVector = Vector<double>.Build.Dense(other);


                // Perform the multiple regression
                var publicSafetycoefficients = MultipleRegression.NormalEquations(designMatrixPublicSafety, publicSafetyVector);
                var schoolcoefficients = MultipleRegression.NormalEquations(designMatrixSchool, schoolVector);
                var othercoefficients = MultipleRegression.NormalEquations(designMatrixOther, otherVector);



                ProjectedExpenditures.Add(new Expenditure
                {
                    Year = nextYear,
                    InflationRate = ParameterInflationRate,
                    InterestRate = ParameterInterestRate,
                    PublicSafety = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate)),
                    School = (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate)),
                    Anomaly = ParameterAnomaly,
                    Other = (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate)),
                    TotalExpenditure = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate))
                                       + (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate))
                                       + ParameterAnomaly
                                       + (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate))
                                       + Adjustment
                });

                HistoricalExpenditures.Add(new Expenditure
                {
                    Year = nextYear,
                    InflationRate = ParameterInflationRate,
                    InterestRate = ParameterInterestRate,
                    PublicSafety = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate)),
                    School = (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate)),
                    Anomaly = ParameterAnomaly,
                    Other = (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate)),
                    TotalExpenditure = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate))
                                       + (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate))
                                       + ParameterAnomaly
                                       + (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate))
                                       + Adjustment
                });

                nextYear++;
                Adjustment = AverageExpenditureChange * i * (1+ParameterInflationRate);
                
            }







            HttpContext.Session.SetString("ProjectedExpenditures", JsonSerializer.Serialize(ProjectedExpenditures));
            return Page();
        }

        public string GetChartDataAsJson()
        {
            return JsonSerializer.Serialize(ProjectedExpenditures);
        }

        public IActionResult OnPostDownloadExcel()
        {

            if (HttpContext.Session.GetString("ProjectedExpenditures") != null)
            {
                ProjectedExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("ProjectedExpenditures"));
            }
            else
            {
                // If not in session, load from DB (or handle as necessary)
                loadData();
            }

            string templatePath = "Pages/SpendingProjection/SpendingProjectionTemplate.xlsx";

            using (var workbook = new XLWorkbook(templatePath))
            {
                IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the first worksheet
                var currentRow = 2;

                // Data Rows
                foreach (var expenditure in ProjectedExpenditures)
                {
                    worksheet.Cell(currentRow, 1).Value = expenditure.Year;
                    worksheet.Cell(currentRow, 2).Value = expenditure.InflationRate;
                    worksheet.Cell(currentRow, 3).Value = expenditure.InterestRate;
                    worksheet.Cell(currentRow, 4).Value = expenditure.PublicSafety;
                    worksheet.Cell(currentRow, 5).Value = expenditure.School;
                    worksheet.Cell(currentRow, 6).Value = expenditure.Anomaly;
                    worksheet.Cell(currentRow, 7).Value = expenditure.Other;
                    worksheet.Cell(currentRow, 8).Value = expenditure.TotalExpenditure;
                    currentRow++;
                }

                // Adjust column widths to content
                worksheet.Columns().AdjustToContents();

                // Prepare the memory stream to download
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SpendingProjections.xlsx");
                }
            }
        }

        public IActionResult OnPostSaveExcel()
        {
            // Check if ProjectedExpenditures are stored in session and deserialize them
            if (HttpContext.Session.GetString("ProjectedExpenditures") != null)
            {
                ProjectedExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("ProjectedExpenditures"));
            }
            else
            {
                // If not in session, load from DB (or handle as necessary)
                loadData();
            }


            // Path to your Excel template for Projected Revenues
            string templatePath = "Pages/SpendingProjection/SpendingProjectionTemplate.xlsx";

            // Open the template
            using (var workbook = new XLWorkbook(templatePath))
            {
                IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the first worksheet
                var currentRow = 2;

                // Data Rows
                foreach (var expenditure in ProjectedExpenditures)
                {
                    worksheet.Cell(currentRow, 1).Value = expenditure.Year;
                    worksheet.Cell(currentRow, 2).Value = expenditure.InflationRate;
                    worksheet.Cell(currentRow, 3).Value = expenditure.InterestRate;
                    worksheet.Cell(currentRow, 4).Value = expenditure.PublicSafety;
                    worksheet.Cell(currentRow, 5).Value = expenditure.School;
                    worksheet.Cell(currentRow, 6).Value = expenditure.Anomaly;
                    worksheet.Cell(currentRow, 7).Value = expenditure.Other;
                    worksheet.Cell(currentRow, 8).Value = expenditure.TotalExpenditure;
                    currentRow++;
                }

                // Adjust column widths to content
                worksheet.Columns().AdjustToContents();

                // Define a path for the server-side file
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var uniqueFileName = $"SpendingProjections_{DateTime.Now:MMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(directoryPath, uniqueFileName);

                // Save the workbook to the specified path
                workbook.SaveAs(filePath);

                // Optional: Update your database with the file's details
                int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
                var fileMeta = new FileMeta
                {
                    FileName_ = uniqueFileName,
                    FilePath = filePath,
                    FileType = ".xlsx",
                    UploadedDateTime = DateTime.Now,
                    userID = HttpContext.Session.GetInt32("UserID")
                };

                DBClass.UploadFile(initID, fileMeta);

                // Notify the user
                TempData["Message"] = $"{uniqueFileName} was Succesfully Saved to Budget Process Resources";
                return Page();
            }
        }


        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
