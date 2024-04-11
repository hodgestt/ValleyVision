using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using System;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using ValleyVisionSolution.Services;

namespace ValleyVisionSolution.Pages.Initiatives
{
    public class InitiativesPageModel : PageModel
    {
        public List<Initiative>? InitiativesList { get; set; } = new List<Initiative>();

        [BindProperty]
        public Initiative NewInit { get; set; }

        [BindProperty]
        public IFormFile BackgroundFile { get; set; }

        [BindProperty]
        public List<int> NewInitUsers { get; set; }

        [BindProperty]
        public List<User> InitUsers { get; set; } = new List<User>();

        [BindProperty]
        public List<Tile> Tiles { get; set; } = new List<Tile>();

        [BindProperty]
        public List<int> NewTiles { get; set; }

        [BindProperty]
        public int EditedInit { get; set; }

        public bool OpenModal { get; set; } = false;

        private readonly IBlobService _blobService;

        public InitiativesPageModel(IBlobService blobService)
        {
            _blobService = blobService;
        }


        public void loadData()
        {
            //get userID from session
            int? UserID = HttpContext.Session.GetInt32("UserID");


            //populate initiatives list the user is a part of
            SqlDataReader reader = DBClass.InitiativesReader((int)UserID);
            while (reader.Read())
            {
                InitiativesList.Add(new Initiative
                {
                    InitID = int.Parse(reader["InitID"].ToString()),
                    InitName = reader["InitName"].ToString(),
                    FilePath = reader["FilePath"].ToString(),
                    InitDateTime = Convert.ToDateTime(reader["InitDateTime"])
                });
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


            //Populate Tile list
            SqlDataReader reader3 = DBClass.TilesReader();
            while (reader3.Read())
            {
                Tiles.Add(new Tile
                {
                    TileID = Int32.Parse(reader3["TileID"].ToString()),
                    TileName = reader3["TileName"].ToString()
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

        }

        public IActionResult OnGet()
        {
            HttpContext.Session.Remove("InitName");
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                loadData();
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
            
        }

        public async Task<IActionResult> OnPostAddNewInit()
        {
            if (!ModelState.IsValid)
            {
                // Model state is not valid, return the page with validation errors
                loadData();
                OpenModal = true;
                return Page();
            }

            if (BackgroundFile != null && BackgroundFile.Length > 0)
            {
                // Generate a unique file name to avoid overwriting existing files
                var fileName = Path.GetFileNameWithoutExtension(BackgroundFile.FileName);
                var fileExtension = Path.GetExtension(BackgroundFile.FileName);
                var uniqueFileName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;

                // Use the IBlobService to upload the file
                using (var fileStream = BackgroundFile.OpenReadStream())
                {
                    await _blobService.UploadFileBlobAsync(uniqueFileName, fileStream, BackgroundFile.ContentType);
                }

                // Model state is valid, continue with processing
                NewInit.FilePath = uniqueFileName;
                DBClass.AddInit(NewInit, NewInitUsers, NewTiles, HttpContext.Session.GetInt32("UserID"));
                loadData();
                ModelState.Clear();
                NewInit = new Initiative();
                NewInitUsers = new List<int>();
                NewTiles = new List<int>();

                return Page();
            }
            return Page();
        }


        //public async Task<IActionResult> OnPostAsync(IFormFile file)
        //{
            
        //    if (file != null && file.Length > 0)
        //    {
        //        // Generate a unique file name to avoid overwriting existing files
        //        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        //        var fileExtension = Path.GetExtension(file.FileName);
        //        var uniqueFileName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;

        //        // Use the IBlobService to upload the file
        //        using (var fileStream = file.OpenReadStream())
        //        {
        //            await _blobService.UploadFileBlobAsync(uniqueFileName, fileStream, file.ContentType);
        //        }

        //        // Model state is valid, continue with processing
        //        NewInit.FilePath = uniqueFileName;
        //        DBClass.AddInit(NewInit, NewInitUsers, NewTiles, HttpContext.Session.GetInt32("UserID"));
        //        loadData();
        //        ModelState.Clear();
        //        NewInit = new Initiative();
        //        NewInitUsers = new List<int>();
        //        NewTiles = new List<int>();

        //        return Page();
        //    }

        //    ViewData["ErrorMessage"] = "You must select a file.";
        //    return Page();
        //}

        public IActionResult OnPostEditInit()
        {
            HttpContext.Session.SetInt32("EditedInitID", EditedInit);
            return RedirectToPage("EditInitiativesPage");
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        
    }
}
