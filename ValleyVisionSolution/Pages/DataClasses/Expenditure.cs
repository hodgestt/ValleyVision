namespace ValleyVisionSolution.Pages.DataClasses
{
    public class Expenditure
    {
        public int Year { get; set; }
        public decimal InflationRate { get; set; }
        public decimal InterestRate { get; set; }
        public decimal PublicSafety { get; set; }
        public decimal School { get; set; }
        public decimal Anomaly { get; set; }
        public decimal Other { get; set; }
        public decimal TotalExpenditure { get; set; }

    }
}
