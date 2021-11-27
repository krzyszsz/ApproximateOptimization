using System;
using System.Collections.Generic;

namespace ApproximateOptimization
{
    /// <summary>
    /// This combines 2 other solution finders to improve results for some problems:
    /// After each iteration of simulated annealing it runs LocalAreaBinarySearch with narrowing down local area.
    /// </summary>
    public class SimulatedAnnealingWithLocalAreaBinarySearch : SimulatedAnnealing
    {
        private readonly List<IControllableSolutionFinder> solutionFinders = new List<IControllableSolutionFinder>();
        private readonly double localAreaMultiplier;

        public SimulatedAnnealingWithLocalAreaBinarySearch(double temperatureMultiplier = 0.9, int randomSeed = 0,
            bool localBinarySearchEnabled=false, bool gradientOptimizerEnabled=true,
            double localAreaMultiplier = 0.2, int binarySearchIterationCount = 3, int binarySearchIterationsPerDimension = 10,
            int maxIterationsGradientSearch=50
            )
            : base(temperatureMultiplier, randomSeed)
        {
            this.localAreaMultiplier = localAreaMultiplier;
            if (localBinarySearchEnabled) solutionFinders.Add(
                new LocalAreaBinarySearch(localAreaMultiplier * temperature, binarySearchIterationCount, binarySearchIterationsPerDimension, false));
            if (gradientOptimizerEnabled) solutionFinders.Add(new GradientAscentOptimizer(false, maxIterationsGradientSearch));
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            foreach (var solutionFinder in solutionFinders)
            {
                solutionFinder.Dimension = dimension;
                solutionFinder.CurrentSolution = currentSolution; // both algorithms work on the same array!
                solutionFinder.BestSolutionSoFar = BestSolutionSoFar;
                solutionFinder.ScoreFunction = getValue;
                solutionFinder.SolutionRange = solutionRange;
            }
        }

        protected override void NextSolution()
        {
            base.NextSolution();

            var currentValue = getValue(currentSolution);
            if (currentValue > SolutionValue)
            {
                Array.Copy(currentSolution, BestSolutionSoFar, dimension);
                SolutionValue = currentValue;
            }
            foreach (var solutionFinder in solutionFinders)
            {
                var binarySearch = solutionFinder as IControllableLocalAreaSolutionFinder;
                var gradientOptimizer = solutionFinder as IControllableGradientAscentOptimizer;
                if (binarySearch != null)
                {
                    binarySearch.LocalArea = localAreaMultiplier * temperature;
                }
                if (gradientOptimizer != null)
                {
                    gradientOptimizer.MaxJump = localAreaMultiplier * temperature;
                }
                solutionFinder.SolutionValue = SolutionValue;
                solutionFinder.NextSolution();
                UpdateBestSolution();
            }
        }
    }
}
