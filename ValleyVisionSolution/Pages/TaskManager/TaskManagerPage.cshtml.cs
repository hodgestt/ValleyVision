using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using Task = ValleyVisionSolution.Pages.DataClasses.Task;

namespace ValleyVisionSolution.Pages.TaskManager;

public class TaskManagerPageModel : PageModel
{
    public List<Task> AllTasks { get; set; }
    public List<Task> MyTasks { get; set; }
    public List<User> InitUsers { get; set; }

    [BindProperty]
    public Task NewTask { get; set; }
    [BindProperty]
    public List<int> NewTaskUsers { get; set; }


    public TaskManagerPageModel()
    {
        AllTasks = new List<Task>();
        MyTasks = new List<Task>();
        InitUsers = new List<User>();
        
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

        //Populate MyTasks list
        SqlDataReader reader2 = DBClass.MyTasksReader(HttpContext.Session.GetInt32("UserID"), HttpContext.Session.GetInt32("InitID"));
        while (reader2.Read())
        {
            MyTasks.Add(new Task
            {
                TaskID = Int32.Parse(reader2["TaskID"].ToString()),
                TaskName = reader2["TaskName"].ToString(),
                TaskStatus = reader2["TaskStatus"].ToString(),
                TaskDescription = reader2["TaskDescription"].ToString(),
                TaskDueDateTime = Convert.ToDateTime(reader2["TaskDueDateTime"]),
                InitID = Int32.Parse(reader2["InitID"].ToString())
            });
        }
        // Close your connection in DBClass
        DBClass.ValleyVisionConnection.Close();

        //Populate InitUsers list
        SqlDataReader reader3 = DBClass.InitiativeUsersReader(initID);
        while (reader3.Read())
        {
            InitUsers.Add(new User
            {
                UserID = Int32.Parse(reader3["UserID"].ToString()),
                FirstName = reader3["FirstName"].ToString(),
                LastName = reader3["LastName"].ToString()
            });
        }
        // Close your connection in DBClass
        DBClass.ValleyVisionConnection.Close();
    }
    public void OnGet()
    {
        loadData();
    }

    public IActionResult OnPostAddTask()
    {
        if (!ModelState.IsValid)
        {
            // Model state is not valid, return the page with validation errors
            loadData();
            return Page();
        }

        // Model state is valid, continue with processing
        DBClass.AddTask(HttpContext.Session.GetInt32("InitID"), NewTask, NewTaskUsers);
        loadData();
        ModelState.Clear();
        NewTaskUsers = new List<int>();
        NewTask = new Task();
        return Page();
    }


    public IActionResult OnPostLogoutHandler()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }
}
