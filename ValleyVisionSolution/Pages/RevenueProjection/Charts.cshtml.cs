using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.Text.Json;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class ChartModel : PageModel
    {
        [BindProperty]
        public List<Revenue> DataList { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                LoadChartDataFromDatabase();
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private void LoadChartDataFromDatabase()
        {
            DataList = DBClass.GetChartDataFromDatabase();
        }

        public string GetChartDataAsJson()
        {
         return JsonSerializer.Serialize(DataList);
        }


    public string GetChartDataAsArray()
        {
            var dataArray = new List<string>();
            foreach (var item in DataList)
            {
                dataArray.Add($"[{item.RealEstateTax}, {item.PersonalPropertyTax}, {item.FeesLicensesTax}, {item.StateFunding}, {item.TotalRevenue}]");
            }
            return $"[{string.Join(",", dataArray)}]";
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
