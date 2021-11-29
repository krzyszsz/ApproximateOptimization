using System;

namespace ApproximateOptimization
{
    public class ExternallyInjectedOptimizerState
    {
        public double[] CurrentSolution { get; set; }
        public double[] BestSolutionSoFar { get; set; }
        public double SolutionValue { get; set; }
        public Action RequestNextSolution { get; set; }
    }
}
