using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System.Text.Json;

namespace ValleyVisionSolution.Pages.SpendingProjection
{
    public class SpendingProjectionPageModel : PageModel
    {
        public List<Expenditure> HistoricalExpenditures { get; set; }
        public Expenditure LatestHistoricalExpenditure { get; set; }
        public List<Expenditure> ProjectedExpenditures { get; set; }

        [BindProperty]
        public int NumProjectionYears { get; set; }
        [BindProperty]
        public decimal ParameterInflationRate { get; set; }
        [BindProperty]
        public decimal ParameterInterestRate { get; set; }
        [BindProperty]
        public decimal ParameterAnomaly { get; set; }

        public SpendingProjectionPageModel() 
        { 
            HistoricalExpenditures = new List<Expenditure>();
            LatestHistoricalExpenditure = new Expenditure();
            ProjectedExpenditures = new List<Expenditure> ();
        }    

        public void loadData()
        {
            //Populate current year revenue data
            SqlDataReader reader = DBClass.HistoricalExpendituresReader();
            while (reader.Read())
            {
                HistoricalExpenditures.Add(new Expenditure
                {
                    Year = int.Parse(reader["Year_"].ToString()),
                    InflationRate = Decimal.Parse(reader["InflationRate"].ToString()),
                    InterestRate = Decimal.Parse(reader["InterestRate"].ToString()),
                    PublicSafety = Decimal.Parse(reader["PublicSafety"].ToString()),
                    School = Decimal.Parse(reader["School"].ToString()),
                    Anomaly = Decimal.Parse(reader["Anomaly"].ToString()),
                    Other = Decimal.Parse(reader["Other"].ToString()),
                    TotalExpenditure = Decimal.Parse(reader["TotalExpenditure"].ToString())
                });
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();

            //Populate current year revenue data
            SqlDataReader reader2 = DBClass.LatestHistoricalExpenditureReader();
            while (reader2.Read())
            {
                LatestHistoricalExpenditure = new Expenditure
                {
                    Year = int.Parse(reader2["Year_"].ToString()),
                    InflationRate = Decimal.Parse(reader2["InflationRate"].ToString()),
                    InterestRate = Decimal.Parse(reader2["InterestRate"].ToString()),
                    PublicSafety = Decimal.Parse(reader2["PublicSafety"].ToString()),
                    School = Decimal.Parse(reader2["School"].ToString()),
                    Anomaly = Decimal.Parse(reader2["Anomaly"].ToString()),
                    Other = Decimal.Parse(reader2["Other"].ToString()),
                    TotalExpenditure = Decimal.Parse(reader2["TotalExpenditure"].ToString())
                };
            }
            // Close your connection in DBClass
            DBClass.ValleyVisionConnection.Close();
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("LoggedIn") == "True")
            {
                loadData();
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public IActionResult OnPostProjectExpenditures()
        {
            loadData();
            ProjectedExpenditures.Add(LatestHistoricalExpenditure);

            // Prepare data for regression
            var inflationRates = HistoricalExpenditures.Select(e => (double)e.InflationRate).ToArray();
            var interestRates = HistoricalExpenditures.Select(e => (double)e.InterestRate).ToArray();

            // Independent variables matrix
            var indepVariables = inflationRates.Zip(interestRates, (inflation, interest) => new[] { inflation, interest }).ToArray();

            // Perform regression for each expenditure category
            var publicSafetyExp = HistoricalExpenditures.Select(e => (double)e.PublicSafety).ToArray();
            var schoolExp = HistoricalExpenditures.Select(e => (double)e.School).ToArray();
            var otherExp = HistoricalExpenditures.Select(e => (double)e.Other).ToArray();

            var publicSafetyModel = MultipleRegression.NormalEquations(indepVariables, publicSafetyExp);
            var schoolModel = MultipleRegression.NormalEquations(indepVariables, schoolExp);
            var otherModel = MultipleRegression.NormalEquations(indepVariables, otherExp);

            ProjectedExpenditures.Add(LatestHistoricalExpenditure);


            // Predict future expenditures for each category
            int lastYear = LatestHistoricalExpenditure.Year;
            for (int i = 1; i <= NumProjectionYears; i++)
            {
                int futureYear = lastYear + i;


                double[] futureIndepVars = { (double)ParameterInflationRate/100, (double)ParameterInterestRate/100 };

                double projectedPublicSafety = publicSafetyModel[0] + futureIndepVars.Zip(publicSafetyModel.Skip(1), (x, m) => x * m).Sum();
                double projectedSchool = schoolModel[0] + futureIndepVars.Zip(schoolModel.Skip(1), (x, m) => x * m).Sum();
                double projectedOther = otherModel[0] + futureIndepVars.Zip(otherModel.Skip(1), (x, m) => x * m).Sum();

                ProjectedExpenditures.Add(new Expenditure
                {
                    Year = futureYear,
                    InflationRate = ParameterInflationRate,
                    InterestRate = ParameterInterestRate,
                    PublicSafety = (decimal)projectedPublicSafety,
                    School = (decimal)projectedSchool,
                    Anomaly = ParameterAnomaly,
                    Other = (decimal)projectedOther,
                    // Calculate TotalExpenditure based on the sum of the individual projected expenditures
                    TotalExpenditure = (decimal)(projectedPublicSafety + projectedSchool + projectedOther) + ParameterAnomaly
                }); 
            }

            return Page();
        }

        public string GetChartDataAsJson()
        {
            return JsonSerializer.Serialize(ProjectedExpenditures);
        }


        public IActionResult OnPostLogoutHandler()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}
