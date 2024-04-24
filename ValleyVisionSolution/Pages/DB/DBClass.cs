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
using System.Net;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using Microsoft.AspNetCore.SignalR;
using Task = ValleyVisionSolution.Pages.DataClasses.Task;
using DocumentFormat.OpenXml.InkML;

namespace ValleyVisionSolution.Pages.DB
{
    public class DBClass
    {
        // Connection Object at Data Field Level
        public static SqlConnection ValleyVisionConnection = new SqlConnection();

        //// Connection String - How to find and connect to DB
        private static readonly String? MainConnString = "Server=Localhost;Database=Main;Trusted_Connection=True";
        private static readonly String? AuthConnString = "Server=Localhost;Database=AUTH;Trusted_Connection=True";

        //Azure hosted database connection strings 
        //private static readonly String? MainConnString = "Server=valleyvisioncapstone2.database.windows.net,1433;" +
        //"Database=ValleyVisionMain;" +
        //"User ID=ValleyVisionAdmin;" +
        //"Password=CIS484ValleyVision;" +
        //"Encrypt=True;" +
        //"TrustServerCertificate=True;" +
        //"Connection Timeout=30;";

        //private static readonly String? AuthConnString = "Server=valleyvisioncapstone3.database.windows.net,1433;" +
        //"Database=ValleyVisionAuth;" +
        //"User ID=VallyVisionAdmin;" +
        //"Password=CIS484ValleyVision;" +
        //"Encrypt=True;" +
        //"TrustServerCertificate=True;" +
        //"Connection Timeout=30;";





        //BEGIN LOGIN PAGE____________________________________________________________________________________________________
        //checks if there is a user that matches the credentials. If yes returns userID, if no return -1
        public static int HashedParameterLogin(HashedCredential UserCredentials)
        {
            int UserID = -1;

            SqlCommand cmdLogin = new SqlCommand();
            cmdLogin.Connection = ValleyVisionConnection;
            cmdLogin.Connection.ConnectionString = AuthConnString;
            cmdLogin.CommandType = System.Data.CommandType.StoredProcedure;

            cmdLogin.CommandText = "sp_Login";
            cmdLogin.Parameters.AddWithValue("@Username", UserCredentials.Username);

            cmdLogin.Connection.Open();

            SqlDataReader hashReader = cmdLogin.ExecuteReader();

            if (hashReader.Read())
            {
                string correctHash = hashReader["Password_"].ToString();
                cmdLogin.Connection.Close();

                if (PasswordHash.ValidatePassword(UserCredentials.Password, correctHash))
                {

                    SqlCommand cmd2 = new SqlCommand();
                    cmd2.Connection = ValleyVisionConnection;
                    cmd2.Connection.ConnectionString = AuthConnString;
                    cmd2.Parameters.AddWithValue("@Username", UserCredentials.Username);
                    cmd2.Parameters.AddWithValue("@Password", correctHash);
                    cmd2.CommandText = "SELECT HC.UserID FROM HashedCredentials HC WHERE HC.Username = @Username AND HC.Password_ = @Password;";
                    cmd2.Connection.Open(); // Open connection here, close in Model!
                    SqlDataReader tempReader2 = cmd2.ExecuteReader();
                    while (tempReader2.Read())
                    {
                        UserID = Int32.Parse(tempReader2["UserID"].ToString());
                    }
                    cmd2.Connection.Close();
                    return UserID;
                }
            }
            cmdLogin.Connection.Close();
            return UserID;
        }
        //END LOGIN PAGE_________________________________________________________________________________________________



        //BEGIN INITIATIVES PAGE_________________________________________________________________________________________
        //reads all the initiatives that the user is a part of
        public static SqlDataReader InitiativesReader(int userID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", userID);
            cmd.CommandText = "SELECT DISTINCT I.initID, I.initName, I.filePath, I.initDateTime FROM Initiative I JOIN InitiativeUsers IU ON IU.initID = I.initID WHERE IU.userID = @UserID;";
            cmd.Connection.Open(); // Open connection here, close in Model! 

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        //delete initiative 
        public static void DeleteInitiative(int? initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);
            String sqlQuery = "DELETE FROM TaskUsers WHERE TaskID IN (SELECT TaskID FROM Task WHERE InitID = @InitID) " + "DELETE FROM Task WHERE initID = @InitID " + "DELETE FROM Message_ WHERE initID = @InitID " + "DELETE FROM InitiativeUsers WHERE initID=@InitID " + "DELETE FROM Initiativefiles WHERE initID=@InitID " + "DELETE FROM InitiativeTiles WHERE initID=@InitID " + "DELETE FROM Initiative WHERE initID=@InitID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

        }

        public static SqlDataReader EditedInitiativeReader(int initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);
            cmd.CommandText = "SELECT DISTINCT I.initID, I.initName, I.filePath, I.initDateTime FROM Initiative I WHERE I.initID = @InitID;";
            cmd.Connection.Open(); // Open connection here, close in Model! 

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }
        //adds a new initiative with selected tiles and users with the current user as a default
        public static void EditInit(Initiative EditedInit, List<int> EditedInitUsers, List<int> EditedTiles, int? CurrentUserID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", EditedInit.InitID);
            cmd.Parameters.AddWithValue("@InitName", EditedInit.InitName);
            cmd.Parameters.AddWithValue("@InitDateTime", EditedInit.InitDateTime);
            cmd.Parameters.AddWithValue("@FilePath", EditedInit.FilePath);
            String sqlQuery = "UPDATE Initiative SET InitName = @InitName, filePath = @FilePath, InitDateTime = @InitDateTime WHERE initID = @InitID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@InitID", EditedInit.InitID);
            String sqlQuery2 = "DELETE FROM InitiativeUsers WHERE initID = @InitID;";
            cmd2.CommandText = sqlQuery2;
            cmd2.Connection.Open();
            cmd2.ExecuteNonQuery();
            cmd2.Connection.Close();

            EditedInitUsers.Add((int)CurrentUserID); //adds the admin ID into each init by default

            foreach (var userID in EditedInitUsers)
            {
                SqlCommand cmd3 = new SqlCommand();
                cmd3.Connection = ValleyVisionConnection;
                cmd3.Connection.ConnectionString = MainConnString;
                cmd3.Parameters.AddWithValue("@InitID", EditedInit.InitID);
                cmd3.Parameters.AddWithValue("@UserID", userID);
                String sqlQuery3 = "INSERT INTO InitiativeUsers (initID, userID) VALUES (@InitID, @UserID);";
                cmd3.CommandText = sqlQuery3;
                cmd3.Connection.Open();
                cmd3.ExecuteNonQuery();
                cmd3.Connection.Close();
            }

            SqlCommand cmd4 = new SqlCommand();
            cmd4.Connection = ValleyVisionConnection;
            cmd4.Connection.ConnectionString = MainConnString;
            cmd4.Parameters.AddWithValue("@InitID", EditedInit.InitID);
            String sqlQuery4 = "DELETE FROM InitiativeTiles WHERE initID = @InitID;";
            cmd4.CommandText = sqlQuery4;
            cmd4.Connection.Open();
            cmd4.ExecuteNonQuery();
            cmd4.Connection.Close();

            foreach (var tile in EditedTiles)
            {
                SqlCommand cmd3 = new SqlCommand();
                cmd3.Connection = ValleyVisionConnection;
                cmd3.Connection.ConnectionString = MainConnString;
                cmd3.Parameters.AddWithValue("@TileID", tile);
                cmd3.Parameters.AddWithValue("@InitID", EditedInit.InitID);
                String sqlQuery5 = "INSERT INTO InitiativeTiles (tileID, initID) VALUES (@TileID, @InitID);";
                cmd3.CommandText = sqlQuery5;
                cmd3.Connection.Open();
                cmd3.ExecuteNonQuery();
                cmd3.Connection.Close();
            }

        }



        public static List<Revenue> GetChartDataFromDatabase()
        {
            List<Revenue> dataList = new List<Revenue>();

            using (SqlConnection connection = new SqlConnection(MainConnString))
            {
                string sqlQuery = "SELECT year_, realEstateTax, personalPropertyTax, feesLicensesTax, otherRevenue, totalRevenue FROM DataFile_2;";
                SqlCommand command = new SqlCommand(sqlQuery, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Revenue dataItem = new Revenue();
                    dataItem.Year = int.Parse(reader["year_"].ToString());
                    dataItem.RealEstateTax = Convert.ToDecimal(reader["realEstateTax"]);
                    dataItem.PersonalPropertyTax = Convert.ToDecimal(reader["personalPropertyTax"]);
                    dataItem.FeesLicensesTax = Convert.ToDecimal(reader["feesLicensesTax"]);
                    dataItem.StateFunding = Convert.ToDecimal(reader["otherRevenue"]);
                    dataItem.TotalRevenue = Convert.ToDecimal(reader["totalRevenue"]);
                    dataList.Add(dataItem);
                }
            }

            return dataList;
        }

        //reads all users for INITIATIVE PAGE_________________________________________________________________________________________
        public static SqlDataReader UsersReader(int? userID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", userID);
            cmd.CommandText = "SELECT U.userID, U.firstName, U.lastName FROM User_ U WHERE U.UserID != @UserID";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }
        //All tiles reader
        public static SqlDataReader TilesReader()
        {
            SqlCommand cmd4 = new SqlCommand();
            cmd4.Connection = ValleyVisionConnection;
            cmd4.Connection.ConnectionString = MainConnString;
            cmd4.CommandText = "SELECT * FROM Tile T;";
            cmd4.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd4.ExecuteReader();

            return tempReader;
        }

        //adds a new initiative with selected tiles and users with the current user as a default
        public static void AddInit(Initiative NewInit, List<int> NewInitUsers, List<int> NewTiles, int? CurrentUserID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitName", NewInit.InitName);
            cmd.Parameters.AddWithValue("@InitDateTime", NewInit.InitDateTime);
            cmd.Parameters.AddWithValue("@FilePath", NewInit.FilePath);
            String sqlQuery = "INSERT INTO Initiative (initName, filePath ,initDateTime) VALUES (@InitName, @FilePath, @InitDateTime);" + "SELECT SCOPE_IDENTITY();";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            NewInit.InitID = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Connection.Close();

            NewInitUsers.Add((int)CurrentUserID); //adds the admin ID into each init by default

            foreach (var user in NewInitUsers)
            {
                SqlCommand cmd2 = new SqlCommand();
                cmd2.Connection = ValleyVisionConnection;
                cmd2.Connection.ConnectionString = MainConnString;
                cmd2.Parameters.AddWithValue("@InitID", NewInit.InitID);
                cmd2.Parameters.AddWithValue("@UserID", user);
                String sqlQuery2 = "INSERT INTO InitiativeUsers (initID, userID) VALUES (@InitID, @UserID);";
                cmd2.CommandText = sqlQuery2;
                cmd2.Connection.Open();
                cmd2.ExecuteNonQuery();
                cmd2.Connection.Close();
            }

            foreach (var tile in NewTiles)
            {
                SqlCommand cmd3 = new SqlCommand();
                cmd3.Connection = ValleyVisionConnection;
                cmd3.Connection.ConnectionString = MainConnString;
                cmd3.Parameters.AddWithValue("@TileID", tile);
                cmd3.Parameters.AddWithValue("@InitID", NewInit.InitID);
                String sqlQuery3 = "INSERT INTO InitiativeTiles (tileID, initID) VALUES (@TileID, @InitID);";
                cmd3.CommandText = sqlQuery3;
                cmd3.Connection.Open();
                cmd3.ExecuteNonQuery();
                cmd3.Connection.Close();
            }
            
        }

        //delete initiative 
        public static void DeleteInitiative(int initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);
            String sqlQuery = "DELETE FROM TaskUsers WHERE TaskID IN (SELECT TaskID FROM Task WHERE InitID = @InitID;" + "DELETE FROM Task WHERE initID = @InitID;" + "DELETE FROM Message_ WHERE initID = @InitID;" + "DELETE FROM InitiativeUsers WHERE initID=@InitID;" + "DELETE FROM Initiativefiles WHERE initID=@InitID;" + "DELETE FROM Initiativetiles WHERE initID=@InitID;" + "DELETE FROM Initiative WHERE initID=@InitID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

        }


        //END INITIATIVES PAGE


        //BEGIN DASHBOARD PAGE___________________________________________________________________________________________
        //reads all of the tiles that are activated for an initiative
        public static SqlDataReader TilesReader(int initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);
            cmd.CommandText = "SELECT DISTINCT T.tileID, T.tileName, T.iconPath, T.pageLink FROM Tile T JOIN InitiativeTiles IT ON T.tileID = IT.tileID WHERE IT.initID = @InitID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }
        //END DASHBOARD PAGE_____________________________________________________________________________________________

        //BEGIN Resources Page
        public static SqlDataReader ResourceReader(int initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);
            cmd.CommandText = "SELECT DISTINCT F.fileMetaID, F.fileName_, F.filePath, F.fileType, F.uploadedDateTime, F.userID, F.published, U.firstName, U.lastName FROM FileMeta F JOIN InitiativeFiles IT ON F.fileMetaID = IT.fileMetaID JOIN User_ U ON F.userID = U.userID WHERE IT.initID = @InitID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }


        public static SqlDataReader PublishReader()
        {
            var tempConnection = new SqlConnection(MainConnString); // Create a new connection instance
            SqlCommand cmd = new SqlCommand
            {
                Connection = tempConnection,
                CommandText = "SELECT * FROM FileMeta WHERE published = 'yes'"
            };

            try
            {
                cmd.Connection.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection); // Ensure the connection is closed when the SqlDataReader is closed
            }
            catch
            {
                cmd.Connection.Close(); // Ensure connection is closed in case of an exception
                throw;
            }
        }

        //public static SqlDataReader PublishReader()
        //{
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.Connection = ValleyVisionConnection;
        //    cmd.Connection.ConnectionString= MainConnString;
        //    cmd.CommandText = "SELECT * FROM FileMeta WHERE published = 'yes'";
        //    cmd.Connection.Open ();
        //    SqlDataReader tempReader = cmd.ExecuteReader();
        //    return tempReader;
        //}

        public static void PublishFile(int fileID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@fileID", fileID);
            cmd.Parameters.AddWithValue("@publishdate", DateTime.Now);
            String sqlQuery = "UPDATE FileMeta SET published = 'yes', publishdate = @publishdate WHERE FileMetaID = @fileID";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static void UnPublishFile(int fileID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@fileID", fileID);
            String sqlQuery = "UPDATE FileMeta SET published = 'no' WHERE FileMetaID = @fileID";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static void UploadFile(int? initID, FileMeta fileMeta)
        {
            int fileID;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@FileName", fileMeta.FileName_);
            cmd.Parameters.AddWithValue("@FilePath", fileMeta.FilePath);
            cmd.Parameters.AddWithValue("@FileType", fileMeta.FileType);
            cmd.Parameters.AddWithValue("@Date", fileMeta.UploadedDateTime);
            cmd.Parameters.AddWithValue("@UserID", fileMeta.userID);
            String sqlQuery = "INSERT INTO FileMeta (fileName_, filePath, fileType, uploadedDateTime, userID) VALUES (@FileName, @FilePath, @FileType, @Date, @UserID)" + "SELECT SCOPE_IDENTITY();";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            fileID = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Connection.Close();

            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@initID", initID);
            cmd2.Parameters.AddWithValue("@fileID", fileID);
            String sqlQuery2 = "INSERT INTO InitiativeFiles (fileMetaID, initID) VALUES (@fileID, @initID)";
            cmd2.CommandText = sqlQuery2;
            cmd.Connection.Open();
            cmd2.ExecuteNonQuery();
            cmd2.Connection.Close();

        }

        public static void DeleteFile(int fileID)
        {
            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@FileID", fileID);
            String sqlQuery2 = "DELETE FROM InitiativeFiles WHERE FileMetaID = @FileID";
            cmd2.CommandText = sqlQuery2;
            cmd2.Connection.Open();
            cmd2.ExecuteNonQuery();
            cmd2.Connection.Close();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@FileID", fileID);
            String sqlQuery = "DELETE FROM FileMeta WHERE FileMetaID = @FileID";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }



        // END RESOURCES PAGE
        //BEGIN TASK MANAGER PAGE________________________________________________________________________________________
        //reads all tasks associated with a specific initiative
        public static SqlDataReader AllTasksReader(int? initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);
            cmd.CommandText = "SELECT DISTINCT T.taskID, T.taskName, T.taskStatus, T.taskDescription, T.taskDueDateTime, T.initID FROM Task T JOIN Initiative I ON T.initID = I.initID WHERE I.initID = @InitID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        public static SqlDataReader AllDevsReader(int? devID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@DevID", devID);
            cmd.CommandText = "SELECT DISTINCT * FROM DevelopmentArea;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        //reads all tasks associated with a specific user in the specific initiative
        public static SqlDataReader MyTasksReader(int? userID, int? initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", userID);
            cmd.Parameters.AddWithValue("@InitID", initID);
            cmd.CommandText = "SELECT DISTINCT T.taskID, T.taskName, T.taskStatus, T.taskDescription, T.taskDueDateTime, T.initID FROM Task T JOIN Initiative I ON T.initID = I.initID JOIN TaskUsers TU ON TU.taskID = T.taskID JOIN User_ U ON U.userID = TU.userID WHERE I.initID = @InitID AND U.userID = @UserID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }
        //reads all users that are part of a given initiative
        public static SqlDataReader InitiativeUsersReader(int? initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);
            cmd.CommandText = "SELECT U.userID, U.firstName, U.lastName FROM User_ U JOIN InitiativeUsers IU ON U.userID = IU.userID WHERE IU.initID = @InitID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        //adds a new task in a specific initiative and assigns it to the specified users
        public static void AddTask(int? initID, DataClasses.Task newTask, List<int> newTaskUsers)
        {
            int taskID;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@TaskName", newTask.TaskName);
            cmd.Parameters.AddWithValue("@TaskStatus", newTask.TaskStatus);
            cmd.Parameters.AddWithValue("@TaskDescription", newTask.TaskDescription);
            cmd.Parameters.AddWithValue("@TaskDueDateTime", newTask.TaskDueDateTime);
            cmd.Parameters.AddWithValue("@InitID", initID);
            String sqlQuery = "INSERT INTO Task (taskName, taskStatus, taskDescription, taskDueDateTime, initID) VALUES (@TaskName, @TaskStatus, @TaskDescription, @TaskDueDateTime, @InitID);" + "SELECT SCOPE_IDENTITY();";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            taskID = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Connection.Close();

            foreach (var user in newTaskUsers)
            {
                SqlCommand cmd2 = new SqlCommand();
                cmd2.Connection = ValleyVisionConnection;
                cmd2.Connection.ConnectionString = MainConnString;
                cmd2.Parameters.AddWithValue("@TaskID", taskID);
                cmd2.Parameters.AddWithValue("@UserID", user);
                String sqlQuery2 = "INSERT INTO TaskUsers (taskID, userID) VALUES (@TaskID, @UserID);";
                cmd2.CommandText = sqlQuery2;
                cmd2.Connection.Open();
                cmd2.ExecuteNonQuery();
                cmd2.Connection.Close();
            }
        }

        public static SqlDataReader GetTasksByUserId(int userId)
        {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = ValleyVisionConnection;
                cmd.Connection.ConnectionString = MainConnString;
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.CommandText = "SELECT T.taskID, T.taskName, T.taskDueDateTime, T.initID FROM Task T INNER JOIN TaskUsers U on T.taskID = U.taskID WHERE U.userID = @UserID;";
                cmd.Connection.Open(); // Open connection here, close in Model!

                SqlDataReader tempReader = cmd.ExecuteReader();

                return tempReader;
        }
            


        //reads all users that have been assigned a given task
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

        public static List<int> ViewedDevUsersReader(int? devID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@DevID", devID);
            cmd.CommandText = "SELECT U.userID FROM User_ U JOIN DevelopmentArea D ON U.userID = D.userID WHERE D.devID = @devID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            List<int> ViewedDevUsers = new List<int>();

            while (tempReader.Read())
            {
                ViewedDevUsers.Add(Int32.Parse(tempReader["UserID"].ToString()));
            }

            cmd.Connection.Close();

            return ViewedDevUsers;
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


        public static void EditDev(DataClasses.DevelopmentArea editedDev)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@DevName", editedDev.devName);
            cmd.Parameters.AddWithValue("@DevDescription", editedDev.devDescription);
            cmd.Parameters.AddWithValue("@DevImpactLevel", editedDev.devImpactLevel);
            cmd.Parameters.AddWithValue("@DevID", editedDev.devID);
            String sqlQuery = "UPDATE DevelopmentArea SET devName = @DevName, devDescription = @DevDescription, devImpactLevel = @DevImpactLevel WHERE devID = @DevID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }





        //reads and loads the previous messages sent from other members in the disucssion board
        public static SqlDataReader MessagesReader(int initID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@InitID", initID);

            cmd.CommandText = "SELECT M.messageID, M.messageContent, M.messageDateTime, M.userID, M.initID, U.firstName, U.lastName FROM Message_ M JOIN User_ U ON U.userID = M.userID WHERE M.initID = @InitID ORDER BY M.messageDateTime ASC;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }
        //adds message in the discussion board
        public static void AddMessage(Message NewMessage)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@MessageContent", NewMessage.MessageContent);
            cmd.Parameters.AddWithValue("@MessageDateTime", NewMessage.MessageDateTime);
            cmd.Parameters.AddWithValue("@UserID", NewMessage.UserID);
            cmd.Parameters.AddWithValue("@InitID", NewMessage.InitID);
            String sqlQuery = "INSERT INTO Message_ (messageContent, messageDateTime, userID, initID) VALUES (@MessageContent, @MessageDateTime, @UserID, @initID);";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }
        //END TASK MANAGER PAGE__________________________________________________________________________________________



        //BEGIN MANAGE PROFILES PAGE_________________________________________________________________________________________
        //reads all user data in the system for the admin to see
        public static List<FullProfile> FullProfilesReader()
        {
            List<FullProfile> MainProfilesInfo = new List<FullProfile>();   

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT U.UserID, U.firstName, U.lastName, U.email, U.phone, U.userType, A.street, A.apartment, A.city, A.state_, A.zip, A.country FROM User_ U JOIN Address_ A ON A.AddressID = U.AddressID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            while (tempReader.Read())
            {
                MainProfilesInfo.Add(new FullProfile
                {
                    UserID = int.Parse(tempReader["UserID"].ToString()),
                    FirstName = tempReader["FirstName"].ToString(),
                    LastName = tempReader["LastName"].ToString(),
                    Email = tempReader["Email"].ToString(),
                    Phone = tempReader["Phone"].ToString(),
                    UserType = tempReader["UserType"].ToString(),
                    Street = tempReader["Street"].ToString(),
                    Apartment = tempReader["Apartment"].ToString(),
                    City = tempReader["City"].ToString(),
                    State = tempReader["State_"].ToString(),
                    Zip = int.Parse(tempReader["Zip"].ToString()),
                    Country = tempReader["Country"].ToString()
                });
            }
            cmd.Connection.Close();

            List<HashedCredential> AuthProfilesInfo = new List<HashedCredential>();

            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = AuthConnString;
            cmd2.CommandText = "SELECT * FROM HashedCredentials;";
            cmd2.Connection.Open(); 

            SqlDataReader tempReader2 = cmd2.ExecuteReader();

            while (tempReader2.Read())
            {
                AuthProfilesInfo.Add(new HashedCredential
                {
                    UserID = int.Parse(tempReader2["UserID"].ToString()),
                    Username = tempReader2["UserName"].ToString(),
                    Password = tempReader2["Password_"].ToString()
                    
                });
            }
            cmd2.Connection.Close();

            List<FullProfile> FullProfileList = new List<FullProfile>();
            
            foreach(var allUser in MainProfilesInfo)
            {
                foreach(var activeUser in AuthProfilesInfo)
                {
                    if(allUser.UserID == activeUser.UserID)
                    {
                        FullProfileList.Add(new FullProfile
                        {
                            UserID = allUser.UserID,
                            UserName = activeUser.Username,
                            FirstName = allUser.FirstName,
                            LastName = allUser.LastName,
                            Email = allUser.Email,
                            Phone = allUser.Phone,
                            UserType = allUser.UserType,
                            Street = allUser.Street,
                            Apartment = allUser.Apartment,
                            City = allUser.City,
                            State = allUser.State,
                            Zip = allUser.Zip,
                            Country = allUser.Country
                        });
                        break;
                    }
                }
              
            }

            return FullProfileList;
        }
        public static FullProfile SingleProfilesReader(int? UserID)
        {
            FullProfile MainProfileInfo = new FullProfile();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.CommandText = "SELECT U.UserID, U.firstName, U.lastName, U.email, U.phone, U.userType, A.street, A.apartment, A.city, A.state_, A.zip, A.country FROM User_ U JOIN Address_ A ON A.AddressID = U.AddressID WHERE U.userID = @UserID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            if (tempReader.Read())
            {
                MainProfileInfo.UserID = int.Parse(tempReader["UserID"].ToString());
                MainProfileInfo.FirstName = tempReader["FirstName"].ToString();
                MainProfileInfo.LastName = tempReader["LastName"].ToString();
                MainProfileInfo.Email = tempReader["Email"].ToString();
                MainProfileInfo.Phone = tempReader["Phone"].ToString();
                MainProfileInfo.UserType = tempReader["UserType"].ToString();
                MainProfileInfo.Street = tempReader["Street"].ToString();
                MainProfileInfo.Apartment = tempReader["Apartment"].ToString();
                MainProfileInfo.City = tempReader["City"].ToString();
                MainProfileInfo.State = tempReader["State_"].ToString();
                MainProfileInfo.Zip = int.Parse(tempReader["Zip"].ToString());
                MainProfileInfo.Country = tempReader["Country"].ToString();
            }
            cmd.Connection.Close();

            HashedCredential AuthProfilesInfo = new HashedCredential();

            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = AuthConnString;
            cmd2.Parameters.AddWithValue("@UserID", UserID);
            cmd2.CommandText = "SELECT * FROM HashedCredentials WHERE UserID = @UserID;";
            cmd2.Connection.Open();

            SqlDataReader tempReader2 = cmd2.ExecuteReader();

            if (tempReader2.Read())
            {
                AuthProfilesInfo.UserID = int.Parse(tempReader2["UserID"].ToString());
                AuthProfilesInfo.Username = tempReader2["UserName"].ToString();
                AuthProfilesInfo.Password = tempReader2["Password_"].ToString();
            }
            cmd2.Connection.Close();

            FullProfile SingleProfile = new FullProfile();

            SingleProfile.UserID = MainProfileInfo.UserID;
            SingleProfile.UserName = AuthProfilesInfo.Username;
            SingleProfile.FirstName = MainProfileInfo.FirstName;
            SingleProfile.LastName = MainProfileInfo.LastName;
            SingleProfile.Email = MainProfileInfo.Email;
            SingleProfile.Phone = MainProfileInfo.Phone;
            SingleProfile.UserType = MainProfileInfo.UserType;
            SingleProfile.Street = MainProfileInfo.Street;
            SingleProfile.Apartment = MainProfileInfo.Apartment;
            SingleProfile.City = MainProfileInfo.City;
            SingleProfile.State = MainProfileInfo.State;
            SingleProfile.Zip = MainProfileInfo.Zip;
            SingleProfile.Country = MainProfileInfo.Country;
           
            return SingleProfile;
        }

        public static void DeleteUser(int? userid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = AuthConnString;
            cmd.Parameters.AddWithValue("@UserID", userid);
            String sqlQuery = "DELETE FROM HashedCredentials WHERE UserID = @UserID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static void DeleteDevelopmentFiles(int devID, int fileMetaID)

        {

            SqlCommand cmd = new SqlCommand();

            cmd.Connection = ValleyVisionConnection;

            cmd.Connection.ConnectionString = MainConnString;

            cmd.Parameters.AddWithValue("@DevID", devID);

            cmd.Parameters.AddWithValue("@fileMetaID", fileMetaID);

            String sqlQuery = "DELETE FROM DevAreaFiles WHERE fileMetaID = @fileMetaID AND devID = @DevID;";

            cmd.CommandText = sqlQuery;

            cmd.Connection.Open();

            cmd.ExecuteNonQuery();

            cmd.Connection.Close();

        }

        public static void DeleteTask(int? taskID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@TaskID", taskID);
            String sqlQuery = "DELETE FROM TaskUsers WHERE taskID = @TaskID;" + "DELETE FROM Task WHERE taskID = @TaskID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        //editing profiles
        public static void UpdateProfile(FullProfile profiletoedit)
        {
            int addressID = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", profiletoedit.UserID);
            String sqlQuery = "SELECT U.addressID FROM User_ U WHERE U.userID = @UserID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            SqlDataReader tempReader = cmd.ExecuteReader();
            if(tempReader.Read())
            {
                addressID = Int32.Parse(tempReader["AddressID"].ToString());
            }
            cmd.Connection.Close();

            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@AddressID", addressID);
            cmd2.Parameters.AddWithValue("@Street", profiletoedit.Street);
            cmd2.Parameters.AddWithValue("@Apartment", profiletoedit.Apartment);
            cmd2.Parameters.AddWithValue("@City", profiletoedit.City);
            cmd2.Parameters.AddWithValue("@State", profiletoedit.State);
            cmd2.Parameters.AddWithValue("@Zip", profiletoedit.Zip);
            cmd2.Parameters.AddWithValue("@Country", profiletoedit.Country);
            String sqlQuery2 = "UPDATE Address_ SET street = @Street, apartment = @Apartment, city = @City, state_ = @State, zip = @Zip, country = @Country WHERE addressID = @AddressID;";
            cmd2.CommandText = sqlQuery2;
            cmd2.Connection.Open();
            cmd2.ExecuteNonQuery();
            cmd2.Connection.Close();


           
            SqlCommand cmd3 = new SqlCommand();
            cmd3.Connection = ValleyVisionConnection;
            cmd3.Connection.ConnectionString = MainConnString;
            cmd3.Parameters.AddWithValue("@UserID", profiletoedit.UserID);
            cmd3.Parameters.AddWithValue("@FirstName", profiletoedit.FirstName);
            cmd3.Parameters.AddWithValue("@LastName", profiletoedit.LastName);
            cmd3.Parameters.AddWithValue("@Email", profiletoedit.Email);
            cmd3.Parameters.AddWithValue("@Phone", profiletoedit.Phone);
            cmd3.Parameters.AddWithValue("@UserType", profiletoedit.UserType);
            cmd3.Parameters.AddWithValue("@AddressID", addressID);
            String sqlQuery3 = "UPDATE User_ SET firstName=@FirstName, lastName=@LastName, email=@Email, phone=@Phone, userType=@UserType, addressID=@AddressID WHERE userID=@UserID;";
            cmd3.CommandText = sqlQuery3;
            cmd3.Connection.Open();
            cmd3.ExecuteNonQuery();
            cmd3.Connection.Close();


            SqlCommand cmdLogin = new SqlCommand();
            cmdLogin.Connection = ValleyVisionConnection;
            cmdLogin.Connection.ConnectionString = AuthConnString;

            cmdLogin.Parameters.AddWithValue("@UserID", profiletoedit.UserID);
            cmdLogin.Parameters.AddWithValue("@Username", profiletoedit.UserName);
            string loginQuery = "UPDATE HashedCredentials SET UserID=@UserID, UserName=@Username WHERE UserID=@UserID;";
            cmdLogin.CommandText = loginQuery;
            cmdLogin.Connection.Open();
            cmdLogin.ExecuteNonQuery();
            DBClass.ValleyVisionConnection.Close();
        }


        //END MANAGE PROFILES PAGE____________________________________________________________________________________________



        //BEGIN NON PAGE SPECIFIC METHODS-------------------------------------------------------------------------------------
        //checks the userType value of a given user
        public static String CheckUserType(int? userID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", userID);
            cmd.CommandText = "SELECT U.UserType FROM User_ U WHERE U.userID = @UserID;";
            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            String userType=null;
            while (reader.Read())
            {
                userType = reader["UserType"].ToString();
            }

            ValleyVisionConnection.Close();

            return userType;

        }

        public static String? CheckUserName(int? userid)

        {

            SqlCommand cmd = new SqlCommand();

            cmd.Connection = ValleyVisionConnection;

            cmd.Connection.ConnectionString = AuthConnString;

            cmd.Parameters.AddWithValue("@UserID", userid);

            cmd.CommandText = "SELECT UserName FROM HashedCredentials WHERE UserID = @UserID;";

            cmd.Connection.Open(); // Open connection here, close in Model! 

            SqlDataReader UserReader = cmd.ExecuteReader();



            String? username = null;

            while (UserReader.Read())

            {

                username = UserReader["UserName"].ToString();

            }



            ValleyVisionConnection.Close();



            return username;

        }





        //END NON PAGE SPECIFIC METHODS---------------------------------------------------------------------------------------


        //BEGIN REVENUE SPECIFIC PAGE METHODS

        //reads data file 2 latest year revenue
        public static SqlDataReader LatestRevenueYearReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT * FROM DataFile_2 WHERE year_ = (SELECT MAX(year_) FROM DataFile_2);";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        //reads data file 2 from database
        public static SqlDataReader DataFileReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT * FROM DataFile_2;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        public static void AddRevenueData(Revenue newRevenueData)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@Year", newRevenueData.Year);
            cmd.Parameters.AddWithValue("@RealEstateTax", newRevenueData.RealEstateTax);
            cmd.Parameters.AddWithValue("@PersonalPropertyTax", newRevenueData.PersonalPropertyTax);
            cmd.Parameters.AddWithValue("@FeesLicensesTax", newRevenueData.FeesLicensesTax);
            cmd.Parameters.AddWithValue("@StateFunding", newRevenueData.StateFunding);
            decimal TotalRevenue = (newRevenueData.RealEstateTax + newRevenueData.PersonalPropertyTax + newRevenueData.FeesLicensesTax + newRevenueData.StateFunding);
            cmd.Parameters.AddWithValue("@TotalRevenue", TotalRevenue);
            String sqlQuery = "INSERT INTO DataFile_2 (year_, realEstateTax, personalPropertyTax,feesLicensesTax,otherRevenue,totalRevenue) VALUES (@Year, @RealEstateTax, @PersonalPropertyTax, @FeesLicensesTax, @StateFunding, @TotalRevenue);";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        //editing revenue data on revenue projection page
        public static void EditRevenueData(Revenue rev)
        {
            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@Year", rev.Year);
            cmd2.Parameters.AddWithValue("@RealEstateTax", rev.RealEstateTax);
            cmd2.Parameters.AddWithValue("@PersonalPropertyTax", rev.PersonalPropertyTax);
            cmd2.Parameters.AddWithValue("@FeesLicensesTax", rev.FeesLicensesTax);
            cmd2.Parameters.AddWithValue("@StateFunding", rev.StateFunding);
            decimal TotalRevenue = (rev.RealEstateTax + rev.PersonalPropertyTax + rev.FeesLicensesTax + rev.StateFunding);
            cmd2.Parameters.AddWithValue("@TotalRevenue", TotalRevenue);
            String sqlQuery2 = "UPDATE DataFile_2 SET realEstateTax=@RealEstateTax, personalPropertyTax=@PersonalPropertyTax, feesLicensesTax=@FeesLicensesTax, otherRevenue=@StateFunding, totalRevenue=@TotalRevenue WHERE year_=@Year;";
            cmd2.CommandText = sqlQuery2;
            cmd2.Connection.Open();
            cmd2.ExecuteNonQuery();
        }

        public static void DeleteRevenueData(int year)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@YeartoEdit", year);
            String sqlQuery = "DELETE FROM DataFile_2 WHERE year_ = @YeartoEdit;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static SqlDataReader SingleRevenueDataReader(int? year)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@Year", year);
            cmd.CommandText = "SELECT * FROM DataFile_2 WHERE DataFile_2.year_ = @Year;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }


        //END REVENUE SPECIFIC PAGE METHODS

        //BEGIN CREATE FULL PROFILE METHODS-------------------------------------------------------------------------------------
        public static void AddUser(FullProfile newfullProfile)
        {
            int addressID;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@Street", newfullProfile.Street);
            cmd.Parameters.AddWithValue("@Apartment", newfullProfile.Apartment);
            cmd.Parameters.AddWithValue("@City", newfullProfile.City);
            cmd.Parameters.AddWithValue("@State", newfullProfile.State);
            cmd.Parameters.AddWithValue("@Zip", newfullProfile.Zip);
            cmd.Parameters.AddWithValue("@Country", newfullProfile.Country);
            String sqlQuery = "INSERT INTO Address_ (street, apartment, city, state_, zip, country) VALUES (@Street, @Apartment, @City, @State, @Zip, @Country);" + "SELECT SCOPE_IDENTITY();";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            addressID = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Connection.Close();


            int userID;
            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@FirstName", newfullProfile.FirstName);
            cmd2.Parameters.AddWithValue("@LastName", newfullProfile.LastName);
            cmd2.Parameters.AddWithValue("@Email", newfullProfile.Email);
            cmd2.Parameters.AddWithValue("@Phone", newfullProfile.Phone);
            cmd2.Parameters.AddWithValue("@UserType", newfullProfile.UserType);
            cmd2.Parameters.AddWithValue("@Address", addressID);
            String sqlQuery2 = "INSERT INTO User_ (firstName, lastName, email, phone, userType, addressID) VALUES (@FirstName, @LastName, @Email, @Phone, 'User', @Address);" + "SELECT SCOPE_IDENTITY();";
            cmd2.CommandText = sqlQuery2;
            cmd2.Connection.Open();
            userID = Convert.ToInt32(cmd2.ExecuteScalar());
            cmd2.Connection.Close();


            
            SqlCommand cmdLogin = new SqlCommand();
            cmdLogin.Connection = ValleyVisionConnection;
            cmdLogin.Connection.ConnectionString = AuthConnString;
            cmdLogin.Parameters.AddWithValue("@UserID", userID);
            cmdLogin.Parameters.AddWithValue("@Username", newfullProfile.UserName);
            cmdLogin.Parameters.AddWithValue("@Password", PasswordHash.HashPassword(newfullProfile.Password));
            string loginQuery = "INSERT INTO HashedCredentials (UserID,UserName,Password_) VALUES (@UserID, @Username, @Password)";
            cmdLogin.CommandText = loginQuery;
            cmdLogin.Connection.Open();
            cmdLogin.ExecuteNonQuery();
            cmdLogin.Connection.Close();
        }
        //END CREATE FULL PROFILE METHODS-------------------------------------------------------------------------------------

        //BEGIN HISTORICAL SPENDING PAGE-------------------------------------------------------------------------------------
        //reads data file 3 from database
        public static SqlDataReader HistoricalExpendituresReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT * FROM DataFile_3;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }



        public static SqlDataReader singleExpenditureReader(int? yearid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@Year", yearid);
            cmd.CommandText = "SELECT * FROM DataFile_3 WHERE Year_ = @Year;";
            cmd.Connection.Open(); // Open connection here, close in Model! 
            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        public static void EditHistoricSpendingData(Expenditure expen, int year)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@YeartoEdit", year);
            cmd.Parameters.AddWithValue("@NewYear", expen.Year);
            cmd.Parameters.AddWithValue("@InflationRate", expen.InflationRate);
            cmd.Parameters.AddWithValue("@InterestRate", expen.InterestRate);
            cmd.Parameters.AddWithValue("@PublicSafety", expen.PublicSafety);
            cmd.Parameters.AddWithValue("@School", expen.School);
            cmd.Parameters.AddWithValue("@Anomaly", expen.Anomaly);
            cmd.Parameters.AddWithValue("@Other", expen.Other);
            decimal TotalExpenditure = (expen.Year + expen.InflationRate + expen.InterestRate + expen.PublicSafety + expen.School + expen.Anomaly + expen.Other);
            cmd.Parameters.AddWithValue("@TotalExpenditure", TotalExpenditure);
            String sqlQuery = "UPDATE DataFile_3 SET year_=@NewYear, inflationRate=@InflationRate, interestRate=@InterestRate, publicSafety=@PublicSafety, school=@School, anomaly=@Anomaly, other=@Other, totalExpenditure=@TotalExpenditure WHERE year_=@YeartoEdit;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
        }

        public static void DeleteHistoricalSpendData(int year)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@YeartoEdit", year);
            String sqlQuery = "DELETE FROM DataFile_3 WHERE year_ = @YeartoEdit;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static void AddHistoricalSpendingData(Expenditure newHistoricalSpendData)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@Year", newHistoricalSpendData.Year);
            cmd.Parameters.AddWithValue("@InflationRate", newHistoricalSpendData.InflationRate);
            cmd.Parameters.AddWithValue("@InterestRate", newHistoricalSpendData.InterestRate);
            cmd.Parameters.AddWithValue("@PublicSafety", newHistoricalSpendData.PublicSafety);
            cmd.Parameters.AddWithValue("@School", newHistoricalSpendData.School);
            cmd.Parameters.AddWithValue("@Anomaly", newHistoricalSpendData.Anomaly);
            cmd.Parameters.AddWithValue("@Other", newHistoricalSpendData.Other);
            decimal TotalExpenditure = (newHistoricalSpendData.Year + newHistoricalSpendData.InflationRate + newHistoricalSpendData.InterestRate + newHistoricalSpendData.PublicSafety + newHistoricalSpendData.School + newHistoricalSpendData.Anomaly + newHistoricalSpendData.Other);
            cmd.Parameters.AddWithValue("@TotalExpenditure", TotalExpenditure);
            String sqlQuery = "INSERT INTO DataFile_3 (year_, inflationRate, interestRate, publicSafety, school, anomaly, other, totalExpenditure) VALUES (@Year, @InflationRate, @InterestRate, @PublicSafety, @School, @Anomaly, @Other, @TotalExpenditure);";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }
        //END HISTORICAL SPENDING PAGE-------------------------------------------------------------------------------------

        //BEGIN SPENDING PROJECTION PAGE-------------------------------------------------------------------------------------
        //reads data file 2 latest year revenue
        public static SqlDataReader LatestHistoricalExpenditureReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT * FROM DataFile_3 WHERE year_ = (SELECT MAX(year_) FROM DataFile_3);";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }
        public static SqlDataReader InflationRatesReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT inflationRate FROM DataFile_4;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }


        //END SPENDING PROJECTION PAGE-------------------------------------------------------------------------------------

        //BEGIN PROPOSED DEVELOPMENT PAGE_________________________________________________________________________________________

        public static void AddDevelopmentArea(DevelopmentArea newDevArea, int? userID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@DevName", newDevArea.devName);
            cmd.Parameters.AddWithValue("@DevDescription", newDevArea.devDescription);
            cmd.Parameters.AddWithValue("@DevImpactLevel", newDevArea.devImpactLevel);
            cmd.Parameters.AddWithValue("@UploadedDateTime", newDevArea.uploadedDateTime);
            cmd.Parameters.AddWithValue("@UserID", userID);
            String sqlQuery = "INSERT INTO DevelopmentArea (devName, devDescription, devImpactLevel, uploadedDateTime, userID) VALUES (@DevName, @DevDescription, @DevImpactLevel, @UploadedDateTime, @UserID);";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public static void AddDevelopmentFiles(int devID, List<int> newDevelopmentFiles)
        {
            foreach (var fileMetaID in newDevelopmentFiles)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = ValleyVisionConnection;
                cmd.Connection.ConnectionString = MainConnString;
                cmd.Parameters.AddWithValue("@FileMetaID", fileMetaID);
                cmd.Parameters.AddWithValue("@DevID", devID);
                String sqlQuery = "INSERT INTO DevAreaFiles(fileMetaID, devID)VALUES (@FileMetaID, @DevID);";
                cmd.CommandText = sqlQuery;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
        }

        public static SqlDataReader DevelopmentReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT * FROM DevelopmentArea ORDER BY devImpactLevel DESC";
            cmd.Connection.Open(); // Open connection here, close in Model! 

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        public static SqlDataReader DevelopmentReader2(int devID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@DevID", devID);
            cmd.CommandText = "SELECT * FROM DevelopmentArea WHERE devID = @DevID";
            cmd.Connection.Open(); // Open connection here, close in Model! 

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        //delete development area 
        public static void DeleteDevelopmentArea(int devID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@DevID", devID);
            String sqlQuery = "DELETE FROM DevAreaFiles WHERE devID = @DevID;" + "DELETE FROM DevelopmentArea WHERE devID = @DevID;";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

        }

        //START DASHBOARD "DETAILS" PAGE
        public static SqlDataReader DevDetailsReader(int devID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@devID", devID);
            cmd.CommandText = "SELECT F.fileMetaID, F.fileName_, F.filePath, F.fileType, F.uploadedDateTime, U.firstName, U.lastName FROM DevelopmentArea D LEFT JOIN DevAreaFiles A ON A.devID = D.devID LEFT JOIN FileMeta F ON F.fileMetaID = A.fileMetaID JOIN User_ U ON U.userID = F.userID WHERE D.devID = @devID; ";
            cmd.Connection.Open(); // Open connection here, close in Model! 

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        public static SqlDataReader ProposedDevelopmentFileReader(int? devid, int? initid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@DevID", devid);
            cmd.Parameters.AddWithValue("@InitID", initid);
            cmd.CommandText = "SELECT DISTINCT F.fileMetaID, F.fileName_, F.filePath, F.fileType, F.uploadedDateTime, F.userID, F.published, U.firstName, U.lastName FROM FileMeta F JOIN InitiativeFiles IT ON F.fileMetaID = IT.fileMetaID JOIN User_ U ON F.userID = U.userID WHERE IT.initID = @InitID AND F.fileMetaID NOT IN (SELECT fileMetaID FROM DevAreaFiles WHERE devID = @DevID);";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }


        //END DASHBOARD "DETAILS" PAGE

        public static void UploadDevFile(int? initID, FileMeta fileMeta, int? devID)
        {
            int fileID;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@FileName", fileMeta.FileName_);
            cmd.Parameters.AddWithValue("@FilePath", fileMeta.FilePath);
            cmd.Parameters.AddWithValue("@FileType", fileMeta.FileType);
            cmd.Parameters.AddWithValue("@Date", fileMeta.UploadedDateTime);
            cmd.Parameters.AddWithValue("@UserID", fileMeta.userID);
            String sqlQuery = "INSERT INTO FileMeta (fileName_, filePath, fileType, uploadedDateTime, userID) VALUES (@FileName, @FilePath, @FileType, @Date, @UserID)" + "SELECT SCOPE_IDENTITY();";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            fileID = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Connection.Close();

            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = ValleyVisionConnection;
            cmd2.Connection.ConnectionString = MainConnString;
            cmd2.Parameters.AddWithValue("@initID", initID);
            cmd2.Parameters.AddWithValue("@fileID", fileID);
            String sqlQuery2 = "INSERT INTO InitiativeFiles (fileMetaID, initID) VALUES (@fileID, @initID)";
            cmd2.CommandText = sqlQuery2;
            cmd.Connection.Open();
            cmd2.ExecuteNonQuery();
            cmd2.Connection.Close();

            SqlCommand cmd3 = new SqlCommand();
            cmd3.Connection = ValleyVisionConnection;
            cmd3.Connection.ConnectionString = MainConnString;
            cmd3.Parameters.AddWithValue("@devID", devID);
            cmd3.Parameters.AddWithValue("@fileID", fileID);
            String sqlQuery3 = "INSERT INTO DevAreaFiles(fileMetaID, devID)VALUES (@fileID, @devID);";
            cmd3.CommandText = sqlQuery3;
            cmd3.Connection.Open();
            cmd3.ExecuteNonQuery();
            cmd3.Connection.Close();

        }




    }
}
