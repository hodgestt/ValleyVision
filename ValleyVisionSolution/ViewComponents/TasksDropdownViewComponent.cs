using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using Task = ValleyVisionSolution.Pages.DataClasses.Task;

namespace ValleyVisionSolution.ViewComponents
{
    public class TasksDropdownViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int initID)
        {
            var tasks = FetchTasks();
            int counters = GetNotificationCount();
           

            return View(tasks);
        }

        private List<Task> FetchTasks()
        {
            List<Task> tasks = new List<Task>();
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (!userId.HasValue)
            {
                return tasks; // Return an empty list if not logged in
            }

            SqlDataReader reader = DBClass.GetTasksByUserId((int)userId);
            while (reader.Read())
            {
                tasks.Add(new Task
                {
                    TaskID = reader.GetInt32(reader.GetOrdinal("taskID")),
                    TaskName = reader.GetString(reader.GetOrdinal("taskName")),
                    TaskDueDateTime = reader.GetDateTime(reader.GetOrdinal("taskDueDateTime")),
                    TaskStatus = reader.GetString(reader.GetOrdinal("taskStatus")),
                    InitID = reader.GetInt32(reader.GetOrdinal("initID"))
                });
            }
            reader.Close(); // Don't forget to close the reader
            DBClass.ValleyVisionConnection.Close(); // Close connection if it's not managed elsewhere
            
            return tasks;
        }

        private int GetNotificationCount()
        {
            var task = new List<Task>();
            int? count = 0;
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (!userId.HasValue)
                return 0;// Return an empty string if not logged in

            SqlDataReader reader = DBClass.GetTasksByUserId((int)userId);
            while (reader.Read())
            {
                task.Add(new Task()
                {
                    TaskID = reader.GetInt32(reader.GetOrdinal("taskID")),
                    TaskName = reader.GetString(reader.GetOrdinal("taskName")),
                    TaskDueDateTime = reader.GetDateTime(reader.GetOrdinal("taskDueDateTime")),
                    TaskStatus = reader.GetString(reader.GetOrdinal("taskStatus")),
                    InitID = reader.GetInt32(reader.GetOrdinal("initID"))
                }
                );
            }
            foreach (var tasks in task)
            {
                count++;
            }
            HttpContext.Session.SetInt32("NotifCount", (int)count);
            reader.Close(); // Don't forget to close the reader
            DBClass.ValleyVisionConnection.Close(); // Close connection if it's not managed elsewhere

            if (count > 0)
            {
                HttpContext.Session.SetInt32("NotifCount", (int)count);
                return (int)count;
            }
            else
            { 
                return 0; 
            }
        }
    }
    
}
