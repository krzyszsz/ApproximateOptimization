using System;

namespace ApproximateOptimization
{
    public interface IControllableLocalAreaSolutionFinder
    {
        void NextSolution();
        double LocalArea { get; set; }
        double[] CurrentSolution { get; set; }
        double[] BestSolutionSoFar { get; set; }
        int Dimension { get; set; }
        Func<double[], double> ScoreFunction { get; set; }
        double SolutionValue { get; set; }
    }
}
