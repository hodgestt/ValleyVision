using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;

namespace ValleyVisionSolution.Pages.RevenueProjection
{
    public class ViewDataPageModel : PageModel
    {
        public List<Revenue> RevenueDataList { get; set; }
        public bool OpenAddProfileModal { get; set; }

        [BindProperty]
        public Revenue NewRevenueData { get; set; }


        public ViewDataPageModel()
        {
            RevenueDataList = new List<Revenue>();
            OpenAddProfileModal = false; //keeps the modal open on page reload if input validations were not successful
        }




        public void OnGet()
        {
        }
    }
}
