using System;

namespace ApproximateOptimization
{
    public class ExternallyInjectedOptimizerState
    {
        public double[] CurrentSolution { get; set; }
        public double[] BestSolutionSoFar { get; set; }
        public int Dimension { get; set; }
        public double SolutionValue { get; set; }
        public Func<double[], double> ScoreFunction { get; set; }
        public double[][] SolutionRange { get; set; }
        public Action RequestNextSolution { get; set; }
    }
}
