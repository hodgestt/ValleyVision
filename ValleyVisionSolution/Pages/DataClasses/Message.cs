namespace ValleyVisionSolution.Pages.DataClasses
{
    public class Message
    {
        public int? MessageID { get; set; }

        public string? MessageContent { get; set; }
        public DateTime? MessageDateTime { get; set; }
        public int? UserID { get; set; }
        public int? InitID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
