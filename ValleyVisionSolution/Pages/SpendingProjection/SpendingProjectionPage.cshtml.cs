using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;

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
            ParameterInflationRate = ParameterInflationRate / 100;
            ParameterInterestRate = ParameterInterestRate / 100;

            int nextYear = LatestHistoricalExpenditure.Year + 1;

            for (int i = 0; i < NumProjectionYears; i++)
            {
                // Extracting data from the list into arrays
                double[] inflationRate = HistoricalExpenditures.Select(e => (double)e.InflationRate).ToArray();
                double[] interestRate = HistoricalExpenditures.Select(e => (double)e.InterestRate).ToArray();
                double[] publicSafety = HistoricalExpenditures.Select(e => (double)e.PublicSafety).ToArray();
                double[] school = HistoricalExpenditures.Select(e => (double)e.School).ToArray();
                double[] other = HistoricalExpenditures.Select(e => (double)e.Other).ToArray();


                // Create the design matrix with an intercept column
                var designMatrixPublicSafety = Matrix<double>.Build.DenseOfColumnArrays(
                    new double[publicSafety.Length].Populate(1), // Intercept column of 1's
                    inflationRate,
                    interestRate
                );

                var designMatrixSchool = Matrix<double>.Build.DenseOfColumnArrays(
                    new double[school.Length].Populate(1), // Intercept column of 1's
                    inflationRate,
                    interestRate
                );

                var designMatrixOther = Matrix<double>.Build.DenseOfColumnArrays(
                    new double[other.Length].Populate(1), // Intercept column of 1's
                    inflationRate,
                    interestRate
                );

                // Create the vector for the dependent variable
                var publicSafetyVector = Vector<double>.Build.Dense(publicSafety);
                var schoolVector = Vector<double>.Build.Dense(school);
                var otherVector = Vector<double>.Build.Dense(other);


                // Perform the multiple regression
                var publicSafetycoefficients = MultipleRegression.NormalEquations(designMatrixPublicSafety, publicSafetyVector);
                var schoolcoefficients = MultipleRegression.NormalEquations(designMatrixSchool, schoolVector);
                var othercoefficients = MultipleRegression.NormalEquations(designMatrixOther, otherVector);

                

                ProjectedExpenditures.Add(new Expenditure
                {
                    Year = nextYear,
                    InflationRate = ParameterInflationRate,
                    InterestRate = ParameterInterestRate,
                    PublicSafety = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate)),
                    School = (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate)),
                    Anomaly = ParameterAnomaly,
                    Other = (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate)),
                    TotalExpenditure = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate))
                                       + (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate))
                                       + ParameterAnomaly
                                       + (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate))
                });

                HistoricalExpenditures.Add(new Expenditure
                {
                    Year = nextYear,
                    InflationRate = ParameterInflationRate,
                    InterestRate = ParameterInterestRate,
                    PublicSafety = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate)),
                    School = (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate)),
                    Anomaly = ParameterAnomaly,
                    Other = (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate)),
                    TotalExpenditure = (decimal)(publicSafetycoefficients[0] + (publicSafetycoefficients[1] * (double)ParameterInflationRate) + (publicSafetycoefficients[2] * (double)ParameterInterestRate))
                                       + (decimal)(schoolcoefficients[0] + (schoolcoefficients[1] * (double)ParameterInflationRate) + (schoolcoefficients[2] * (double)ParameterInterestRate))
                                       + ParameterAnomaly
                                       + (decimal)(othercoefficients[0] + (othercoefficients[1] * (double)ParameterInflationRate) + (othercoefficients[2] * (double)ParameterInterestRate))
                });

                nextYear++;
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
