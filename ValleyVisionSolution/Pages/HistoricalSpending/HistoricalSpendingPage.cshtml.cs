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

                    int currentRow = 6; // Adjust based on your template's starting row for data
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
                        outputStream.Position = 0; // Reset stream position for reading
                        return File(outputStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HistoricalSpendingReport.xlsx");
                    }
                }
            }
        }


        //public async Task<IActionResult> OnPostDownloadExcel()
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

        //    // Download the Excel template as a stream from Azure Blob Storage
        //    Stream templateStream = await _blobService.DownloadFileBlobAsync("HistoricalSpendingTemplate.xlsx");
        //    if (templateStream == null)
        //    {
        //        TempData["Error"] = "The template could not be found in Blob Storage.";
        //        return Page();
        //    }

        //    // Now that you have the template, you can start working with it using ClosedXML
        //    using (var workbook = new XLWorkbook(templateStream))
        //    {
        //        IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming the data should be placed in the second worksheet

        //        // Populate the worksheet with data
        //        int currentRow = 6; // Adjust the starting row based on your template
        //        foreach (var expenditure in HistoricalExpenditures)
        //        {
        //            // Fill the cells with expenditure data
        //            worksheet.Cell(currentRow, 15).Value = expenditure.Year;
        //            worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
        //            // Continue for other cells...
        //            currentRow++;
        //        }

        //        // Adjust column widths to content
        //        worksheet.Columns().AdjustToContents();

        //        // Prepare the workbook to be returned
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0; // Reset the stream position to the beginning
        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HistoricalSpendingReport.xlsx");
        //        }
        //    }
        //}
        //public async Task<IActionResult> OnPostSaveExcel()
        //{
        //    if (HttpContext.Session.GetString("HistoricalExpenditures") != null)
        //    {
        //        HistoricalExpenditures = JsonSerializer.Deserialize<List<Expenditure>>(HttpContext.Session.GetString("HistoricalExpenditures"));
        //    }
        //    else
        //    {
        //        loadData();
        //    }

        //    var uniqueFileName = $"HistoricalSpendingReport_{DateTime.Now:MMdd_HHmmss}.xlsx";

        //    // Attempt to download the Excel template as a stream from Azure Blob Storage
        //    Stream nonSeekableStream = await _blobService.DownloadFileBlobAsync("HistoricalSpendingTemplate.xlsx");
        //    if (nonSeekableStream == null)
        //    {
        //        TempData["Error"] = "The template could not be found in Blob Storage.";
        //        return Page();
        //    }

        //    using (var seekableStream = new MemoryStream())
        //    {
        //        await nonSeekableStream.CopyToAsync(seekableStream);
        //        seekableStream.Position = 0; // Reset the position to the beginning for ClosedXML to read from start

        //        using (var workbook = new XLWorkbook(seekableStream)) // Load the workbook from the now seekable stream
        //        {
        //            IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2);

        //            int currentRow = 6;
        //            foreach (var expenditure in HistoricalExpenditures)
        //            {
        //                worksheet.Cell(currentRow, 15).Value = expenditure.Year;
        //                worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
        //                worksheet.Cell(currentRow, 17).Value = expenditure.InterestRate;
        //                worksheet.Cell(currentRow, 18).Value = expenditure.PublicSafety;
        //                worksheet.Cell(currentRow, 19).Value = expenditure.School;
        //                worksheet.Cell(currentRow, 20).Value = expenditure.Anomaly;
        //                worksheet.Cell(currentRow, 21).Value = expenditure.Other;
        //                worksheet.Cell(currentRow, 22).Value = expenditure.TotalExpenditure;
        //                currentRow++;
        //            }

        //            worksheet.Columns().AdjustToContents();

        //            using (var outputStream = new MemoryStream())
        //            {
        //                workbook.SaveAs(outputStream);
        //                outputStream.Position = 0; // Reset the position to the beginning for uploading

        //                // Upload the filled workbook to Azure Blob Storage
        //                await _blobService.UploadFileBlobAsync(uniqueFileName, outputStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        //            }
        //        }
        //    }

        //    // Update your database with the file's details
        //    int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
        //    var fileMeta = new FileMeta
        //    {
        //        FileName_ = uniqueFileName,
        //        FilePath = uniqueFileName, // Since this is in Blob Storage, adjust as needed
        //        FileType = ".xlsx",
        //        UploadedDateTime = DateTime.Now,
        //        userID = HttpContext.Session.GetInt32("UserID")
        //    };

        //    DBClass.UploadFile(initID, fileMeta);

        //    TempData["Message"] = $"{uniqueFileName} was Successfully Saved to Budget Process Resources";
        //    return Page();
        //}




        //public async Task<IActionResult> OnPostSaveExcel()
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

        //    // Generate a unique file name for the new Excel file
        //    var uniqueFileName = $"HistoricalSpendingReport_{DateTime.Now:MMdd_HHmmss}.xlsx";

        //    // Use a MemoryStream for the Excel package to work with
        //    using (var stream = new MemoryStream())
        //    {
        //        // Path to your Excel template
        //        string templatePath = "Pages/HistoricalSpending/HistoricalSpendingTemplate.xlsx";

        //        using (var workbook = new XLWorkbook(templatePath))
        //        {
        //            IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the second worksheet

        //            // Populate the worksheet with data
        //            int currentRow = 6;
        //            foreach (var expenditure in HistoricalExpenditures)
        //            {
        //                worksheet.Cell(currentRow, 15).Value = expenditure.Year;
        //                worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
        //                worksheet.Cell(currentRow, 17).Value = expenditure.InterestRate;
        //                worksheet.Cell(currentRow, 18).Value = expenditure.PublicSafety;
        //                worksheet.Cell(currentRow, 19).Value = expenditure.School;
        //                worksheet.Cell(currentRow, 20).Value = expenditure.Anomaly;
        //                worksheet.Cell(currentRow, 21).Value = expenditure.Other;
        //                worksheet.Cell(currentRow, 22).Value = expenditure.TotalExpenditure;
        //                currentRow++;
        //            }

        //            // Adjust column widths to content
        //            worksheet.Columns().AdjustToContents();

        //            // Write the workbook to the MemoryStream
        //            workbook.SaveAs(stream);
        //        }

        //        // Reset the position of the MemoryStream to the beginning
        //        stream.Position = 0;

        //        // Use the IBlobService to upload the MemoryStream to Azure Blob Storage
        //        await _blobService.UploadFileBlobAsync(uniqueFileName, stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        //        // You can update your database with the file's details
        //        int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
        //        var fileMeta = new FileMeta
        //        {
        //            FileName_ = uniqueFileName,
        //            FilePath = uniqueFileName, // Since this is in Blob Storage, adjust as needed
        //            FileType = ".xlsx",
        //            UploadedDateTime = DateTime.Now,
        //            userID = HttpContext.Session.GetInt32("UserID")
        //        };

        //        DBClass.UploadFile(initID, fileMeta);
        //    }

        //    // Notify the user (you might want to redirect to a confirmation page or show a message)
        //    TempData["Message"] = $"{uniqueFileName} was Successfully Saved to Budget Process Resources";
        //    return Page();
        //}

        //public async Task<IActionResult> OnPostSaveExcel()
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

        //    // Generate a unique file name for the new Excel file
        //    var uniqueFileName = $"HistoricalSpendingReport_{DateTime.Now:MMdd_HHmmss}.xlsx";

        //    // Use a MemoryStream for the Excel package to work with
        //    using (var stream = new MemoryStream())
        //    {
        //        // Path to your Excel template
        //        string templatePath = "Pages/HistoricalSpending/HistoricalSpendingTemplate.xlsx";

        //        using (var workbook = new XLWorkbook(templatePath))
        //        {
        //            IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the second worksheet

        //            // Populate the worksheet with data
        //            int currentRow = 6;
        //            foreach (var expenditure in HistoricalExpenditures)
        //            {
        //                worksheet.Cell(currentRow, 15).Value = expenditure.Year;
        //                worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
        //                worksheet.Cell(currentRow, 17).Value = expenditure.InterestRate;
        //                worksheet.Cell(currentRow, 18).Value = expenditure.PublicSafety;
        //                worksheet.Cell(currentRow, 19).Value = expenditure.School;
        //                worksheet.Cell(currentRow, 20).Value = expenditure.Anomaly;
        //                worksheet.Cell(currentRow, 21).Value = expenditure.Other;
        //                worksheet.Cell(currentRow, 22).Value = expenditure.TotalExpenditure;
        //                currentRow++;
        //            }

        //            // Adjust column widths to content
        //            worksheet.Columns().AdjustToContents();

        //            // Write the workbook to the MemoryStream
        //            workbook.SaveAs(stream);
        //        }

        //        // Reset the position of the MemoryStream to the beginning
        //        stream.Position = 0;

        //        // Use the IBlobService to upload the MemoryStream to Azure Blob Storage
        //        await _blobService.UploadFileBlobAsync(uniqueFileName, stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        //        // You can update your database with the file's details
        //        int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
        //        var fileMeta = new FileMeta
        //        {
        //            FileName_ = uniqueFileName,
        //            FilePath = uniqueFileName, // Since this is in Blob Storage, adjust as needed
        //            FileType = ".xlsx",
        //            UploadedDateTime = DateTime.Now,
        //            userID = HttpContext.Session.GetInt32("UserID")
        //        };

        //        DBClass.UploadFile(initID, fileMeta);
        //    }

        //    // Notify the user (you might want to redirect to a confirmation page or show a message)
        //    TempData["Message"] = $"{uniqueFileName} was Successfully Saved to Budget Process Resources";
        //    return Page();
        //}





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

        //    // Path to your Excel template
        //    string templatePath = "Pages/HistoricalSpending/HistoricalSpendingTemplate.xlsx";

        //    // Open the template
        //    using (var workbook = new XLWorkbook(templatePath))
        //    {
        //        IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the first worksheet

        //        // Assuming your template's data range starts at row 2
        //        int currentRow = 6;
        //        foreach (var expenditure in HistoricalExpenditures)
        //        {
        //            worksheet.Cell(currentRow, 15).Value = expenditure.Year;
        //            worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
        //            worksheet.Cell(currentRow, 17).Value = expenditure.InterestRate;
        //            worksheet.Cell(currentRow, 18).Value = expenditure.PublicSafety;
        //            worksheet.Cell(currentRow, 19).Value = expenditure.School;
        //            worksheet.Cell(currentRow, 20).Value = expenditure.Anomaly;
        //            worksheet.Cell(currentRow, 21).Value = expenditure.Other;
        //            worksheet.Cell(currentRow, 22).Value = expenditure.TotalExpenditure;
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
        //public async Task<IActionResult> OnPostSaveExcel()
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

        //    // Generate a unique file name for the new Excel file
        //    var uniqueFileName = $"HistoricalSpendingReport_{DateTime.Now:MMdd_HHmmss}.xlsx";

        //    // Use a MemoryStream for the Excel package to work with
        //    using (var stream = new MemoryStream())
        //    {
        //        // Path to your Excel template
        //        string templatePath = "Pages/HistoricalSpending/HistoricalSpendingTemplate.xlsx";

        //        using (var workbook = new XLWorkbook(templatePath))
        //        {
        //            IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the second worksheet

        //            // Populate the worksheet with data
        //            int currentRow = 6;
        //            foreach (var expenditure in HistoricalExpenditures)
        //            {
        //                worksheet.Cell(currentRow, 15).Value = expenditure.Year;
        //                worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
        //                worksheet.Cell(currentRow, 17).Value = expenditure.InterestRate;
        //                worksheet.Cell(currentRow, 18).Value = expenditure.PublicSafety;
        //                worksheet.Cell(currentRow, 19).Value = expenditure.School;
        //                worksheet.Cell(currentRow, 20).Value = expenditure.Anomaly;
        //                worksheet.Cell(currentRow, 21).Value = expenditure.Other;
        //                worksheet.Cell(currentRow, 22).Value = expenditure.TotalExpenditure;
        //                currentRow++;
        //            }

        //            // Adjust column widths to content
        //            worksheet.Columns().AdjustToContents();

        //            // Write the workbook to the MemoryStream
        //            workbook.SaveAs(stream);
        //        }

        //        // Reset the position of the MemoryStream to the beginning
        //        stream.Position = 0;

        //        // Use the IBlobService to upload the MemoryStream to Azure Blob Storage
        //        await _blobService.UploadFileBlobAsync(uniqueFileName, stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        //        // You can update your database with the file's details
        //        int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
        //        var fileMeta = new FileMeta
        //        {
        //            FileName_ = uniqueFileName,
        //            FilePath = uniqueFileName, // Since this is in Blob Storage, adjust as needed
        //            FileType = ".xlsx",
        //            UploadedDateTime = DateTime.Now,
        //            userID = HttpContext.Session.GetInt32("UserID")
        //        };

        //        DBClass.UploadFile(initID, fileMeta);
        //    }

        //    // Notify the user (you might want to redirect to a confirmation page or show a message)
        //    TempData["Message"] = $"{uniqueFileName} was Successfully Saved to Budget Process Resources";
        //    return Page();
        //}

        //public async Task<IActionResult> OnPostDownloadExcel()
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

        //    // Download the Excel template as a stream from Azure Blob Storage
        //    Stream templateStream = await _blobService.DownloadFileBlobAsync("HistoricalSpendingTemplate.xlsx");
        //    if (templateStream == null)
        //    {
        //        TempData["Error"] = "The template could not be found in Blob Storage.";
        //        return Page();
        //    }

        //    // Now that you have the template, you can start working with it using ClosedXML
        //    using (var workbook = new XLWorkbook(templateStream))
        //    {
        //        IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming the data should be placed in the second worksheet

        //        // Populate the worksheet with data
        //        int currentRow = 6; // Adjust the starting row based on your template
        //        foreach (var expenditure in HistoricalExpenditures)
        //        {
        //            // Fill the cells with expenditure data
        //            worksheet.Cell(currentRow, 15).Value = expenditure.Year;
        //            worksheet.Cell(currentRow, 16).Value = expenditure.InflationRate;
        //            // Continue for other cells...
        //            currentRow++;
        //        }

        //        // Adjust column widths to content
        //        worksheet.Columns().AdjustToContents();

        //        // Prepare the workbook to be returned
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0; // Reset the stream position to the beginning
        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HistoricalSpendingReport.xlsx");
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
