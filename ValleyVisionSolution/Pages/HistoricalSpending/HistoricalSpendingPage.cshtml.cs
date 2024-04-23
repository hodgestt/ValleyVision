using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.Text.Json;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ValleyVisionSolution.Services;


namespace ValleyVisionSolution.Pages.HistoricalSpending
{
    public class HistoricalSpendingPageModel : PageModel
    {
        public List<Expenditure> HistoricalExpenditures = new List<Expenditure>();
        private readonly IBlobService _blobService;
        public HistoricalSpendingPageModel(IBlobService blobService) 
        {
            _blobService = blobService;
        }

        public void loadData()
        {
            
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
        public async Task<IActionResult> OnPostSaveExcel()
        {
            if (HttpContext.Session.GetString("HistoricalExpenditures") != null)
            {
                HistoricalExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("HistoricalExpenditures"));
            }
            else
            {
                loadData();
            }

            var uniqueFileName = $"HistoricalSpendingReport_{DateTime.Now:MMdd_HHmmss}.xlsx";

            // Attempt to download the Excel template as a stream from Azure Blob Storage
            Stream nonSeekableStream = await _blobService.DownloadFileBlobAsync("HistoricalSpendingTemplate.xlsx");
            if (nonSeekableStream == null)
            {
                TempData["Error"] = "The template could not be found in Blob Storage.";
                return Page();
            }

            using (var seekableStream = new MemoryStream())
            {
                await nonSeekableStream.CopyToAsync(seekableStream);
                seekableStream.Position = 0; // Reset the position to the beginning for ClosedXML to read from start

                using (var workbook = new XLWorkbook(seekableStream)) // Load the workbook from the now seekable stream
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2);

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

                    worksheet.Columns().AdjustToContents();

                    using (var outputStream = new MemoryStream())
                    {
                        workbook.SaveAs(outputStream);
                        outputStream.Position = 0; // Reset the position to the beginning for uploading

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
                FilePath = uniqueFileName, // Since this is in Blob Storage, adjust as needed
                FileType = ".xlsx",
                UploadedDateTime = DateTime.Now,
                userID = HttpContext.Session.GetInt32("UserID")
            };

            DBClass.UploadFile(initID, fileMeta);

            TempData["Message"] = $"{uniqueFileName} was Successfully Saved to Budget Process Resources";
            return Page();
        }


        public async Task<IActionResult> OnPostDownloadExcel()
        {
            if (HttpContext.Session.GetString("HistoricalExpenditures") != null)
            {
                HistoricalExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("HistoricalExpenditures"));
            }
            else
            {
                loadData();
            }

            Stream templateStream = await _blobService.DownloadFileBlobAsync("HistoricalSpendingTemplate.xlsx");
            if (templateStream == null)
            {
                TempData["Error"] = "The template could not be found in Blob Storage.";
                return Page();
            }

            using (var stream = new MemoryStream())
            {
                await templateStream.CopyToAsync(stream);
                stream.Position = 0; // Ensure the stream is at the beginning for ClosedXML to read from start

                using (var workbook = new XLWorkbook(stream)) // Load the workbook from the stream
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming the data should be placed in the second worksheet

                    int currentRow = 2; // Adjust based on your template's starting row for data
                    foreach (var expenditure in HistoricalExpenditures)
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
                        outputStream.Position = 0; // Reset stream position for reading
                        return File(outputStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HistoricalSpendingReport.xlsx");
                    }
                }
            }
        }


        


        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
