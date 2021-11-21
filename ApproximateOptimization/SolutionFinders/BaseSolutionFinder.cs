using System;
using System.Diagnostics;

namespace ApproximateOptimization
{
    public abstract class BaseSolutionFinder : ISolutionFinder
    {
        protected double[] currentSolution;
        protected int dimension;
        protected Func<double[], double> getValue;

        public double[] BestSolutionSoFar { get; private set; }

        public double SolutionValue { get; private set; }

        public bool SolutionFound { get; private set; }

        protected virtual void SetInitialSolution()
        {
            currentSolution = new double[dimension];
        }

        /// <summary>
        /// Implementations of this method should update "currentSolution" and not change "BestSolutionSoFar"
        /// but they can read "BestSolutionSoFar" to create next iteration based on it.
        /// </summary>
        protected abstract void NextSolution();

        public void FindMaximum(int dimension, Func<double[], double> getValue, TimeSpan timeLimit = default, long maxIterations=-1)
        {
            this.getValue = getValue;
            this.dimension = dimension;
            SetInitialSolution();
            BestSolutionSoFar = new double[dimension];
            Array.Copy(currentSolution, BestSolutionSoFar, dimension);
            SolutionValue = getValue(BestSolutionSoFar);
            long iterations = 0;
            var sw = new Stopwatch();
            sw.Start();
            while ((maxIterations > 0 && iterations < maxIterations) || (timeLimit != default && sw.Elapsed < timeLimit))
            {
                iterations++;
                NextSolution();
                var value = getValue(currentSolution);
                if (value > SolutionValue)
                {
                    Array.Copy(currentSolution, BestSolutionSoFar, dimension);
                    SolutionValue = value;
                }
            }
            sw.Stop();
            SolutionValue = SolutionValue;
            SolutionFound = true;
        }
    }
}
