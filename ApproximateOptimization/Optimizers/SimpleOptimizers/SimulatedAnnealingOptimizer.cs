using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingOptimizer : BaseOptimizer
    {
        protected double _temperature;
        protected readonly Random _random;
        private SimulatedAnnealingOptimizerParams _problemParameters;
        private int[] _stagePerDimension;
        const int MaxStages = 4;


        public SimulatedAnnealingOptimizer(SimulatedAnnealingOptimizerParams simulatedAnnealingParams)
            : base(simulatedAnnealingParams)
        {
            _random = new Random(simulatedAnnealingParams.RandomSeed);
            _temperature = simulatedAnnealingParams.InitialTemperature;
            _problemParameters = simulatedAnnealingParams;
            _stagePerDimension = new int[simulatedAnnealingParams.Dimension];
        }

        protected override double NextSolution()
        {
            for (var i=0; i<_stagePerDimension.Length; i++)
            {
                _stagePerDimension[i] = (_stagePerDimension[i] + 1) % MaxStages;
                if (_stagePerDimension[i] != 0) break;
            }

            for (int i = 0; i < _problemParameters.Dimension; i++)
            {
                var rangeWidth = _problemParameters.SolutionRange[i][1] - _problemParameters.SolutionRange[i][0];
                var moreSystematicRandom = ((double)_stagePerDimension[i] / MaxStages) + (_random.NextDouble() / MaxStages);
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


            //for (int i=0; i< _problemParameters.Dimension; i++)
            //{
            //    var rangeWidth = _problemParameters.SolutionRange[i][1] - _problemParameters.SolutionRange[i][0];
            //    _currentSolution[i] = BestSolutionSoFar[i] + (_random.NextDouble() * 2.0 * rangeWidth - rangeWidth) * _temperature;
            //    if (_currentSolution[i] > _problemParameters.SolutionRange[i][1])
            //    {
            //        var excess = _currentSolution[i] - _problemParameters.SolutionRange[i][0];
            //        var excessInRange = excess - Math.Floor(excess / rangeWidth) * rangeWidth;
            //        _currentSolution[i] = _problemParameters.SolutionRange[i][0] + excessInRange;
            //    } else if (_currentSolution[i] < _problemParameters.SolutionRange[i][0])
            //    {
            //        var deficit = _problemParameters.SolutionRange[i][1] - _currentSolution[i];
            //        var deficitInRange = deficit - Math.Floor(deficit / rangeWidth) * rangeWidth;
            //        _currentSolution[i] = _problemParameters.SolutionRange[i][1] - deficitInRange;
            //    }
            //}
            _temperature *= _problemParameters.TemperatureMultiplier;
            LocalAreaAtTheEnd = _temperature;
            return GetCurrentValueAndUpdateBest();
        }
    }
}
