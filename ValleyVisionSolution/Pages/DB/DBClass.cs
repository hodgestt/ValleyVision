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

        public static SqlDataReader InitiativesReader(int userID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = ValleyVisionConnection;
            cmd.Connection.ConnectionString = MainConnString;
            cmd.Parameters.AddWithValue("@UserID", userID);
            cmd.CommandText = "SELECT DISTINCT I.initID, I.initName, I.initDateTime FROM Initiative I JOIN InitiativeUsers IU ON IU.initID = I.initID WHERE IU.userID = @UserID;";
            cmd.Connection.Open(); // Open connection here, close in Model!

            SqlDataReader tempReader = cmd.ExecuteReader();

            return tempReader;
        }
        //END INITIATIVES PAGE____________________________________________________________________________________________



        //BEGIN DASHBOARD PAGE___________________________________________________________________________________________
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














    }
}
