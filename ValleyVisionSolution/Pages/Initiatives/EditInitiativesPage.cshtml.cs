using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.Net;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using ValleyVisionSolution.Services;

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

        [BindProperty]
        public IFormFile BackgroundFile { get; set; }


        private readonly IBlobService _blobService;

        public EditInitiativesPageModel(IBlobService blobService)
        {
            EditedInitiative = new Initiative();
            InitUsers = new List<User>();
            SelectedInitUsers = new List<int>();
            Tiles = new List<Tile>();
            SelectedTiles = new List<int>();

            _blobService = blobService;

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

        public async Task<IActionResult> OnPostEditInitAsync()
        {
            if (EditedInitiative.InitName == null)
            {
                loadData();
                return Page();
            }

            if (BackgroundFile != null && BackgroundFile.Length > 0)
            {
                // Generate a unique file name to avoid overwriting existing files
                var fileName = Path.GetFileNameWithoutExtension(BackgroundFile.FileName);
                var fileExtension = Path.GetExtension(BackgroundFile.FileName);
                var uniqueFileName = $"{fileName}{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                // Upload the file to Azure Blob Storage
                string blobUrl;
                using (var fileStream = BackgroundFile.OpenReadStream())
                {
                    // Assuming UploadFileBlobAsync uploads the file and returns the full Blob URL
                    await _blobService.UploadFileBlobAsync(uniqueFileName, fileStream, BackgroundFile.ContentType);
                }

                // If the _blobService.UploadFileBlobAsync method doesn't return a URL, construct it manually
                string storageAccountUrl = "https://valleyvisionstorage2.blob.core.windows.net/";
                string containerName = "uploads";
                blobUrl = $"{storageAccountUrl}{containerName}/{uniqueFileName}";

                EditedInitiative.FilePath = blobUrl;
            }
            DBClass.EditInit(EditedInitiative, EditedInitUsers, EditedTiles, HttpContext.Session.GetInt32("UserID"));
            loadData();
            return RedirectToPage("/Initiatives/InitiativesPage");
        }

    }
}
