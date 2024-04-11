using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.Initiatives
{
    public class EditInitiativesPageModel : PageModel
    {
        [BindProperty]
        public Initiative EditedInitiative { get; set; }

        public List<User> InitUsers { get; set; }
        public List<int> SelectedInitUsers { get; set; }
        [BindProperty]
        public List<int> EditedInitUsers { get; set; }

        public List<Tile> Tiles { get; set; }
        public List<int> SelectedTiles { get; set; }
        [BindProperty]
        public List<int> EditedTiles { get; set; }



        public EditInitiativesPageModel()
        {
            EditedInitiative = new Initiative();
            InitUsers = new List<User>();
            SelectedInitUsers = new List<int>();
            Tiles = new List<Tile>();
            SelectedTiles = new List<int>();
        }

        public void loadData()
        {
            //get userID from session
            int? UserID = HttpContext.Session.GetInt32("UserID");
            int initID = (int)HttpContext.Session.GetInt32("EditedInitID");

            EditedInitiative.InitID = initID;

            //populate initiatives list the user is a part of
            SqlDataReader reader = DBClass.EditedInitiativeReader(initID);
            while (reader.Read())
            {
                EditedInitiative = new Initiative
                {
                    InitID = int.Parse(reader["InitID"].ToString()),
                    InitName = reader["InitName"].ToString(),
                    FilePath = reader["FilePath"].ToString(),
                    InitDateTime = Convert.ToDateTime(reader["InitDateTime"])
                };
            }
            DBClass.ValleyVisionConnection.Close();
            // Close your connection in DBClass

            //Populate Users list
            SqlDataReader reader2 = DBClass.UsersReader(HttpContext.Session.GetInt32("UserID"));
            while (reader2.Read())
            {
                InitUsers.Add(new User
                {
                    UserID = Int32.Parse(reader2["UserID"].ToString()),
                    FirstName = reader2["firstName"].ToString(),
                    LastName = reader2["lastName"].ToString()
                });
            }
            DBClass.ValleyVisionConnection.Close();
            // Close your connection in DBClass

            //Populate Users list
            SqlDataReader reader3 = DBClass.InitiativeUsersReader(initID);
            while (reader3.Read())
            {
                SelectedInitUsers.Add(Int32.Parse(reader3["UserID"].ToString()));
            }
            DBClass.ValleyVisionConnection.Close();
            // Close your connection in DBClass

            //Populate Tile list
            SqlDataReader reader4 = DBClass.TilesReader();
            while (reader4.Read())
            {
                Tiles.Add(new Tile
                {
                    TileID = Int32.Parse(reader4["TileID"].ToString()),
                    TileName = reader4["TileName"].ToString()
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

            //Populate Tile list
            SqlDataReader reader5 = DBClass.TilesReader(initID);
            while (reader5.Read())
            {
                SelectedTiles.Add(Int32.Parse(reader5["TileID"].ToString()));
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
        }

        public void OnGet(int EditedInit)
        {
            loadData();
        }

        public IActionResult OnPostDeleteInit()
        {
            loadData();
            DBClass.DeleteInitiative(EditedInitiative.InitID);
            loadData();
            return RedirectToPage("/Initiatives/InitiativesPage");
        }

        public IActionResult OnPostEditInit()
        {
            
            DBClass.EditInit(EditedInitiative, EditedInitUsers, EditedTiles, HttpContext.Session.GetInt32("UserID"));
            loadData();
            return RedirectToPage("/Initiatives/InitiativesPage");
        }

    }
}
