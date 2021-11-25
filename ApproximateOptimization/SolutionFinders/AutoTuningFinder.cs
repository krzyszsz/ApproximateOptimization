using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This is a convenience class for finding the range of the solution iteratively.
    /// It's much slower to use this class because it runs the actual solution finder multiple times.
    /// Also: it does not narrow the initial range, it can only widen it.
    /// </summary>
    public class AutoTuningFinder : ISolutionFinder
    {
        private readonly Func<ISolutionFinder> solutionFinderFactoryMethod;
        private int attempts;
        private ISolutionFinder solutionFinder;

        public AutoTuningFinder(Func<ISolutionFinder> solutionFinderFactoryMethod, int maxAttempts=50)
        {
            this.solutionFinderFactoryMethod = solutionFinderFactoryMethod;
            this.attempts = maxAttempts;
        }

        public double[] BestSolutionSoFar => solutionFinder.BestSolutionSoFar;

        public double SolutionValue => solutionFinder.SolutionValue;

        public bool SolutionFound => solutionFinder.SolutionFound;

        public void FindMaximum(int dimension, Func<double[], double> getValue, TimeSpan timeLimit = default, long maxIterations = -1, double[][] solutionRange = null)
        {
            solutionRange = solutionRange ?? BaseSolutionFinder.GetDefaultSolutionRange(dimension);
            bool requiresRecalculation = false;
            do
            {
                solutionFinder = solutionFinderFactoryMethod();
                solutionFinder.FindMaximum(dimension, getValue, timeLimit, maxIterations, solutionRange);
                if (solutionFinder.SolutionFound)
                {
                    requiresRecalculation = false;
                    for (var i = 0; i < dimension; i++)
                    {
                        var rangeWidth = solutionRange[i][1] - solutionRange[i][0];
                        if (solutionFinder.BestSolutionSoFar[i] - solutionRange[i][0] < 0.01 * rangeWidth)
                        {
                            solutionRange[i][0] -= rangeWidth * 10;
                            solutionRange[i][1] -= rangeWidth * 8;
                            requiresRecalculation = true;
                        }

                        if (solutionRange[i][1] - solutionFinder.BestSolutionSoFar[i] < 0.01 * rangeWidth)
                        {
                            solutionRange[i][0] += rangeWidth * 8;
                            solutionRange[i][1] += rangeWidth * 10;
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
