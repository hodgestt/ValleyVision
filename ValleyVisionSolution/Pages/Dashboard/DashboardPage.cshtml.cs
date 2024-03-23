using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.Dashboard
{
    public class DashboardPageModel : PageModel
    {
        public List<Tile> TilesList { get; set; }

        public DashboardPageModel() 
        {
            TilesList = new List<Tile>();
        }

        public void loadData(int initID) 
        {
            //populate TilesList with tiles that the specific initiative has activated
            SqlDataReader reader = DBClass.TilesReader((int)initID);
            while (reader.Read())
            {
                TilesList.Add(new Tile
                {
                    TileID = int.Parse(reader["TileID"].ToString()),
                    TileName = reader["TileName"].ToString(),
                    IconPath = reader["IconPath"].ToString(),
                    PageLink = reader["PageLink"].ToString()
                });
            }

            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

        }

        public IActionResult OnGet(int initID)
        {
            HttpContext.Session.SetInt32("InitID", initID);

            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                loadData(initID);
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }

            
        }


        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

    }
}
