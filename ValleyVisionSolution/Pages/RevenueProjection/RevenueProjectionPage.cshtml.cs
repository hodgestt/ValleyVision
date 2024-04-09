using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using ClosedXML.Excel;
using ValleyVisionSolution.Services;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class RevenueProjectionPageModel : PageModel
    {
        public Revenue LatestRevenue = new Revenue();
        public Revenue[] ProjectedRevenues { get; set; }
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

            // Generate a random number between 0.975 and 1.025
            Random random = new Random();
            double randomNum = 0.975 + (random.NextDouble() * 0.05);
            decimal randomNumber = (decimal)randomNum;

            //list of the projected revenues
            ProjectedRevenues = new Revenue[NumProjectionYears + 1];

            ProjectedRevenues[0] = new Revenue
            {
                RealEstateTax = LatestRevenue.RealEstateTax,
                PersonalPropertyTax = LatestRevenue.PersonalPropertyTax,
                FeesLicensesTax = LatestRevenue.FeesLicensesTax,
                StateFunding = LatestRevenue.StateFunding,
                TotalRevenue = LatestRevenue.TotalRevenue,
                Year = LatestRevenue.Year
            };

            // Calculate projections for subsequent years
            for (int i = 1; i <= NumProjectionYears; i++)
            {
                ProjectedRevenues[i] = new Revenue
                {
                    RealEstateTax = Math.Round(ProjectedRevenues[i - 1].RealEstateTax * (1 + (RealEstateTaxGrowth / 100)) * randomNumber, 2),
                    PersonalPropertyTax = Math.Round(ProjectedRevenues[i - 1].PersonalPropertyTax * (1 + (PersonalPropertyTaxGrowth / 100)) * randomNumber, 2),
                    FeesLicensesTax = Math.Round(ProjectedRevenues[i - 1].FeesLicensesTax * (1 + (FeesLicensesTaxGrowth / 100)) * randomNumber, 2),
                    StateFunding = Math.Round(StateFundingAmount, 2),
                    Year = ProjectedRevenues[i - 1].Year + 1
                };

                ProjectedRevenues[i].TotalRevenue = Math.Round(
                    ProjectedRevenues[i].RealEstateTax +
                    ProjectedRevenues[i].PersonalPropertyTax +
                    ProjectedRevenues[i].FeesLicensesTax +
                    ProjectedRevenues[i].StateFunding, 2);
            }
            HttpContext.Session.SetString("ProjectedRevenues", JsonSerializer.Serialize(ProjectedRevenues));
            DefaultLoad = false;

            return Page();
        }
        public string GetChartDataAsJson()
        {
            return JsonSerializer.Serialize(ProjectedRevenues);
        }

        public IActionResult OnPostDownloadExcel()
        {
            if (HttpContext.Session.GetString("ProjectedRevenues") != null)
            {
                ProjectedRevenues = JsonSerializer.Deserialize<Revenue[]>(HttpContext.Session.GetString("ProjectedRevenues"));

                // Specify the path to your projected revenues Excel template
                string templatePath = "Pages/RevenueProjection/RevenueProjectionsTemplate.xlsx";

                using (var workbook = new XLWorkbook(templatePath)) // Open the template
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming you want to use the second worksheet
                    var currentRow = 5; // Starting row for the data

                    // Header rows might already be set in your template, so you might not need to set them again here

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

                    // Adjust column widths to content, if necessary
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "RevenueProjections.xlsx");
                    }
                }
            }
            else
            {
                //TempData["ErrorMessage"] = "Please run the projection before downloading the report.";
                return RedirectToPage("/RevenueProjection");
            }
        }

        //public IActionResult OnPostSaveExcel()
        //{
        //    // Check if ProjectedRevenues are stored in session and deserialize them
        //    if (HttpContext.Session.GetString("ProjectedRevenues") != null)
        //    {
        //        ProjectedRevenues = JsonSerializer.Deserialize<Revenue[]>(HttpContext.Session.GetString("ProjectedRevenues"));
        //    }
        //    else
        //    {
        //        loadData();

        //    }

        //    // Path to your Excel template for Projected Revenues
        //    string templatePath = "Pages/RevenueProjection/RevenueProjectionsTemplate.xlsx";

        //    // Open the template
        //    using (var workbook = new XLWorkbook(templatePath))
        //    {
        //        IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming the data should be placed in the second worksheet

        //        // Starting row for the data
        //        int currentRow = 5; // Adjust based on your template

        //        // Populate the worksheet with data from ProjectedRevenues
        //        foreach (var revenue in ProjectedRevenues)
        //        {
        //            currentRow++;
        //            worksheet.Cell(currentRow, 1).Value = revenue.Year;
        //            worksheet.Cell(currentRow, 2).Value = revenue.RealEstateTax;
        //            worksheet.Cell(currentRow, 3).Value = revenue.PersonalPropertyTax;
        //            worksheet.Cell(currentRow, 4).Value = revenue.FeesLicensesTax;
        //            worksheet.Cell(currentRow, 5).Value = revenue.StateFunding;
        //            worksheet.Cell(currentRow, 6).Value = revenue.TotalRevenue;
        //        }

        //        // Adjust column widths to content
        //        worksheet.Columns().AdjustToContents();

        //        // Define a path for the server-side file
        //        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        //        if (!Directory.Exists(directoryPath))
        //        {
        //            Directory.CreateDirectory(directoryPath);
        //        }
        //        var uniqueFileName = $"RevenueProjections_{DateTime.Now:MMdd_HHmmss}.xlsx";
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
            // Check if ProjectedRevenues are stored in session and deserialize them
            if (HttpContext.Session.GetString("ProjectedRevenues") != null)
            {
                ProjectedRevenues = JsonSerializer.Deserialize<Revenue[]>(HttpContext.Session.GetString("ProjectedRevenues"));
            }
            else
            {
                loadData();
            }

            // Generate a unique file name for the new Excel file
            var uniqueFileName = $"RevenueProjections_{DateTime.Now:MMdd_HHmmss}.xlsx";

            // Use a MemoryStream for the Excel package to work with
            using (var stream = new MemoryStream())
            {
                // Path to your Excel template for Projected Revenues
                //string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pages", "RevenueProjection", "RevenueProjectionsTemplate.xlsx");
                string templatePath = "Pages/RevenueProjection/RevenueProjectionsTemplate.xlsx";

                using (var workbook = new XLWorkbook(templatePath))
                {
                    IXLWorksheet worksheet = workbook.Worksheets.Worksheet(2); // Assuming the data should be placed in the second worksheet

                    // Starting row for the data
                    int currentRow = 5; // Adjust based on your template

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

                    // Adjust column widths to content
                    worksheet.Columns().AdjustToContents();

                    // Write the workbook to the MemoryStream
                    workbook.SaveAs(stream);
                }

                // Reset the position of the MemoryStream to the beginning
                stream.Position = 0;

                // Use the IBlobService to upload the MemoryStream to Azure Blob Storage
                await _blobService.UploadFileBlobAsync(uniqueFileName, stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                // Optional: Update your database with the file's details
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
            }

            // Notify the user
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
