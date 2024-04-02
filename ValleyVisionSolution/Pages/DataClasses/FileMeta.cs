using System.ComponentModel.DataAnnotations;

namespace ValleyVisionSolution.Pages.DataClasses
{
    public class FileMeta
    {
        public int? FileMetaID { get; set; }
        public string? FileName_ { get; set; }
        public string? FilePath { get; set; }
        public string? FileType{ get; set; }
        public DateTime UploadedDateTime { get; set; }
        public int? userID { get; set; }
        public string? published { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string InitiativeName { get; set; }

    }
}
