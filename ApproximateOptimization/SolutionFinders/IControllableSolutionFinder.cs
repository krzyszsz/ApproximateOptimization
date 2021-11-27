using System;

namespace ApproximateOptimization
{
    public interface IControllableSolutionFinder
    {
        void OnInitialized();
        void NextSolution();
        double[] CurrentSolution { get; set; }
        double[] BestSolutionSoFar { get; set; }
        int Dimension { get; set; }
        Func<double[], double> ScoreFunction { get; set; }
        double SolutionValue { get; set; }
        double[][] SolutionRange { get; set; }
    }
}
