using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;


namespace ValleyVisionSolution.Pages.ManageProfiles
{
    public class EditProfilesPageModel : PageModel
    {

        [BindProperty]
        public FullProfile EditedProfile { get; set; }
        public int? ViewedProfile { get; set; }
        public List<FullProfile> AllProfiles { get; set; } 

        public EditProfilesPageModel()
        {
            AllProfiles = new List<FullProfile>();
        }

        public void OnGet()
        {
        }
    }
}
