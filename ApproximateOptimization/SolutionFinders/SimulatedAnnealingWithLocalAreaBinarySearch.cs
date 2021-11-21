using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This combines 2 other solution finders to improve results for some problems:
    /// After each iteration of simulated annealing it runs LocalAreaBinarySearch with narrowing down local area.
    /// </summary>
    public class SimulatedAnnealingWithLocalAreaBinarySearch : SimulatedAnnealing
    {
        private readonly LocalAreaBinarySearch localAreaBinarySearch;
        private readonly IControllableLocalAreaSolutionFinder controllableLocalAreaSolutionFinder;
        private readonly double localAreaMultiplier;

        public SimulatedAnnealingWithLocalAreaBinarySearch(double temperatureMultiplier = 0.9, int randomSeed = 0,
            double localAreaMultiplier = 0.2, int iterationCount = 3, int iterationsPerDimension = 10)
            : base(temperatureMultiplier, randomSeed)
        {
            this.localAreaMultiplier = localAreaMultiplier;
            localAreaBinarySearch = new LocalAreaBinarySearch(localAreaMultiplier * temperature, iterationCount, iterationsPerDimension, false);
            controllableLocalAreaSolutionFinder = localAreaBinarySearch;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            controllableLocalAreaSolutionFinder.Dimension = dimension;
            controllableLocalAreaSolutionFinder.CurrentSolution = currentSolution; // both algorithms work on the same array!
            controllableLocalAreaSolutionFinder.BestSolutionSoFar = BestSolutionSoFar;
            controllableLocalAreaSolutionFinder.ScoreFunction = getValue;
        }

        protected override void NextSolution()
        {
            base.NextSolution();
            controllableLocalAreaSolutionFinder.LocalArea = localAreaMultiplier * temperature;
            var currentValue = getValue(currentSolution);
            if (currentValue > SolutionValue)
            {
                Array.Copy(currentSolution, BestSolutionSoFar, dimension);
                SolutionValue = currentValue;
            }
            controllableLocalAreaSolutionFinder.SolutionValue = SolutionValue;
            controllableLocalAreaSolutionFinder.NextSolution();
        }
    }
}
