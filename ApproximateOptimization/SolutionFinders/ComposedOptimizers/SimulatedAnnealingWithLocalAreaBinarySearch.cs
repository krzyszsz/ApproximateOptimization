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
        private GradientAscentOptimizerParams gradientAscentOptimizerParams;
        private GradientAscentOptimizer<GradientAscentOptimizerParams> gradientAscentOptimizer;

        public SimulatedAnnealingWithLocalAreaBinarySearch(T searchParams)
            : base(searchParams)
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
            gradientAscentOptimizerParams.MaxJump = problemParameters.localAreaMultiplier * temperature;
        }

        private ExternallyInjectedOptimizerState GetExternallyInjectedOptimizerState()
        {
            return new ExternallyInjectedOptimizerState
            {
                BestSolutionSoFar = new double[problemParameters.dimension],
                CurrentSolution = currentSolution,
                CurrentSolutionAtStart = new double[problemParameters.dimension],
            };
        }

        protected override double NextSolution()
        {
            var currentValue = base.NextSolution();
            var externalStateAware = ((IExternalOptimazerAware)gradientAscentOptimizerParams).externalOptimizerState;
            externalStateAware.SolutionValue = currentValue;
            Array.Copy(currentSolution, externalStateAware.CurrentSolutionAtStart, problemParameters.dimension);
            Array.Copy(currentSolution, externalStateAware.BestSolutionSoFar, problemParameters.dimension);
            externalStateAware.RequestNextSolution();
            if (externalStateAware.SolutionValue > SolutionValue)
            {
                Array.Copy(externalStateAware.BestSolutionSoFar, BestSolutionSoFar, problemParameters.dimension);
                // Array.Copy(externalStateAware.BestSolutionSoFar, currentSolution, problemParameters.dimension);
                SolutionValue = externalStateAware.SolutionValue;
            }
            gradientAscentOptimizerParams.MaxJump = problemParameters.localAreaMultiplier * temperature;
            return currentValue;
        }
    }
}
