using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingOptimizer : BaseOptimizer
    {
        protected double _temperature;
        protected readonly Random _random;
        private SimulatedAnnealingOptimizerParams _problemParameters;
        private int[] _stagePerDimension;


        public SimulatedAnnealingOptimizer(SimulatedAnnealingOptimizerParams simulatedAnnealingParams)
            : base(simulatedAnnealingParams)
        {
            _random = new Random(simulatedAnnealingParams.RandomSeed);
            _temperature = simulatedAnnealingParams.InitialTemperature;
            _problemParameters = simulatedAnnealingParams;
            _stagePerDimension = new int[simulatedAnnealingParams.Dimension];
        }

        /// <summary>
        /// For problems where checking each solution takes a long time, it may be beneficial if each SimulatedAnnealigOptimizer
        /// starts in a different part of the solutions space. This way we can run these optimizers concurrently and have good results
        /// quicker.
        /// </summary>
        /// <param name="stage">Value from range 0 (inclusive) to 1 (inclusive) indicating part of the solutions space to start.</param>
        public void SetInitialStage(double stage)
        {
            stage = stage - (long)stage;
            var stageInt = (int)(stage * Math.Pow(_problemParameters.MaxStages, _stagePerDimension.Length));
            for (var position = _problemParameters.Dimension - 1; position >= 0; position--)
            {
                int digit = stageInt / _problemParameters.MaxStages;
                stageInt = stageInt - digit * _problemParameters.MaxStages;
                _stagePerDimension[position] = digit;
            }
        }

        protected override double NextSolution()
        {
            for (var i = 0; i < _stagePerDimension.Length; i++)
            {
                _stagePerDimension[i] = (_stagePerDimension[i] + 1) % _problemParameters.MaxStages;
                if (_stagePerDimension[i] != 0) break;
            }

            for (int i = 0; i < _problemParameters.Dimension; i++)
            {
                var rangeWidth = _problemParameters.SolutionRange[i][1] - _problemParameters.SolutionRange[i][0];
                var moreSystematicRandom = ((double)_stagePerDimension[i] / _problemParameters.MaxStages) + (_random.NextDouble() / _problemParameters.MaxStages);
                if (moreSystematicRandom > 1.0) moreSystematicRandom = moreSystematicRandom - 1.0;
                _currentSolution[i] = BestSolutionSoFar[i] + (moreSystematicRandom * 2.0 * rangeWidth - rangeWidth) * _temperature;
                if (_currentSolution[i] > _problemParameters.SolutionRange[i][1])
                {
                    var excess = _currentSolution[i] - _problemParameters.SolutionRange[i][0];
                    var excessInRange = excess - Math.Floor(excess / rangeWidth) * rangeWidth;
                    _currentSolution[i] = _problemParameters.SolutionRange[i][0] + excessInRange;
                }
                else if (_currentSolution[i] < _problemParameters.SolutionRange[i][0])
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
