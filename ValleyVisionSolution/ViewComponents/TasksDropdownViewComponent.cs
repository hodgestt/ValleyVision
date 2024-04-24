using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ValleyVisionSolution.Pages.DB;
using Microsoft.AspNetCore.Http;

namespace ValleyVisionSolution.ViewComponents
{
    public class TasksDropdownViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue)
                return Content("No more notifications"); // Return an empty string if not logged in

            var tasks = DBClass.GetTasksByUserId((int)userId);
            return View(tasks);
        }
        
    }

}
