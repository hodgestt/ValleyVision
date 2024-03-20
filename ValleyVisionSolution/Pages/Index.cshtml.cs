using ValleyVisionSolution.Pages.DataClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages
{
    public class IndexModel : PageModel
    {

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                HttpContext.Session.SetInt32("UserID", -1);
            }   
        }
    }
}
