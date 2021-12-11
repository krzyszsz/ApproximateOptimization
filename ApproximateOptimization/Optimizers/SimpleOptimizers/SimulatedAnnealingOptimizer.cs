using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingOptimizer : BaseOptimizer
    {
        protected double _temperature;
        protected readonly Random _random;
        private SimulatedAnnealingOptimizerParams _problemParameters;

        public SimulatedAnnealingOptimizer(SimulatedAnnealingOptimizerParams simulatedAnnealingParams)
            : base(simulatedAnnealingParams)
        {
            _random = new Random(simulatedAnnealingParams.RandomSeed);
            _temperature = simulatedAnnealingParams.InitialTemperature;
            _problemParameters = simulatedAnnealingParams;
        }

        protected override double NextSolution()
        {
            for (int i=0; i< _problemParameters.Dimension; i++)
            {
                var rangeWidth = _problemParameters.SolutionRange[i][1] - _problemParameters.SolutionRange[i][0];
                _currentSolution[i] = BestSolutionSoFar[i] + (_random.NextDouble() * 2.0 * rangeWidth - rangeWidth) * _temperature;
                if (_currentSolution[i] > _problemParameters.SolutionRange[i][1])
                {
                    var excess = _currentSolution[i] - _problemParameters.SolutionRange[i][0];
                    var excessInRange = excess - Math.Floor(excess / rangeWidth) * rangeWidth;
                    _currentSolution[i] = _problemParameters.SolutionRange[i][0] + excessInRange;
                } else if (_currentSolution[i] < _problemParameters.SolutionRange[i][0])
                {
                    var deficit = _problemParameters.SolutionRange[i][1] - _currentSolution[i];
                    var deficitInRange = deficit - Math.Floor(deficit / rangeWidth) * rangeWidth;
                    _currentSolution[i] = _problemParameters.SolutionRange[i][1] - deficitInRange;
                }
            }
            _temperature *= _problemParameters.TemperatureMultiplier;
            LocalAreaAtTheEnd = _temperature;
            return GetCurrentValueAndUpdateBest();
        }
    }
}
