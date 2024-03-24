using ValleyVisionSolution.Pages.DataClasses;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static BenchmarkDotNet.Engines.EngineEventSource;

namespace ValleyVisionSolution.Pages.DB
{
    public class TempDBClass
    {
        // Connection Object at Data Field Level
        public static SqlConnection ValleyVisionConnection = new SqlConnection();

        // Connection String - How to find and connect to DB
        private static readonly String? MainConnString = "Server=Localhost;Database=Main;Trusted_Connection=True";
        private static readonly String? AuthConnString = "Server=Localhost;Database=AUTH;Trusted_Connection=True";

        //reads all users that are part of a given initiative
        public static List<int> ViewedTaskUsersReader(int? taskID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@TaskID", taskID);
            cmd.CommandText = "SELECT U.userID FROM User_ U JOIN TaskUsers TU ON U.userID = TU.userID WHERE TU.taskID = @TaskID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            List<int> ViewedTaskUsers = new List<int>();

            while (tempReader.Read())
            {
                ViewedTaskUsers.Add(Int32.Parse(tempReader["UserID"].ToString()));
            }

            cmd.Connection.Close();

            return ViewedTaskUsers;
        }
    }
}
