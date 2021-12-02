using System;

namespace ApproximateOptimization
{
    public class ExternallyInjectedOptimizerState
    {
        public double[] CurrentSolution { get; set; }
        public double[] BestSolutionSoFar { get; set; }
        public double[] CurrentSolutionAtStart { get; set; }
        public double SolutionValue { get; set; }
        public Func<double> RequestNextSolution { get; set; }
    }
}
