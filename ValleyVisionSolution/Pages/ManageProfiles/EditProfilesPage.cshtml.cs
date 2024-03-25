using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;

namespace ValleyVisionSolution.Pages.ManageProfiles
{
    public class EditProfilesPageModel : PageModel
    {

        [BindProperty]
        public FullProfile EditedProfile { get; set; }

        public EditProfilesPageModel()
        {
            
        }

        public void OnGet()
        {
        }
    }
}
