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
        private IOptimizer _solutionFinder;
        private T _problemParameters;

        public AutoTuningFinder(T problemParameters)
        {
            this._problemParameters = problemParameters;
            _solutionFinder = problemParameters.OptimizerFactoryMethod(problemParameters.SolutionRange);
        }

        public double[] BestSolutionSoFar => _solutionFinder.BestSolutionSoFar;

        public double SolutionValue => _solutionFinder.SolutionValue;

        public bool SolutionFound => _solutionFinder.SolutionFound;

        public void FindMaximum()
        {
            var solutionRange = _problemParameters.SolutionRange ?? ParametersManagement.GetDefaultSolutionRange(_problemParameters.Dimension);
            bool requiresRecalculation = false;
            var attempts = _problemParameters.MaxAttempts;
            do
            {
                _solutionFinder = _problemParameters.OptimizerFactoryMethod(solutionRange);
                _solutionFinder.FindMaximum();
                if (_solutionFinder.SolutionFound)
                {
                    requiresRecalculation = false;
                    for (var i = 0; i < _problemParameters.Dimension; i++)
                    {
                        var rangeWidth = solutionRange[i][1] - solutionRange[i][0];
                        if (_solutionFinder.BestSolutionSoFar[i] - solutionRange[i][0] < 0.01 * rangeWidth)
                        {
                            solutionRange[i][0] = solutionRange[i][0] - rangeWidth * 1.5;
                            solutionRange[i][1] = solutionRange[i][1] - rangeWidth * 0.5;
                            requiresRecalculation = true;
                        } else if (solutionRange[i][1] - _solutionFinder.BestSolutionSoFar[i] < 0.01 * rangeWidth)
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