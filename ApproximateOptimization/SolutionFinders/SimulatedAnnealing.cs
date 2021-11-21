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

        protected override void SetInitialSolution()
        {
            base.SetInitialSolution();
            for (int i=0; i<dimension; i++)
            {
                currentSolution[i] = 0.5;
            }
        }

        protected override void NextSolution()
        {
            for (int i=0; i<dimension; i++)
            {
                currentSolution[i] = BestSolutionSoFar[i] + (random.NextDouble() * 2.0 - 1.0) * temperature;
                currentSolution[i] = Math.Max(0, Math.Min(1, currentSolution[i]));
            }
            temperature *= temperatureMultiplier;
        }
    }
}
