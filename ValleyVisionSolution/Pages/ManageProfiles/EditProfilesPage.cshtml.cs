using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;

namespace ValleyVisionSolution.Pages.ManageProfiles
{
    public class EditProfilesPageModel : PageModel
    {

        [BindProperty]
        public User UserToUpdate { get; set; }

       

        public EditProfilesPageModel()
        {
            UserToUpdate = new User();
        }

        public void OnGet()
        {
        }
    }
}
