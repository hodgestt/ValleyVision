using System.ComponentModel.DataAnnotations;

namespace ValleyVisionSolution.Pages.DataClasses
{
    public class Task
    {
        public int? TaskID { get; set; }
        [Required(ErrorMessage = "The task name is required")]
        public string? TaskName { get; set; }
        [Required]
        public string? TaskStatus { get; set; }
        [Required(ErrorMessage = "The description is required")]
        public string? TaskDescription { get; set; }
        [Required(ErrorMessage = "The task due date is required")]
        public DateTime TaskDueDateTime { get; set; }
        public int? InitID { get; set; }
    }
}
