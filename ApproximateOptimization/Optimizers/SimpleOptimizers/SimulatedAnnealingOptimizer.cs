using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingOptimizer : BaseOptimizer
    {
        protected double temperature;
        protected readonly Random random;
        private SimulatedAnnealingOptimizerParams problemParameters;

        public SimulatedAnnealingOptimizer(SimulatedAnnealingOptimizerParams simulatedAnnealingParams)
            : base(simulatedAnnealingParams)
        {
            random = new Random(simulatedAnnealingParams.randomSeed);
            temperature = simulatedAnnealingParams.initialTemperature;
            problemParameters = simulatedAnnealingParams;
        }

        protected override double NextSolution()
        {
            for (int i=0; i< problemParameters.dimension; i++)
            {
                var rangeWidth = problemParameters.solutionRange[i][1] - problemParameters.solutionRange[i][0];
                currentSolution[i] = BestSolutionSoFar[i] + (random.NextDouble() * 2.0 * rangeWidth - rangeWidth) * temperature;
                if (currentSolution[i] > problemParameters.solutionRange[i][1])
                {
                    var excess = currentSolution[i] - problemParameters.solutionRange[i][0];
                    var excessInRange = excess - Math.Floor(excess / rangeWidth) * rangeWidth;
                    currentSolution[i] = problemParameters.solutionRange[i][0] + excessInRange;
                } else if (currentSolution[i] < problemParameters.solutionRange[i][0])
                {
                    var deficit = problemParameters.solutionRange[i][1] - currentSolution[i];
                    var deficitInRange = deficit - Math.Floor(deficit / rangeWidth) * rangeWidth;
                    currentSolution[i] = problemParameters.solutionRange[i][1] - deficitInRange;
                }
            }
            temperature *= problemParameters.temperatureMultiplier;
            LocalAreaAtTheEnd = temperature;
            return GetCurrentValueAndUpdateBest();
        }
    }
}
