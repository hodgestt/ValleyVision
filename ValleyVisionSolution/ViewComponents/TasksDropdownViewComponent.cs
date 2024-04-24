using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using Task = ValleyVisionSolution.Pages.DataClasses.Task;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ValleyVisionSolution.ViewComponents
{
    public class TasksDropdownViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int initID)
        {
            var task = new List<Task>();
            var inits = new List<Initiative>();
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue)
                return Content("No more notifications"); // Return an empty string if not logged in

            SqlDataReader reader = DBClass.GetTasksByUserId((int)userId);
            while (reader.Read())
            {
                task.Add( new Task()
                {
                    TaskID = reader.GetInt32(reader.GetOrdinal("taskID")),
                    TaskName = reader.GetString(reader.GetOrdinal("taskName")),
                    TaskDueDateTime = reader.GetDateTime(reader.GetOrdinal("taskDueDateTime")),
                    InitID = reader.GetInt32(reader.GetOrdinal("initID"))
                }
                );

            }
            reader.Close(); // Don't forget to close the reader
            DBClass.ValleyVisionConnection.Close(); // Close connection if it's not managed elsewhere


            return View(task);
        }

    }

}
