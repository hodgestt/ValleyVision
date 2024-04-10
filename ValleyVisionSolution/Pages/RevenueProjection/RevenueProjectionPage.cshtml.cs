using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using ClosedXML.Excel;
using ValleyVisionSolution.Services;
using System.Collections.Generic;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class RevenueProjectionPageModel : PageModel
    {
        public Revenue LatestRevenue = new Revenue();
        public List<Revenue> ProjectedRevenues { get; set; } = new List<Revenue>();
        public bool DefaultLoad = true;



        [BindProperty]
        public decimal RealEstateTaxGrowth { get; set; }
        [BindProperty]
        public decimal PersonalPropertyTaxGrowth { get; set; }
        [BindProperty]
        public decimal FeesLicensesTaxGrowth { get; set; }
        [BindProperty]
        public decimal StateFundingAmount { get; set; }
        [BindProperty]
        public int NumProjectionYears { get; set; }

        private readonly IBlobService _blobService;

        public RevenueProjectionPageModel(IBlobService blobService)
        {

            _blobService = blobService;

        }

        public void loadData()
        {
            //Populate current year revenue data
            SqlDataReader reader = DBClass.LatestRevenueYearReader();
            if (reader.Read())
            {
                LatestRevenue = new Revenue
                {
                    Year = int.Parse(reader["Year_"].ToString()),
                    RealEstateTax = Decimal.Parse(reader["RealEstateTax"].ToString()),
                    PersonalPropertyTax = Decimal.Parse(reader["PersonalPropertyTax"].ToString()),
                    FeesLicensesTax = Decimal.Parse(reader["FeesLicensesTax"].ToString()),
                    StateFunding = Decimal.Parse(reader["OtherRevenue"].ToString()),
                    TotalRevenue = Decimal.Parse(reader["TotalRevenue"].ToString())
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

        public IActionResult OnPostRunProjection()
        {
            loadData();

            //// Generate a random number between 0.975 and 1.025
            //Random random = new Random();
            //double randomNum = 0.975 + (random.NextDouble() * 0.05);
            //decimal randomNumber = (decimal)randomNum;

            

            ProjectedRevenues.Add(new Revenue
            {
                RealEstateTax = LatestRevenue.RealEstateTax,
                PersonalPropertyTax = LatestRevenue.PersonalPropertyTax,
                FeesLicensesTax = LatestRevenue.FeesLicensesTax,
                StateFunding = LatestRevenue.StateFunding,
                TotalRevenue = LatestRevenue.TotalRevenue,
                Year = LatestRevenue.Year
            });

            // Calculate projections for subsequent years
            for (int i = 1; i <= NumProjectionYears; i++)
            {
                ProjectedRevenues.Add(new Revenue
                {
                    RealEstateTax = Math.Round(ProjectedRevenues[i - 1].RealEstateTax * (1 + (RealEstateTaxGrowth / 100)), 2),
                    PersonalPropertyTax = Math.Round(ProjectedRevenues[i - 1].PersonalPropertyTax * (1 + (PersonalPropertyTaxGrowth / 100)), 2),
                    FeesLicensesTax = Math.Round(ProjectedRevenues[i - 1].FeesLicensesTax * (1 + (FeesLicensesTaxGrowth / 100)), 2),
                    StateFunding = Math.Round(StateFundingAmount, 2),
                    Year = ProjectedRevenues[i - 1].Year + 1,
                    TotalRevenue = Math.Round((ProjectedRevenues[i - 1].RealEstateTax * (1 + (RealEstateTaxGrowth / 100))) +
                        (ProjectedRevenues[i - 1].PersonalPropertyTax * (1 + (PersonalPropertyTaxGrowth / 100))) +
                        (ProjectedRevenues[i - 1].FeesLicensesTax * (1 + (FeesLicensesTaxGrowth / 100))) +
                        (StateFundingAmount), 2)

                });

               
            }
            HttpContext.Session.SetString("ProjectedRevenues", JsonSerializer.Serialize(ProjectedRevenues));
            DefaultLoad = false;

            return Page();
        }
        public string GetChartDataAsJson()
        {
            return JsonSerializer.Serialize(ProjectedRevenues);
        }
        

        public async Task<IActionResult> OnPostDownloadExcel()
        {
            if (HttpContext.Session.GetString("ProjectedRevenues") != null)
            {
                ProjectedRevenues = JsonSerializer.Deserialize<List<Revenue>>(HttpContext.Session.GetString("ProjectedRevenues"));

                Stream templateStream = await _blobService.DownloadFileBlobAsync("RevenueProjectionsTemplate.xlsx");
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

                        int currentRow = 5; // Start populating data from this row onwards

                        foreach (var revenue in ProjectedRevenues)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = revenue.Year;
                            worksheet.Cell(currentRow, 2).Value = revenue.RealEstateTax;
                            worksheet.Cell(currentRow, 3).Value = revenue.PersonalPropertyTax;
                            worksheet.Cell(currentRow, 4).Value = revenue.FeesLicensesTax;
                            worksheet.Cell(currentRow, 5).Value = revenue.StateFunding;
                            worksheet.Cell(currentRow, 6).Value = revenue.TotalRevenue;
                            
                        }

                        worksheet.Columns().AdjustToContents();

                        using (var outputStream = new MemoryStream())
                        {
                            workbook.SaveAs(outputStream);
                            outputStream.Position = 0; // Reset for reading
                            return File(outputStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RevenueProjections.xlsx");
                        }
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please run the projection before downloading the report.";
                return RedirectToPage("/RevenueProjection");
            }
        }




        public async Task<IActionResult> OnPostSaveExcel()
        {
            if (HttpContext.Session.GetString("ProjectedRevenues") != null)
            {
                ProjectedRevenues = JsonSerializer.Deserialize<List<Revenue>>(HttpContext.Session.GetString("ProjectedRevenues"));
            }
            else
            {
                loadData();
            }

            var uniqueFileName = $"RevenueProjections_{DateTime.Now:MMdd_HHmmss}.xlsx";

            // Attempt to download the Excel template as a stream from Azure Blob Storage
            Stream templateStream = await _blobService.DownloadFileBlobAsync("RevenueProjectionsTemplate.xlsx");
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

                    int currentRow = 5; // Adjust based on your template's starting row for data

                    // Populate the worksheet with data from ProjectedRevenues
                    foreach (var revenue in ProjectedRevenues)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = revenue.Year;
                        worksheet.Cell(currentRow, 2).Value = revenue.RealEstateTax;
                        worksheet.Cell(currentRow, 3).Value = revenue.PersonalPropertyTax;
                        worksheet.Cell(currentRow, 4).Value = revenue.FeesLicensesTax;
                        worksheet.Cell(currentRow, 5).Value = revenue.StateFunding;
                        worksheet.Cell(currentRow, 6).Value = revenue.TotalRevenue;
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


    //    public async Task<IActionResult> OnPostSaveExcel()
    //    {
    //        // Check if ProjectedRevenues are stored in session and deserialize them
    //        if (HttpContext.Session.GetString("ProjectedRevenues") != null)
    //        {
    //            ProjectedRevenues = JsonSerializer.Deserialize<Revenue[]>(HttpContext.Session.GetString("ProjectedRevenues"));
    //        }
    //        else
    //        {
    //            loadData();
    //        }

    //        // Generate a unique file name for the new Excel file
    //        var uniqueFileName = $"RevenueProjections_{DateTime.Now:MMdd_HHmmss}.xlsx";

    //        // Use a MemoryStream for the Excel package to work with
    //        using (var stream = new MemoryStream())
    //        {
    //            // Path to your Excel template for Projected Revenues
    //            //string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pages", "RevenueProjection", "RevenueProjectionsTemplate.xlsx");
    //            string templatePath = "Pages/RevenueProjection/RevenueProjectionsTemplate.xlsx";

    //            using (var workbook = new XLWorkbook(templatePath))
    //            {
    //                IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming the data should be placed in the second worksheet

    //                // Starting row for the data
    //                int currentRow = 5; // Adjust based on your template

    //                // Populate the worksheet with data from ProjectedRevenues
    //                foreach (var revenue in ProjectedRevenues)
    //                {
    //                    currentRow++;
    //                    worksheet.Cell(currentRow, 1).Value = revenue.Year;
    //                    worksheet.Cell(currentRow, 2).Value = revenue.RealEstateTax;
    //                    worksheet.Cell(currentRow, 3).Value = revenue.PersonalPropertyTax;
    //                    worksheet.Cell(currentRow, 4).Value = revenue.FeesLicensesTax;
    //                    worksheet.Cell(currentRow, 5).Value = revenue.StateFunding;
    //                    worksheet.Cell(currentRow, 6).Value = revenue.TotalRevenue;

    //                }

    //                // Adjust column widths to content
    //                worksheet.Columns().AdjustToContents();

    //                // Write the workbook to the MemoryStream
    //                workbook.SaveAs(stream);
    //            }

    //            // Reset the position of the MemoryStream to the beginning
    //            stream.Position = 0;

    //            // Use the IBlobService to upload the MemoryStream to Azure Blob Storage
    //            await _blobService.UploadFileBlobAsync(uniqueFileName, stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    //            // Optional: Update your database with the file's details
    //            int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
    //            var fileMeta = new FileMeta
    //            {
    //                FileName_ = uniqueFileName,
    //                FilePath = uniqueFileName, // Since this is in Blob Storage, adjust as needed
    //                FileType = ".xlsx",
    //                UploadedDateTime = DateTime.Now,
    //                userID = HttpContext.Session.GetInt32("UserID")
    //            };

    //            DBClass.UploadFile(initID, fileMeta);
    //        }

    //        // Notify the user
    //        TempData["Message"] = $"{uniqueFileName} was Successfully Saved to Budget Process Resources";
    //        return Page();
    //    }


    //    public IActionResult OnPostLogoutHandler()
    //    {
    //        HttpContext.Session.Clear();
    //        return RedirectToPage("/Index");
    //    }
    //}
}
