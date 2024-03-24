using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using Task = ValleyVisionSolution.Pages.DataClasses.Task;

namespace ValleyVisionSolution.Pages.TaskManager
{
    public class EditTaskPageModel : PageModel
    {
        public List<Task> AllTasks { get; set; }
        public List<User> InitUsers { get; set; }
        public int ViewedTask { get; set; }  
        public List<int> ViewedTaskUsers { get; set; }

        [BindProperty]
        public Task EditedTask { get; set; }
        [BindProperty]
        public List<int> EditedTaskUsers { get; set; }

        public EditTaskPageModel() 
        {
            AllTasks = new List<Task>();
            InitUsers = new List<User>();
            ViewedTaskUsers = new List<int>();
        }

        public void loadData()
        {
            int? initID = HttpContext.Session.GetInt32("InitID");

            //Populate AllTasks list
            SqlDataReader reader = DBClass.AllTasksReader(initID);
            while (reader.Read())
            {
                AllTasks.Add(new Task
                {
                    TaskID = Int32.Parse(reader["TaskID"].ToString()),
                    TaskName = reader["TaskName"].ToString(),
                    TaskStatus = reader["TaskStatus"].ToString(),
                    TaskDescription = reader["TaskDescription"].ToString(),
                    TaskDueDateTime = Convert.ToDateTime(reader["TaskDueDateTime"]),
                    InitID = Int32.Parse(reader["InitID"].ToString())
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

            //Populate InitUsers list
            SqlDataReader reader2 = DBClass.InitiativeUsersReader(initID);
            while (reader2.Read())
            {
                InitUsers.Add(new User
                {
                    UserID = Int32.Parse(reader2["UserID"].ToString()),
                    FirstName = reader2["FirstName"].ToString(),
                    LastName = reader2["LastName"].ToString()
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
        }

        public void OnGet(int taskID)
        {
            ViewedTask = taskID;
            ViewedTaskUsers = TempDBClass.ViewedTaskUsersReader(taskID);
            loadData();
        }
    }
}
