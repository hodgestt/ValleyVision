namespace ValleyVisionSolution.Pages.DataClasses
{
    public class ExpenditureProjection
    {
        public int Year { get; set; }
        public decimal InflationRate { get; set; }
        public decimal PublicSafetyUCL { get; set; }
        public decimal PublicSafety { get; set; }
        public decimal PublicSafetyLCL { get; set; }
        public decimal SchoolUCL { get; set; }
        public decimal School { get; set; }
        public decimal SchoolLCL { get; set; }
        public decimal AnomalyUCL { get; set; }
        public decimal Anomaly { get; set; }
        public decimal AnomalyLCL { get; set; }
        public decimal OtherUCL { get; set; }
        public decimal Other { get; set; }
        public decimal OtherLCL { get; set; }
        public decimal TotalExpenditureUCL { get; set; }
        public decimal TotalExpenditure { get; set; }
        public decimal TotalExpenditureLCL { get; set; }
    }
}
