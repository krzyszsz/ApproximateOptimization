﻿using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This is a convenience class for finding the range of the solution iteratively.
    /// It's much slower to use this class because it runs the actual solution finder multiple times.
    /// Also: it does not narrow the initial range, it can only widen it.
    /// </summary>
    public class AutoTuningFinder<T, P> : BaseSolutionFinder<T> where T: AutoTuningParams<P> where P : BaseSolutionFinderParams
    {
        private ISolutionFinder<P> solutionFinder;

        public override void Initialize(T solutionFinderParams)
        {
            base.Initialize(solutionFinderParams);
            solutionFinder = problemParameters.solutionFinderFactoryMethod();
        }

        public new double[] BestSolutionSoFar => solutionFinder.BestSolutionSoFar;

        public new double SolutionValue => solutionFinder.SolutionValue;

        public new bool SolutionFound => solutionFinder.SolutionFound;

        public new void FindMaximum()
        {
            solutionRange = solutionRange ?? BaseSolutionFinder<P>.GetDefaultSolutionRange(problemParameters.dimension);
            bool requiresRecalculation = false;
            var attempts = problemParameters.maxAttempts;
            do
            {
                solutionFinder = problemParameters.solutionFinderFactoryMethod();
                solutionFinder.FindMaximum();
                if (solutionFinder.SolutionFound)
                {
                    requiresRecalculation = false;
                    for (var i = 0; i < problemParameters.dimension; i++)
                    {
                        var rangeWidth = solutionRange[i][1] - solutionRange[i][0];
                        if (solutionFinder.BestSolutionSoFar[i] - solutionRange[i][0] < 0.01 * rangeWidth)
                        {
                            solutionRange[i][1] = solutionRange[i][0] + rangeWidth * 0.5;
                            solutionRange[i][0] -= rangeWidth * 10;
                            requiresRecalculation = true;
                        } else if (solutionRange[i][1] - solutionFinder.BestSolutionSoFar[i] < 0.01 * rangeWidth)
                        {
                            solutionRange[i][0] = solutionRange[i][1] - rangeWidth * 0.5;
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

        protected override void NextSolution()
        {
            throw new NotImplementedException();
        }
    }
}
