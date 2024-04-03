using Microsoft.Diagnostics.Utilities;

namespace ValleyVisionSolution.Pages.DataClasses
{
    public class DevelopmentArea
    {
        public int devID { get; set; }
        public string devName { get; set; }
        public string devDescription { get; set; }
        public string devImpactLevel { get; set; }
        public DateTime uploadedDateTime { get; set; }
    }
}
