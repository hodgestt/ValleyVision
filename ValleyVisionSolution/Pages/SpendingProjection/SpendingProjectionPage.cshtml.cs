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
using ValleyVisionSolution.Services;
using DocumentFormat.OpenXml.Bibliography;

namespace ValleyVisionSolution.Pages.SpendingProjection
{
    public class SpendingProjectionPageModel : PageModel
    {
        public List<Expenditure> HistoricalExpenditures { get; set; } = new List<Expenditure>();
        public Expenditure LatestHistoricalExpenditure { get; set; } = new Expenditure();
        public List<Expenditure> ProjectedExpenditures { get; set; } = new List<Expenditure>();
        public List<ExpenditureProjection> ProjectedExpenditures2 { get; set; } = new List<ExpenditureProjection>();
        List<decimal> HistoricalInflationRates { get; set; } = new List<decimal>();
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
        public decimal ParameterAnomaly { get; set; }

        private readonly IBlobService _blobService;
        public SpendingProjectionPageModel(IBlobService blobService)
        {
            _blobService = blobService;
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
                
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

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
            
            DBClass.ValleyVisionConnection.Close();


            SqlDataReader reader3 = DBClass.InflationRatesReader();
            while (reader3.Read()) 
            { 
                HistoricalInflationRates.Add(Decimal.Parse(reader3["InflationRate"].ToString()));
            }
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

        public IActionResult OnPostProjectExpenditures2()
        {
            loadData();
            ProjectedExpenditures2.Add(new ExpenditureProjection
            { 
                Year = LatestHistoricalExpenditure.Year,
                InflationRate = LatestHistoricalExpenditure.InflationRate,
                PublicSafetyUCL = LatestHistoricalExpenditure.PublicSafety,
                PublicSafety = LatestHistoricalExpenditure.PublicSafety,
                PublicSafetyLCL = LatestHistoricalExpenditure.PublicSafety,
                SchoolUCL = LatestHistoricalExpenditure.School,
                School = LatestHistoricalExpenditure.School,
                SchoolLCL = LatestHistoricalExpenditure.School,
                AnomalyUCL = LatestHistoricalExpenditure.Anomaly,
                Anomaly = LatestHistoricalExpenditure.Anomaly,
                AnomalyLCL = LatestHistoricalExpenditure.Anomaly,
                OtherUCL = LatestHistoricalExpenditure.Other,
                Other = LatestHistoricalExpenditure.Other,
                OtherLCL = LatestHistoricalExpenditure.Other,
                TotalExpenditureUCL = LatestHistoricalExpenditure.TotalExpenditure,
                TotalExpenditure = LatestHistoricalExpenditure.TotalExpenditure,
                TotalExpenditureLCL = LatestHistoricalExpenditure.TotalExpenditure
            });
                        

            int nextYear = LatestHistoricalExpenditure.Year + 1;
            
            int elements = HistoricalInflationRates.Count;
            Random rnd = new Random();
            int randomIndex;
            int n = 24;

            List<decimal> InflationSimulations = new List<decimal>();
            List<decimal> InflationAverages = new List<decimal>();
            

            decimal PublicSafety = 0;
            List<decimal> PublicSafetySimulations = new List<decimal>();
            List<decimal> PublicSafetyUCLs = new List<decimal>();
            List<decimal> PublicSafetyAverages = new List<decimal>();
            List<decimal> PublicSafetyLCLs = new List<decimal>();

            decimal School = 0;
            List<decimal> SchoolSimulations = new List<decimal>();
            List<decimal> SchoolUCLs = new List<decimal>();
            List<decimal> SchoolAverages = new List<decimal>();
            List<decimal> SchoolLCLs = new List<decimal>();

            decimal Anomaly = 0;
            List<decimal> AnomalySimulations = new List<decimal>();
            List<decimal> AnomalyUCLs = new List<decimal>();
            List<decimal> AnomalyAverages = new List<decimal>();
            List<decimal> AnomalyLCLs = new List<decimal>();

            decimal Other = 0;
            List<decimal> OtherSimulations = new List<decimal>();
            List<decimal> OtherUCLs = new List<decimal>();
            List<decimal> OtherAverages = new List<decimal>();
            List<decimal> OtherLCLs = new List<decimal>();

            List<decimal> TotalExpenditureSimulations = new List<decimal>();
            List<decimal> TotalExpenditureUCLs = new List<decimal>();
            List<decimal> TotalExpenditureAverages = new List<decimal>();
            List<decimal> TotalExpenditureLCLs = new List<decimal>();



            for (int i = 0; i < NumProjectionYears; i++)
            {
                if (i == 0)
                {
                 
                    for (int j = 0; j < 1000; j++)
                    {
                        randomIndex = rnd.Next(0, elements - 1);
                        InflationSimulations.Add(HistoricalInflationRates[randomIndex]);

                        
                        PublicSafety = LatestHistoricalExpenditure.PublicSafety * (1 + HistoricalInflationRates[randomIndex]);
                        PublicSafetySimulations.Add(PublicSafety);

                        
                        School = LatestHistoricalExpenditure.School * (1 + HistoricalInflationRates[randomIndex]);
                        SchoolSimulations.Add(School);

                        
                        Anomaly = ParameterAnomaly;
                        AnomalySimulations.Add(Anomaly);

                        
                        Other = LatestHistoricalExpenditure.Other * (1 + HistoricalInflationRates[randomIndex]);
                        OtherSimulations.Add(Other);

                        TotalExpenditureSimulations.Add(PublicSafety + School + Anomaly + Other);
                    }

                    InflationAverages.Add(InflationSimulations.Average());

                    PublicSafetyUCLs.Add(PublicSafetySimulations.OrderByDescending(num => num).Skip(n).First());
                    PublicSafetyAverages.Add(PublicSafetySimulations.Average());
                    PublicSafetyLCLs.Add(PublicSafetySimulations.OrderBy(num => num).Skip(n).First());

                    SchoolUCLs.Add(SchoolSimulations.OrderByDescending(num => num).Skip(n).First());
                    SchoolAverages.Add(SchoolSimulations.Average());
                    SchoolLCLs.Add(SchoolSimulations.OrderBy(num => num).Skip(n).First());

                    AnomalyUCLs.Add(AnomalySimulations.OrderByDescending(num => num).Skip(n).First());
                    AnomalyAverages.Add(AnomalySimulations.Average());
                    AnomalyLCLs.Add(AnomalySimulations.OrderBy(num => num).Skip(n).First());

                    OtherUCLs.Add(OtherSimulations.OrderByDescending(num => num).Skip(n).First());
                    OtherAverages.Add(OtherSimulations.Average());
                    OtherLCLs.Add(OtherSimulations.OrderBy(num => num).Skip(n).First());

                    TotalExpenditureUCLs.Add(TotalExpenditureSimulations.OrderByDescending(num => num).Skip(n).First());
                    TotalExpenditureAverages.Add(TotalExpenditureSimulations.Average());
                    TotalExpenditureLCLs.Add(TotalExpenditureSimulations.OrderBy(num => num).Skip(n).First());

                    ProjectedExpenditures2.Add(new ExpenditureProjection
                    {
                        Year = nextYear,
                        InflationRate = InflationAverages[i],
                        PublicSafetyUCL = PublicSafetyUCLs[i],
                        PublicSafety = PublicSafetyAverages[i],
                        PublicSafetyLCL = PublicSafetyLCLs[i],
                        SchoolUCL = SchoolUCLs[i],
                        School = SchoolAverages[i],
                        SchoolLCL = SchoolLCLs[i],
                        AnomalyUCL = AnomalyUCLs[i],
                        Anomaly = AnomalyAverages[i],
                        AnomalyLCL = AnomalyLCLs[i],
                        OtherUCL = OtherUCLs[i],
                        Other = OtherAverages[i],
                        OtherLCL = OtherLCLs[i],
                        TotalExpenditureUCL = TotalExpenditureUCLs[i],
                        TotalExpenditure = TotalExpenditureAverages[i],
                        TotalExpenditureLCL = TotalExpenditureLCLs[i]
                    });

                    nextYear++;


                }
                else 
                {
                   
                    for (int j = 0; j < 1000; j++)
                    {
                        randomIndex = rnd.Next(0, elements - 1);
                        InflationSimulations[j] = HistoricalInflationRates[randomIndex];

                        PublicSafetySimulations[j] = PublicSafetySimulations[j] * (1 + HistoricalInflationRates[randomIndex]);
                        
                        SchoolSimulations[j] = SchoolSimulations[j] * (1 + HistoricalInflationRates[randomIndex]);
                        
                        AnomalySimulations[j] = AnomalySimulations[j] * (1 + HistoricalInflationRates[randomIndex]);

                        OtherSimulations[j] = OtherSimulations[j] * (1 + HistoricalInflationRates[randomIndex]);

                        TotalExpenditureSimulations[j] = (PublicSafetySimulations[j] + SchoolSimulations[j] + AnomalySimulations[j] + OtherSimulations[j]);
                    }

                    InflationAverages.Add(InflationSimulations.Average());

                    PublicSafetyUCLs.Add(PublicSafetySimulations.OrderByDescending(num => num).Skip(n).First());
                    PublicSafetyAverages.Add(PublicSafetySimulations.Average());
                    PublicSafetyLCLs.Add(PublicSafetySimulations.OrderBy(num => num).Skip(n).First());

                    SchoolUCLs.Add(SchoolSimulations.OrderByDescending(num => num).Skip(n).First());
                    SchoolAverages.Add(SchoolSimulations.Average());
                    SchoolLCLs.Add(SchoolSimulations.OrderBy(num => num).Skip(n).First());

                    AnomalyUCLs.Add(AnomalySimulations.OrderByDescending(num => num).Skip(n).First());
                    AnomalyAverages.Add(AnomalySimulations.Average());
                    AnomalyLCLs.Add(AnomalySimulations.OrderBy(num => num).Skip(n).First());

                    OtherUCLs.Add(OtherSimulations.OrderByDescending(num => num).Skip(n).First());
                    OtherAverages.Add(OtherSimulations.Average());
                    OtherLCLs.Add(OtherSimulations.OrderBy(num => num).Skip(n).First());

                    TotalExpenditureUCLs.Add(TotalExpenditureSimulations.OrderByDescending(num => num).Skip(n).First());
                    TotalExpenditureAverages.Add(TotalExpenditureSimulations.Average());
                    TotalExpenditureLCLs.Add(TotalExpenditureSimulations.OrderBy(num => num).Skip(n).First());

                    ProjectedExpenditures2.Add(new ExpenditureProjection
                    {
                        Year = nextYear,
                        InflationRate = InflationAverages[i],
                        PublicSafetyUCL = PublicSafetyUCLs[i],
                        PublicSafety = PublicSafetyAverages[i],
                        PublicSafetyLCL = PublicSafetyLCLs[i],
                        SchoolUCL = SchoolUCLs[i],
                        School = SchoolAverages[i],
                        SchoolLCL = SchoolLCLs[i],
                        AnomalyUCL = AnomalyUCLs[i],
                        Anomaly = AnomalyAverages[i],
                        AnomalyLCL = AnomalyLCLs[i],
                        OtherUCL = OtherUCLs[i],
                        Other = OtherAverages[i],
                        OtherLCL = OtherLCLs[i],
                        TotalExpenditureUCL = TotalExpenditureUCLs[i],
                        TotalExpenditure = TotalExpenditureAverages[i],
                        TotalExpenditureLCL = TotalExpenditureLCLs[i]
                    });

                    nextYear++;

                }
                
            }
            HttpContext.Session.SetString("ProjectedExpenditures2", JsonSerializer.Serialize(ProjectedExpenditures2));
            return Page();
        }


        public string GetChartDataAsJson()
        {
            return JsonSerializer.Serialize(ProjectedExpenditures2);
        }

        public async Task<IActionResult> OnPostDownloadExcel()
        {
            if (HttpContext.Session.GetString("ProjectedExpenditures2") != null)
            {
                ProjectedExpenditures2 = JsonSerializer.Deserialize<List<ExpenditureProjection>>(HttpContext.Session.GetString("ProjectedExpenditures2"));

                Stream templateStream = await _blobService.DownloadFileBlobAsync("SpendingProjectionTemplate.xlsx");
                if (templateStream == null)
                {
                    TempData["Error"] = "The template could not be found in Blob Storage.";
                    return Page();
                }

                using (var stream = new MemoryStream())
                {
                    await templateStream.CopyToAsync(stream);
                    stream.Position = 0; // Reset the stream position to the beginning

                    using (var workbook = new XLWorkbook(stream)) // Load the workbook from the stream
                    {
                        IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Use the appropriate worksheet

                        int currentRow = 2; // Start populating data from this row onwards

                        foreach (var expenditure in ProjectedExpenditures2)
                        {
                            worksheet.Cell(currentRow, 1).Value = expenditure.Year;
                            worksheet.Cell(currentRow, 2).Value = expenditure.InflationRate;
                            worksheet.Cell(currentRow, 3).Value = expenditure.PublicSafetyUCL;
                            worksheet.Cell(currentRow, 4).Value = expenditure.PublicSafety;
                            worksheet.Cell(currentRow, 5).Value = expenditure.PublicSafetyLCL;
                            worksheet.Cell(currentRow, 6).Value = expenditure.SchoolLCL;
                            worksheet.Cell(currentRow, 7).Value = expenditure.School;
                            worksheet.Cell(currentRow, 8).Value = expenditure.SchoolUCL;
                            worksheet.Cell(currentRow, 9).Value = expenditure.AnomalyUCL;
                            worksheet.Cell(currentRow, 10).Value = expenditure.Anomaly;
                            worksheet.Cell(currentRow, 11).Value = expenditure.AnomalyUCL;
                            worksheet.Cell(currentRow, 12).Value = expenditure.OtherUCL;
                            worksheet.Cell(currentRow, 13).Value = expenditure.Other;
                            worksheet.Cell(currentRow, 14).Value = expenditure.OtherUCL;
                            worksheet.Cell(currentRow, 15).Value = expenditure.TotalExpenditureLCL;
                            worksheet.Cell(currentRow, 16).Value = expenditure.TotalExpenditure;
                            worksheet.Cell(currentRow, 17).Value = expenditure.TotalExpenditureUCL;
                            currentRow++;
                        }

                        worksheet.Columns().AdjustToContents();

                        using (var outputStream = new MemoryStream())
                        {
                            workbook.SaveAs(outputStream);
                            outputStream.Position = 0; // Reset for reading
                            return File(outputStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SpendingProjections.xlsx");
                        }
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please run the projection before downloading the report.";
                return Page();
            }
        }

        //public IActionResult OnPostDownloadExcel()
        //{

        //    if (HttpContext.Session.GetString("ProjectedExpenditures") != null)
        //    {
        //        ProjectedExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("ProjectedExpenditures"));
        //    }
        //    else
        //    {
        //        // If not in session, load from DB (or handle as necessary)
        //        loadData();
        //    }

        //    string templatePath = "Pages/SpendingProjection/SpendingProjectionTemplate.xlsx";

        //    using (var workbook = new XLWorkbook(templatePath))
        //    {
        //        IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the first worksheet
        //        var currentRow = 2;

        //        // Data Rows
        //        foreach (var expenditure in ProjectedExpenditures)
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

        //        // Prepare the memory stream to download
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            var content = stream.ToArray();
        //            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SpendingProjections.xlsx");
        //        }
        //    }
        //}

        //public IActionResult OnPostSaveExcel()
        //{
        //    // Check if ProjectedExpenditures are stored in session and deserialize them
        //    if (HttpContext.Session.GetString("ProjectedExpenditures") != null)
        //    {
        //        ProjectedExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("ProjectedExpenditures"));
        //    }
        //    else
        //    {
        //        // If not in session, load from DB (or handle as necessary)
        //        loadData();
        //    }


        //    // Path to your Excel template for Projected Revenues
        //    string templatePath = "Pages/SpendingProjection/SpendingProjectionTemplate.xlsx";

        //    // Open the template
        //    using (var workbook = new XLWorkbook(templatePath))
        //    {
        //        IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the first worksheet
        //        var currentRow = 2;

        //        // Data Rows
        //        foreach (var expenditure in ProjectedExpenditures)
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

        //        // Define a path for the server-side file
        //        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        //        if (!Directory.Exists(directoryPath))
        //        {
        //            Directory.CreateDirectory(directoryPath);
        //        }
        //        var uniqueFileName = $"SpendingProjections_{DateTime.Now:MMdd_HHmmss}.xlsx";
        //        var filePath = Path.Combine(directoryPath, uniqueFileName);

        //        // Save the workbook to the specified path
        //        workbook.SaveAs(filePath);

        //        // Optional: Update your database with the file's details
        //        int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
        //        var fileMeta = new FileMeta
        //        {
        //            FileName_ = uniqueFileName,
        //            FilePath = filePath,
        //            FileType = ".xlsx",
        //            UploadedDateTime = DateTime.Now,
        //            userID = HttpContext.Session.GetInt32("UserID")
        //        };

        //        DBClass.UploadFile(initID, fileMeta);

        //        // Notify the user
        //        TempData["Message"] = $"{uniqueFileName} was Succesfully Saved to Budget Process Resources";
        //        return Page();
        //    }
        //}
        public async Task<IActionResult> OnPostSaveExcel()
        {
            if (HttpContext.Session.GetString("ProjectedExpenditures") != null)
            {
                ProjectedExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("ProjectedExpenditures"));
            }
            else
            {
                loadData();
            }

            var uniqueFileName = $"SpendingProjections_{DateTime.Now:MMdd_HHmmss}.xlsx";

            // Attempt to download the Excel template as a stream from Azure Blob Storage
            Stream templateStream = await _blobService.DownloadFileBlobAsync("SpendingProjectionTemplate.xlsx");
            if (templateStream == null)
            {
                TempData["Error"] = "The template could not be found in Blob Storage.";
                return Page();
            }

            using (var seekableStream = new MemoryStream())
            {
                await templateStream.CopyToAsync(seekableStream);
                seekableStream.Position = 0; // Ensure the stream is at the beginning before reading it

                using (var workbook = new XLWorkbook(seekableStream)) // Load the workbook from the stream
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Use the appropriate worksheet

                    int currentRow = 2; // Adjust based on your template's starting row for data

                    // Populate the worksheet with data from ProjectedRevenues
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

                    worksheet.Columns().AdjustToContents();

                    using (var outputStream = new MemoryStream())
                    {
                        workbook.SaveAs(outputStream);
                        outputStream.Position = 0; // Ready the stream for uploading

                        // Upload the filled workbook to Azure Blob Storage
                        await _blobService.UploadFileBlobAsync(uniqueFileName, outputStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    }
                }
            }
            // Update your database with the file's details
            int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
            var fileMeta = new FileMeta
            {
                FileName_ = uniqueFileName,
                FilePath = uniqueFileName, // Use a path or identifier suitable for Azure Blob Storage access
                FileType = ".xlsx",
                UploadedDateTime = DateTime.Now,
                userID = HttpContext.Session.GetInt32("UserID")
            };

            DBClass.UploadFile(initID, fileMeta);

            TempData["Message"] = $"{uniqueFileName} was Successfully Saved to Budget Process Resources";
            return Page();
        }

    public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
