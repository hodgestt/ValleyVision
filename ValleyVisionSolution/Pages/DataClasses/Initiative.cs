using System.ComponentModel.DataAnnotations;

namespace ValleyVisionSolution.Pages.DataClasses
{
    public class Initiative
    {
        public int? InitID { get; set; }
        [Required]
        public string? InitName { get; set; }
        public string? FilePath { get; set; }
        public DateTime? InitDateTime { get; set; }
    }
}
