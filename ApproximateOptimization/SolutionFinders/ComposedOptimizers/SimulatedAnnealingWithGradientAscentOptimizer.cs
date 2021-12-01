using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This combines multiple optimizers to improve results for some problems:
    /// After each iteration of simulated annealing it runs GradientAscentOptimizer
    /// to find local maximum in the area.
    /// </summary>
    public class SimulatedAnnealingWithGradientAscentOptimizer<T> : SimulatedAnnealingOptimizer<T> where T: SimulatedAnnealingWithGradientAscentOptimizerParams
    {
        private GradientAscentOptimizerParams gradientAscentOptimizerParams;
        private GradientAscentOptimizer<GradientAscentOptimizerParams> gradientAscentOptimizer;

        public SimulatedAnnealingWithGradientAscentOptimizer(T searchParams)
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
            ((IExternalOptimizerAware)gradientAscentOptimizerParams).externalOptimizerState = GetExternallyInjectedOptimizerState();
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
            var externalStateAware = ((IExternalOptimizerAware)gradientAscentOptimizerParams).externalOptimizerState;
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
