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

    public TaskManagerPageModel()
    {
        AllTasks = new List<Task>();
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
    }
    public void OnGet()
    {
        loadData();
    }
}
