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
            var tiles = new List<Tile>();
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

            return View(tiles); // Pass the list of tiles to the view
        }
    }
}


