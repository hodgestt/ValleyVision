namespace ValleyVisionSolution.Pages.DataClasses
{
    public class Revenue
    {
        public String Year { get; set; }
        public decimal RealEstateTax { get; set; }
        public decimal PersonalPropertyTax { get; set; }
        public decimal FeesLicensesTax { get; set; }
        public decimal StateFunding { get; set; }
        public decimal TotalRevenue { get; set; }

    }
}
