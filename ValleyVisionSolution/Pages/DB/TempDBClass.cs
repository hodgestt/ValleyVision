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






        //edit a task
        public static void EditTask(DataClasses.Task editedTask, List<int> editedTaskUsers)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@TaskName", editedTask.TaskName);
            cmd.Parameters.AddWithValue("@TaskStatus", editedTask.TaskStatus);
            cmd.Parameters.AddWithValue("@TaskDescription", editedTask.TaskDescription);
            cmd.Parameters.AddWithValue("@TaskDueDateTime", editedTask.TaskDueDateTime);
            cmd.Parameters.AddWithValue("@TaskID", editedTask.TaskID);
            String sqlQuery = "UPDATE Task SET taskName = @TaskName, taskStatus = @TaskStatus, taskDescription = @TaskDescription, taskDueDateTime = @TaskDueDateTime WHERE taskID = @TaskID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@TaskID", editedTask.TaskID);
            String sqlQuery2 = "DELETE FROM TaskUsers WHERE taskID = @TaskID;";
            cmd2.CommandText = sqlQuery2;
            cmd2.Connection.Open();
            cmd2.ExecuteNonQuery();
            cmd2.Connection.Close();

            foreach (var user in editedTaskUsers)
            {
                SqlCommand cmd3 = new SqlCommand();
                cmd3.Connection = ValleyVisionConnection;
                cmd3.Connection.ConnectionString = MainConnString;
                cmd3.Parameters.AddWithValue("@TaskID", editedTask.TaskID);
                cmd3.Parameters.AddWithValue("@UserID", user);
                String sqlQuery3 = "INSERT INTO TaskUsers (taskID, userID) VALUES (@TaskID, @UserID);";
                cmd3.CommandText = sqlQuery3;
                cmd3.Connection.Open();
                cmd3.ExecuteNonQuery();
                cmd3.Connection.Close();
            }
        }


        







    }
}
