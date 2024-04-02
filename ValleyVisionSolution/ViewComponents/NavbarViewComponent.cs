using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int initID)
        {
            int? UserID = HttpContext.Session.GetInt32("UserID");
            var tiles = new List<Tile>();
            var inits = new List<Initiative>();
            // Utilize DBClass to fetch tiles data based on initID
            SqlDataReader reader = DBClass.TilesReader(initID);
            while (reader.Read())
            {
                tiles.Add(new Tile
                {
                    TileID = int.Parse(reader["TileID"].ToString()),
                    TileName = reader["TileName"].ToString(),
                    IconPath = reader["IconPath"].ToString(),
                    PageLink = reader["PageLink"].ToString()
                });
            }
            reader.Close(); // Don't forget to close the reader
            DBClass.ValleyVisionConnection.Close(); // Close connection if it's not managed elsewhere

            SqlDataReader reader2 = DBClass.InitiativesReader((int)UserID);
            while (reader2.Read())
            {
                inits.Add(new Initiative
                {
                    InitID = int.Parse(reader2["initID"].ToString()),
                    InitName = reader2["initName"].ToString(),
                    FilePath = reader2["filePath"].ToString()
                });
            }
            reader.Close(); // Don't forget to close the reader
            DBClass.ValleyVisionConnection.Close(); // Close connection if it's not managed elsewhere

            return View((tiles, inits)); // Pass the list of tiles to the view
        }
    }
}


