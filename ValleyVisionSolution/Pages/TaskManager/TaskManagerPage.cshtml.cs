using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Diagnostics.Tracing.Parsers;
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
    public bool OpenModal { get; set; }

    public int initID { get; set; }

    [BindProperty]
    public Task NewTask { get; set; }
    [BindProperty]
    public List<int> NewTaskUser { get; set; }

    public List<Initiative> Inits { get; set; }


    public TaskManagerPageModel()
    {
        AllTasks = new List<Task>();
        MyTasks = new List<Task>();
        InitUsers = new List<User>();
        OpenModal = false;
        Inits = new List<Initiative>();
    }

    public void loadData(int initID)
    {
        string initName = HttpContext.Session.GetString("InitName");
        initID = (int)HttpContext.Session.GetInt32("InitID");
        

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
        SqlDataReader reader3 = DBClass.InitiativeUserReader(initID);
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

    public void OnGet(int initID)
    {
        int? sessionInitID = HttpContext.Session.GetInt32("InitID");

        if (sessionInitID != null)
        {
            if (sessionInitID == initID)
            {
                if (HttpContext.Session.GetString("InitName") == null)
                {
                    LoadInitiativeNameData();
                }
                else
                {
                    LoadInitiativeData();
                }
            }
            else
            {
                if(initID == 0)
                {
                    initID = (int)sessionInitID;
                    GetInitName(initID);
                    loadData(initID);
                }
                else
                {
                    GetInitName(initID);
                    loadData(initID);
                }

                
            }
        }
        else
        {
            HttpContext.Session.SetInt32("InitID", initID);
            SqlDataReader reader4 = DBClass.EditedInitiativeReader(initID);
            while (reader4.Read())
            {
                Inits.Add(new Initiative
                {
                InitID = Int32.Parse(reader4["InitID"].ToString()),
                    InitName = reader4["InitName"].ToString()
                });

            }
            foreach (var initiative in Inits)
            {
                if (initiative.InitID == initID)
                {
                    HttpContext.Session.SetString("InitName", initiative.InitName);
                }
            }
            DBClass.ValleyVisionConnection.Close();
            loadData(initID);
        }

        
    }

    public IActionResult OnPostAddTask(int initID)
    {
        if (!ModelState.IsValid)
        {
                // Model state is not valid, return the page with validation errors
            loadData(initID);
            OpenModal = true;
            return Page();
        }

            // Model state is valid, continue with processing
        DBClass.AddTask(HttpContext.Session.GetInt32("InitID"), NewTask, NewTaskUsers);
        loadData(initID);
        ModelState.Clear();
        NewTaskUser = new List<int>();
        NewTask = new Task();
        return RedirectToPage("/TaskManager/TaskManagerPage");
        }


    public IActionResult OnPostLogoutHandler(int initID)
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }

    private void LoadInitiativeNameData()
    {
        int initID = (int)HttpContext.Session.GetInt32("InitID");
        using (SqlDataReader reader4 = DBClass.EditedInitiativeReader(initID))
        {
            // Assuming the reader returns only one row per initiative.
            if (reader4.Read())
            {
                HttpContext.Session.SetString("InitName", reader4["InitName"].ToString());
            }
        }

            // Ensure the database connection is closed after the operation.
        DBClass.ValleyVisionConnection.Close();
        loadData(initID);
    }
    private void LoadInitiativeData()
    {
        int initID = (int)HttpContext.Session.GetInt32("InitID");

            // Ensure the database connection is closed after the operation.
        DBClass.ValleyVisionConnection.Close();
        loadData(initID);
    }
    private void GetInitName(int initID)
    {
        HttpContext.Session.SetInt32("InitID", initID);
        initID = (int)HttpContext.Session.GetInt32("InitID");
        SqlDataReader reader4 = DBClass.EditedInitiativeReader(initID);
        while (reader4.Read())
        {
            Inits.Add(new Initiative
            {
                InitID = Int32.Parse(reader4["InitID"].ToString()),
                InitName = reader4["InitName"].ToString()
            });

        }
        foreach (var initiative in Inits)
        {
            if (initiative.InitID == initID)
            {
                HttpContext.Session.SetString("InitName", initiative.InitName);
            }
        }
        DBClass.ValleyVisionConnection.Close();

    }
}
