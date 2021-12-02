using ApproximateOptimization.Utils;
using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This is a convenience class for finding the range of the solution iteratively.
    /// It's much slower to use this class because it runs the actual solution finder multiple times.
    /// Also: it does not narrow the initial range, it can only widen it.
    /// </summary>
    public class AutoTuningFinder<T, P> : IOptimizer where T: OptimizerWithRangeDiscoveryParams<P> where P : BaseOptimizerParams
    {
        private IOptimizer solutionFinder;
        private T problemParameters;

        public AutoTuningFinder(T problemParameters)
        {
            this.problemParameters = problemParameters;
            solutionFinder = problemParameters.optimizerFactoryMethod(problemParameters.solutionRange);
        }

        public double[] BestSolutionSoFar => solutionFinder.BestSolutionSoFar;

        public double SolutionValue => solutionFinder.SolutionValue;

        public bool SolutionFound => solutionFinder.SolutionFound;

        public void FindMaximum()
        {
            var solutionRange = problemParameters.solutionRange ?? ParametersManagement.GetDefaultSolutionRange(problemParameters.dimension);
            bool requiresRecalculation = false;
            var attempts = problemParameters.maxAttempts;
            do
            {
                solutionFinder = problemParameters.optimizerFactoryMethod(solutionRange);
                solutionFinder.FindMaximum();
                if (solutionFinder.SolutionFound)
                {
                    requiresRecalculation = false;
                    for (var i = 0; i < problemParameters.dimension; i++)
                    {
                        var rangeWidth = solutionRange[i][1] - solutionRange[i][0];
                        if (solutionFinder.BestSolutionSoFar[i] - solutionRange[i][0] < 0.01 * rangeWidth)
                        {
                            solutionRange[i][0] = solutionRange[i][0] - rangeWidth * 1.5;
                            solutionRange[i][1] = solutionRange[i][1] - rangeWidth * 0.5;
                            requiresRecalculation = true;
                        } else if (solutionRange[i][1] - solutionFinder.BestSolutionSoFar[i] < 0.01 * rangeWidth)
                        {
                            solutionRange[i][0] = solutionRange[i][0] + rangeWidth * 0.5;
                            solutionRange[i][1] = solutionRange[i][1] + rangeWidth * 1.5;
                            requiresRecalculation = true;
                        }
                    }
                } else
                {
                    break;
                }
            } while (attempts-- > 0 && requiresRecalculation);
        }
    }
}