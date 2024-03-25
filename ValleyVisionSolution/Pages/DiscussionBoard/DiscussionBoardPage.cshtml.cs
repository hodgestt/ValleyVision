using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static BenchmarkDotNet.Engines.EngineEventSource;
using ValleyVisionSolution.Pages.DataClasses;
using System.Data.SqlClient;
using ValleyVisionSolution.Pages.DB;

namespace ValleyVisionSolution.Pages.DiscussionBoard
{
    public class DiscussionBoardPageModel : PageModel
    {
        public List<Message> Messages { get; set; }

        public int initID { get; set; }

        public int? InitiativeArea { get; set; }

        public int? LoggedInUser { get; set; }

        [BindProperty]
        public Message NewMessage { get; set; }
        
        public DiscussionBoardPageModel()
        {
            Messages = new List<Message>();
            NewMessage = new Message();
            NewMessage.MessageContent = "";
        }



            public void OnGet()
        {
            LoggedInUser = HttpContext.Session.GetInt32("UserID");
            InitiativeArea = HttpContext.Session.GetInt32("InitID");
            initID = (int)InitiativeArea;

            SqlDataReader reader5 = DBClass.MessagesReader(initID);
            while (reader5.Read())
            {
                Messages.Add(new Message
                {
                    MessageID = int.Parse(reader5["messageID"].ToString()),
                    MessageContent = reader5["messageContent"].ToString(),
                    MessageDateTime = Convert.ToDateTime(reader5["messageDateTime"]),
                    UserID = int.Parse(reader5["userID"].ToString()),
                    InitID = int.Parse(reader5["initID"].ToString()),
                    FirstName = reader5["firstName"].ToString(),
                    LastName = reader5["lastName"].ToString()
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
        }
       
        public IActionResult OnPostSendMessage()
        {
            if (NewMessage.MessageContent != null)
            {
                NewMessage.UserID = HttpContext.Session.GetInt32("UserID");
                NewMessage.InitID = HttpContext.Session.GetInt32("InitID");
                NewMessage.MessageDateTime = DateTime.Now;
                DBClass.AddMessage(NewMessage);
                
            }
            
            ModelState.Clear();
            Messages = new List<Message>();
            NewMessage = new Message();

            LoggedInUser = HttpContext.Session.GetInt32("UserID");
            InitiativeArea = HttpContext.Session.GetInt32("InitID");
            initID = (int)InitiativeArea;

            SqlDataReader reader5 = DBClass.MessagesReader(initID);
            while (reader5.Read())
            {
                Messages.Add(new Message
                {
                    MessageID = int.Parse(reader5["messageID"].ToString()),
                    MessageContent = reader5["messageContent"].ToString(),
                    MessageDateTime = Convert.ToDateTime(reader5["messageDateTime"]),
                    UserID = int.Parse(reader5["userID"].ToString()),
                    InitID = int.Parse(reader5["initID"].ToString()),
                    FirstName = reader5["firstName"].ToString(),
                    LastName = reader5["lastName"].ToString()
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

            return Page();
        }

        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
