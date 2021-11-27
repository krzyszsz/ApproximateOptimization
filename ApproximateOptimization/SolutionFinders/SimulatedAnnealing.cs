using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealing : BaseSolutionFinder
    {
        private readonly double temperatureMultiplier;
        protected double temperature = 1.0f;
        protected readonly Random random;

        public SimulatedAnnealing(double temperatureMultiplier=0.99, int randomSeed = 0)
        {
            this.temperatureMultiplier = temperatureMultiplier;
            random = new Random(randomSeed);
        }

        protected override void Initialize()
        {
            base.Initialize();
            for (int i = 0; i < dimension; i++)
            {
                var rangeWidth = solutionRange[i][1] - solutionRange[i][0];
                currentSolution[i] = solutionRange[i][0] + rangeWidth / 2;
            }
        }

        protected override void NextSolution()
        {
            for (int i=0; i<dimension; i++)
            {
                var rangeWidth = solutionRange[i][1] - solutionRange[i][0];
                currentSolution[i] = BestSolutionSoFar[i] + (random.NextDouble() * 2.0 * rangeWidth - rangeWidth) * temperature;
                currentSolution[i] = Math.Max(solutionRange[i][0], Math.Min(solutionRange[i][1], currentSolution[i]));
            }
            temperature *= temperatureMultiplier;
            UpdateBestSolution();
        }
    }
}
