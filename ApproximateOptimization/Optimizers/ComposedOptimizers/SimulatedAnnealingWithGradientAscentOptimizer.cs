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
        private GradientAscentOptimizerParams _gradientAscentOptimizerParams;
        private SimulatedAnnealingWithGradientAscentOptimizerParams _problemParameters;

        public SimulatedAnnealingWithGradientAscentOptimizer(SimulatedAnnealingWithGradientAscentOptimizerParams searchParams)
            : base(searchParams)
        {
            _problemParameters = searchParams;
            _gradientAscentOptimizerParams = new GradientAscentOptimizerParams
            {
                Dimension = searchParams.Dimension,
                ScoreFunction = searchParams.ScoreFunction,
                JumpLengthIterationsFinal = searchParams.JumpLengthIterationsFinal,
                JumpLengthIterationsInitial = searchParams.JumpLengthIterationsInitial,
                FinalJumpsNumber = searchParams.FinalJumpsNumber,
                MaxIterations = searchParams.GradientFollowingIterations,
                SolutionRange = searchParams.SolutionRange,
                TimeLimit = searchParams.TimeLimit,
            };
            ((IExternalOptimizerAware)_gradientAscentOptimizerParams).ExternalOptimizerState = GetExternallyInjectedOptimizerState();
            var gradientAscentOptimizer = new GradientAscentOptimizer(
                _gradientAscentOptimizerParams);
            _gradientAscentOptimizerParams.MaxJump = _problemParameters.LocalAreaMultiplier * _temperature;
        }

        private ExternallyInjectedOptimizerState GetExternallyInjectedOptimizerState()
        {
            return new ExternallyInjectedOptimizerState
            {
                BestSolutionSoFar = new double[_problemParameters.Dimension],
                CurrentSolution = _currentSolution,
                CurrentSolutionAtStart = new double[_problemParameters.Dimension],
            };
        }

        protected override double NextSolution()
        {
            _gradientAscentOptimizerParams.MaxJump = _problemParameters.LocalAreaMultiplier * _temperature / _problemParameters.InitialTemperature;
            var currentValue = base.NextSolution();
            var externalStateAware = ((IExternalOptimizerAware)_gradientAscentOptimizerParams).ExternalOptimizerState;
            externalStateAware.SolutionValue = currentValue;
            Array.Copy(_currentSolution, externalStateAware.CurrentSolutionAtStart, _problemParameters.Dimension);
            Array.Copy(_currentSolution, externalStateAware.BestSolutionSoFar, _problemParameters.Dimension);
            currentValue = externalStateAware.RequestNextSolution();
            if (currentValue > SolutionValue)
            {
                Array.Copy(externalStateAware.BestSolutionSoFar, BestSolutionSoFar, _problemParameters.Dimension);
                // Array.Copy(externalStateAware.BestSolutionSoFar, currentSolution, problemParameters.dimension);
                SolutionValue = currentValue;
            }
            return currentValue;
        }
    }
}
