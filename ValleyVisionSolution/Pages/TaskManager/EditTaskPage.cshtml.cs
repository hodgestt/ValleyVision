using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftAntimalwareEngine;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
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
        public int? ViewedTask { get; set; }  
        public List<int> ViewedTaskUser { get; set; }
        [Required(ErrorMessage = "The description is required")]
        [BindProperty]
        
        public string TempTaskDescription { get; set; } 

        [BindProperty]
        public Task EditedTask { get; set; }
        [BindProperty]
        public List<int> EditedTaskUser { get; set; }
        

        public EditTaskPageModel() 
        {
            AllTasks = new List<Task>();
            InitUsers = new List<User>();
            ViewedTaskUser = new List<int>();
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
            SqlDataReader reader2 = DBClass.InitiativeUserReader(initID);
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

        public IActionResult OnGet(int taskID)
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                HttpContext.Session.SetInt32("TaskID", taskID);
                ViewedTask = HttpContext.Session.GetInt32("TaskID");
                ViewedTaskUser = DBClass.ViewedTaskUserReader(HttpContext.Session.GetInt32("TaskID"));
                loadData();
                foreach (var task in AllTasks)
                {
                    if (task.TaskID == taskID)
                    {
                        TempTaskDescription = task.TaskDescription;
                    }
                }
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public IActionResult OnPostDeleteTask()
        {
            ViewedTask = HttpContext.Session.GetInt32("TaskID");
            DBClass.DeleteTask(ViewedTask);
            return RedirectToPage("/TaskManager/TaskManagerPage");
        }

        public IActionResult OnPostUpdateTask()
        {
            if (!ModelState.IsValid)
            {
                // Model state is not valid, return the page with validation errors
                ViewedTask = HttpContext.Session.GetInt32("TaskID");
                ViewedTaskUser = DBClass.ViewedTaskUserReader(HttpContext.Session.GetInt32("TaskID"));
                loadData();
                foreach (var task in AllTasks)
                {
                    if (task.TaskID == HttpContext.Session.GetInt32("TaskID"))
                    {
                        TempTaskDescription = task.TaskDescription;
                    }
                }
                return Page();
            }

            // Model state is valid, continue with processing
            EditedTask.TaskDescription = TempTaskDescription;
            DBClass.EditTask(EditedTask, EditedTaskUser);
            return RedirectToPage("/TaskManager/TaskManagerPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

    }
}
