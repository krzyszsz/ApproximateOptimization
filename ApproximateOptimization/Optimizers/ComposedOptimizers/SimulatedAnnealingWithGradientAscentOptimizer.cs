using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This combines multiple optimizers to improve results for some problems:
    /// After each iteration of simulated annealing it runs GradientAscentOptimizer
    /// to find local maximum in the area.
    /// </summary>
    public class SimulatedAnnealingWithGradientAscentOptimizer : SimulatedAnnealingOptimizer
    {
        private GradientAscentOptimizerParams gradientAscentOptimizerParams;
        private SimulatedAnnealingWithGradientAscentOptimizerParams problemParameters;

        public SimulatedAnnealingWithGradientAscentOptimizer(SimulatedAnnealingWithGradientAscentOptimizerParams searchParams)
            : base(searchParams)
        {
            problemParameters = searchParams;
            gradientAscentOptimizerParams = new GradientAscentOptimizerParams
            {
                dimension = searchParams.dimension,
                scoreFunction = searchParams.scoreFunction,
                gradientFollowingIterations = searchParams.gradientFollowingIterations,
                jumpLengthIterationsFinal = searchParams.jumpLengthIterationsFinal,
                jumpLengthIterationsInitial = searchParams.jumpLengthIterationsInitial,
                finalJumpsNumber = searchParams.finalJumpsNumber,
                maxIterations = searchParams.maxIterations,
                solutionRange = searchParams.solutionRange,
                timeLimit = searchParams.timeLimit,
            };
            ((IExternalOptimizerAware)gradientAscentOptimizerParams).externalOptimizerState = GetExternallyInjectedOptimizerState();
            var gradientAscentOptimizer = new GradientAscentOptimizer(
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
            gradientAscentOptimizerParams.MaxJump = problemParameters.localAreaMultiplier * temperature / problemParameters.initialTemperature;
            var currentValue = base.NextSolution();
            var externalStateAware = ((IExternalOptimizerAware)gradientAscentOptimizerParams).externalOptimizerState;
            externalStateAware.SolutionValue = currentValue;
            Array.Copy(currentSolution, externalStateAware.CurrentSolutionAtStart, problemParameters.dimension);
            Array.Copy(currentSolution, externalStateAware.BestSolutionSoFar, problemParameters.dimension);
            currentValue = externalStateAware.RequestNextSolution();
            if (currentValue > SolutionValue)
            {
                Array.Copy(externalStateAware.BestSolutionSoFar, BestSolutionSoFar, problemParameters.dimension);
                // Array.Copy(externalStateAware.BestSolutionSoFar, currentSolution, problemParameters.dimension);
                SolutionValue = currentValue;
            }
            return currentValue;
        }
    }
}
