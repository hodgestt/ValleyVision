using System.ComponentModel.DataAnnotations;

namespace ValleyVisionSolution.Pages.DataClasses
{
    public class Task
    {
        public int? TaskID { get; set; }
        [Required]
        public string? TaskName { get; set; }
        [Required]
        public string? TaskStatus { get; set; }
        [Required]
        public string? TaskDescription { get; set; }
        [Required]
        public DateTime TaskDueDateTime { get; set; }
        public int? InitID { get; set; }
    }
}
