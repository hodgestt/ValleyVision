using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.ProposedDevelopments
{
    public class EditDevelopmentsModel : PageModel
    {
        public List<DevelopmentArea> AllDevs { get; set; }
        public List<User> InitUsers { get; set; }
        public int? ViewedDev { get; set; }
        public List<int> ViewedDevUsers { get; set; }
        [BindProperty]
        public string TempDevDescription { get; set; }

        [BindProperty]
        public DevelopmentArea EditedDev { get; set; }
        [BindProperty]
        public List<int> EditedDevUsers { get; set; }

        public EditDevelopmentsModel()
        {
            AllDevs = new List<DevelopmentArea>();
            InitUsers = new List<User>();
            ViewedDevUsers = new List<int>();
        }

        public void loadData()
        {
            int? devID = HttpContext.Session.GetInt32("devID");

            //Populate AllTasks list
            SqlDataReader reader = DBClass.AllDevsReader(devID);
            while (reader.Read())
            {
                AllDevs.Add(new DevelopmentArea
                {
                    devID = Int32.Parse(reader["devID"].ToString()),
                    devName = reader["devName"].ToString(),
                    devDescription = reader["devDescription"].ToString(),
                    devImpactLevel = reader["devImpactLevel"].ToString(),
                    uploadedDateTime = Convert.ToDateTime(reader["uploadedDateTime"]),
                    userID = Int32.Parse(reader["userID"].ToString())
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
        }

        public void OnGet(int devID)
        {
            HttpContext.Session.SetInt32("devID", devID);
            ViewedDev = HttpContext.Session.GetInt32("devID");
            ViewedDevUsers = DBClass.ViewedDevUsersReader(HttpContext.Session.GetInt32("devID"));
            loadData();
            foreach (var dev in AllDevs)
            {
                if (dev.devID == devID)
                {
                    TempDevDescription = dev.devDescription;
                }
            }
        }

        public IActionResult OnPostUpdateDev()
        {
            if (!ModelState.IsValid)
            {
                // Model state is not valid, return the page with validation errors
                ViewedDev = HttpContext.Session.GetInt32("devID");
                ViewedDevUsers = DBClass.ViewedDevUsersReader(HttpContext.Session.GetInt32("devID"));
                loadData();
                foreach (var dev in AllDevs)
                {
                    if (dev.devID == HttpContext.Session.GetInt32("devID"))
                    {
                        TempDevDescription = dev.devDescription;
                    }
                }
                return Page();
            }

            // Model state is valid, continue with processing
            EditedDev.devDescription = TempDevDescription;
            DBClass.EditDev(EditedDev, EditedDevUsers);
            return RedirectToPage("/ProposedDevelopments/DevelopmentPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
