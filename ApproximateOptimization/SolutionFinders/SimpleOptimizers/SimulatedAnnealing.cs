using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealing<T> : BaseSolutionFinder<T> where T : SimulatedAnnealingParams
    {
        protected double temperature = 1.0f;
        protected readonly Random random;

        public SimulatedAnnealing(T simulatedAnnealingParams)
            : base(simulatedAnnealingParams)
        {
            random = new Random(simulatedAnnealingParams.randomSeed);
        }

        protected override void NextSolution()
        {
            for (int i=0; i< problemParameters.dimension; i++)
            {
                var rangeWidth = problemParameters.solutionRange[i][1] - problemParameters.solutionRange[i][0];
                currentSolution[i] = BestSolutionSoFar[i] + (random.NextDouble() * 2.0 * rangeWidth - rangeWidth) * temperature;
                currentSolution[i] = Math.Max(problemParameters.solutionRange[i][0],
                    Math.Min(problemParameters.solutionRange[i][1], currentSolution[i]));
            }
            temperature *= problemParameters.temperatureMultiplier;
            UpdateBestSolution();
        }
    }
}
