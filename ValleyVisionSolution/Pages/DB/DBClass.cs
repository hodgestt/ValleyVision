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
namespace ValleyVisionSolution.Pages.DB
{
    public class DBClass
    {
        // Connection Object at Data Field Level
        public static SqlConnection ValleyVisionConnection = new SqlConnection();

        // Connection String - How to find and connect to DB
        private static readonly String? MainConnString = "Server=Localhost;Database=Main;Trusted_Connection=True";
        private static readonly String? AuthConnString = "Server=Localhost;Database=AUTH;Trusted_Connection=True";




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



        public static List<Revenue> GetChartDataFromDatabase()
        {
            List<Revenue> dataList = new List<Revenue>();

            using (SqlConnection connection = new SqlConnection(MainConnString))
            {
                string sqlQuery = "SELECT year_, realEstateTax, personalPropertyTax, feesLicensesTax, stateFunding, totalRevenue FROM DataFile_2;";
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
                    dataItem.StateFunding = Convert.ToDecimal(reader["stateFunding"]);
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
            String sqlQuery = "INSERT INTO Initiative (InitName, InitDateTime) VALUES (@InitName, @InitDateTime);" + "SELECT SCOPE_IDENTITY();";
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
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString= MainConnString;
            cmd.CommandText = "SELECT * FROM FileMeta WHERE published = 'yes'";
            cmd.Connection.Open ();
            SqlDataReader tempReader = cmd.ExecuteReader();
            return tempReader;
        }

        public static void PublishFile(int fileID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@fileID", fileID);
            String sqlQuery = "UPDATE FileMeta SET published = 'yes' WHERE FileMetaID = @fileID";
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
        public static SqlDataReader FullProfilesReader()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.CommandText = "SELECT C.UserName, C.Password_, U.UserID, U.firstName, U.lastName, U.email, U.phone, U.userType, A.street, A.apartment, A.city, A.state_, A.zip, A.country FROM AUTH.dbo.HashedCredentials C JOIN Main.dbo.User_ U ON U.UserID = C.UserID JOIN Main.dbo.Address_ A ON A.AddressID = U.AddressID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }

        public static SqlDataReader SingleProfilesReader(int? userid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", userid);
            cmd.CommandText = "SELECT C.UserName, C.Password_, U.userID, U.firstName, U.lastName, U.email, U.phone, U.userType, A.street, A.apartment, A.city, A.state_, A.zip, A.country FROM AUTH.dbo.HashedCredentials C JOIN Main.dbo.User_ U ON U.userID = C.UserID JOIN Main.dbo.Address_ A ON A.AddressID = U.AddressID WHERE U.userID=@UserID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
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

            //SqlCommand cmd3 = new SqlCommand();
            //cmd3.Connection = ValleyVisionConnection;
            //cmd3.Connection.ConnectionString = MainConnString;
            //cmd3.Parameters.AddWithValue("@UserID", userid);
            //String sqlQuery3 = "DELETE FROM Message_ WHERE userID = @UserID;";
            //cmd3.CommandText = sqlQuery3;
            //cmd3.Connection.Open();
            //cmd3.ExecuteNonQuery();
            //cmd3.Connection.Close();

            //SqlCommand cmd4 = new SqlCommand();
            //cmd4.Connection = ValleyVisionConnection;
            //cmd4.Connection.ConnectionString = MainConnString;
            //cmd4.Parameters.AddWithValue("@UserID", userid);
            //String sqlQuery4 = "DELETE FROM InitiativeUsers WHERE userID = @UserID;";
            //cmd4.CommandText = sqlQuery4;
            //cmd4.Connection.Open();
            //cmd4.ExecuteNonQuery();
            //cmd4.Connection.Close();

            //SqlCommand cmd5 = new SqlCommand();
            //cmd5.Connection = ValleyVisionConnection;
            //cmd5.Connection.ConnectionString = MainConnString;
            //cmd5.Parameters.AddWithValue("@UserID", userid);
            //String sqlQuery5 = "DELETE FROM TaskUsers WHERE userID = @UserID;";
            //cmd5.CommandText = sqlQuery5;
            //cmd5.Connection.Open();
            //cmd5.ExecuteNonQuery();
            //cmd5.Connection.Close();

            //SqlCommand cmd6 = new SqlCommand();
            //cmd6.Connection = ValleyVisionConnection;
            //cmd6.Connection.ConnectionString = MainConnString;
            //cmd5.Parameters.AddWithValue("@UserID", userid);
            //String sqlQuery6 = "SELECT fileMetaID FROM FileMeta WHERE userID = @UserID;";
            //cmd6.CommandText = sqlQuery6;
            //cmd6.Connection.Open();
            //SqlDataReader tempReader2 = cmd.ExecuteReader();
            //cmd6.Connection.Close();

            //int filemeta;
            //while (tempReader2.Read())
            //{
            //    filemeta = Int32.Parse(tempReader2["fileMetaID"].ToString());
            //}

            //SqlCommand cmd7 = new SqlCommand();
            //cmd7.Connection = ValleyVisionConnection;
            //cmd7.Connection.ConnectionString = MainConnString;
            //String sqlQuery7 = "DELETE FROM Initiatives WHERE fileMetaID = " + @filemeta + ";";
            //cmd7.CommandText = sqlQuery6;
            //cmd7.Connection.Open();
            //cmd7.ExecuteNonQuery();
            //cmd7.Connection.Close();

            //SqlCommand cmd8 = new SqlCommand();
            //cmd8.Connection = ValleyVisionConnection;
            //cmd8.Connection.ConnectionString = MainConnString;
            //cmd8.Parameters.AddWithValue("@UserID", userid);
            //String sqlQuery8 = "DELETE FROM FileMeta WHERE userID = @UserID;";
            //cmd8.CommandText = sqlQuery8;
            //cmd8.Connection.Open();
            //cmd8.ExecuteNonQuery();
            //cmd8.Connection.Close();

            //SqlCommand cmd9 = new SqlCommand();
            //cmd9.Connection = ValleyVisionConnection;
            //cmd9.Connection.ConnectionString = MainConnString;
            //cmd9.Parameters.AddWithValue("@UserID", userid);
            //String sqlQuery9 = "DELETE FROM User_ WHERE userID = @UserID;";
            //cmd9.CommandText = sqlQuery9;
            //cmd9.Connection.Open();
            //cmd9.ExecuteNonQuery();
            //cmd9.Connection.Close();
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
            cmd.CommandText = "SELECT U.UserType FROM Main.dbo.User_ U JOIN AUTH.dbo.HashedCredentials C ON C.UserID=U.userID WHERE U.userID = @UserID;";
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

            cmd.Connection.ConnectionString = MainConnString;

            cmd.Parameters.AddWithValue("@UserID", userid);

            cmd.CommandText = "SELECT C.UserName FROM AUTH.dbo.HashedCredentials C JOIN Main.dbo.User_ U ON U.userID = C.UserID WHERE U.userID=@UserID;";

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
            String sqlQuery = "INSERT INTO DataFile_2 (year_, realEstateTax, personalPropertyTax,feesLicensesTax,stateFunding,totalRevenue) VALUES (@Year, @RealEstateTax, @PersonalPropertyTax, @FeesLicensesTax, @StateFunding, @TotalRevenue);";
            cmd.CommandText = sqlQuery;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
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
        //END SPENDING PROJECTION PAGE-------------------------------------------------------------------------------------

        //BEGIN PROPOSED DEVELOPMENT PAGE_________________________________________________________________________________________
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





    }
}
