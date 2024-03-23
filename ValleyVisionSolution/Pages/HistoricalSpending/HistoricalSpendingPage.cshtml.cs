using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ValleyVisionSolution.Pages.HistoricalSpending
{
    public class HistoricalSpendingPageModel : PageModel
    {
        public void OnGet()
        {
        }



        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
