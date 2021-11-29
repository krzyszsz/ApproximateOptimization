using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This combines multiple solution finders to improve results for some problems:
    /// After each iteration of simulated annealing it runs LocalAreaBinarySearch / GradientAscentOptimizer
    /// with narrowing down local area.
    /// </summary>
    public class SimulatedAnnealingWithLocalAreaBinarySearch<T> : SimulatedAnnealing<T> where T: SimulatedAnnealingWithLocalAreaBinarySearchParams
    {
        private LocalAreaBinarySearchParams localAreaBinarySearchParams;
        private LocalAreaBinarySearch<LocalAreaBinarySearchParams> localAreaBinarySearch;
        private GradientAscentOptimizerParams gradientAscentOptimizerParams;
        private GradientAscentOptimizer<GradientAscentOptimizerParams> gradientAscentOptimizer;

        public SimulatedAnnealingWithLocalAreaBinarySearch(T searchParams)
            : base(searchParams)
        {
            if (searchParams.gradientOptimizerEnabled)
            {
                gradientAscentOptimizerParams = new GradientAscentOptimizerParams
                {
                    dimension = searchParams.dimension,
                    getValue = searchParams.getValue,
                    iterationCount = searchParams.maxIterationsGradientSearch,
                    jumpLengthIterations = searchParams.maxIterationsGradientJumps,
                    maxIterations = searchParams.maxIterations,
                    solutionRange = searchParams.solutionRange,
                    timeLimit = searchParams.timeLimit,
                };
                ((IExternalOptimazerAware)gradientAscentOptimizerParams).externalOptimizerState = GetExternallyInjectedOptimizerState();
                gradientAscentOptimizer = new GradientAscentOptimizer<GradientAscentOptimizerParams>(
                    gradientAscentOptimizerParams);
            }
            if (searchParams.localBinarySearchEnabled)
            {
                localAreaBinarySearchParams = new LocalAreaBinarySearchParams
                {
                    dimension = searchParams.dimension,
                    getValue = searchParams.getValue,
                    maxIterations = searchParams.maxIterations,
                    maxBinarySearchIterations = searchParams.binarySearchIterationCount,
                    iterationsPerDimension = searchParams.binarySearchIterationsPerDimension,
                    solutionRange = searchParams.solutionRange,
                    timeLimit = searchParams.timeLimit,
                };
                ((IExternalOptimazerAware)localAreaBinarySearchParams).externalOptimizerState = GetExternallyInjectedOptimizerState();
                localAreaBinarySearch = new LocalAreaBinarySearch<LocalAreaBinarySearchParams>(
                    localAreaBinarySearchParams);
            }
        }

        private ExternallyInjectedOptimizerState GetExternallyInjectedOptimizerState()
        {
            return new ExternallyInjectedOptimizerState
            {
                BestSolutionSoFar = BestSolutionSoFar,
                CurrentSolution = currentSolution,
                Dimension = problemParameters.dimension,
                ScoreFunction = problemParameters.getValue,
                SolutionRange = problemParameters.solutionRange,
            };
        }

        protected override void NextSolution()
        {
            base.NextSolution();

            var currentValue = problemParameters.getValue(currentSolution);
            if (currentValue > SolutionValue)
            {
                Array.Copy(currentSolution, BestSolutionSoFar, problemParameters.dimension);
                SolutionValue = currentValue;
            }

            if (problemParameters.gradientOptimizerEnabled)
            {
                gradientAscentOptimizerParams.MaxJump = problemParameters.localAreaMultiplier * temperature;
                var externalStateAware = ((IExternalOptimazerAware)gradientAscentOptimizerParams).externalOptimizerState;
                externalStateAware.SolutionValue = SolutionValue;
                externalStateAware.RequestNextSolution();
            }

            if (problemParameters.localBinarySearchEnabled)
            {
                localAreaBinarySearchParams.localArea = problemParameters.localAreaMultiplier * temperature;
                var externalStateAware = ((IExternalOptimazerAware)localAreaBinarySearchParams).externalOptimizerState;
                externalStateAware.SolutionValue = SolutionValue;
                externalStateAware.RequestNextSolution();
            }
            UpdateBestSolution(); // TODO: Every next solution should call this so we should not need it here???
        }
    }
}
