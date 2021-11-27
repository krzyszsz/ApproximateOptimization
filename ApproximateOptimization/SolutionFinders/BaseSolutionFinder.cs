using System;
using System.Diagnostics;
using System.Linq;

namespace ApproximateOptimization
{
    public abstract class BaseSolutionFinder : ISolutionFinder
    {
        protected double[] currentSolution;
        protected int dimension;
        protected Func<double[], double> getValue;
        protected double[][] solutionRange;

        public double[] BestSolutionSoFar { get; protected set; }

        public double SolutionValue { get; protected set; }

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

        protected virtual void OnInitialized()
        {
        }

        public void FindMaximum(
            int dimension,
            Func<double[], double> getValue,
            TimeSpan timeLimit = default,
            long maxIterations=-1,
            double[][] solutionRange=null)
        {
            ValidateArguments(dimension, timeLimit, maxIterations, solutionRange);
            this.getValue = getValue;
            this.dimension = dimension;
            this.solutionRange = solutionRange ?? GetDefaultSolutionRange(dimension);
            SetInitialSolution();
            BestSolutionSoFar = new double[dimension];
            OnInitialized();
            Array.Copy(currentSolution, BestSolutionSoFar, dimension);
            SolutionValue = getValue(BestSolutionSoFar);
            long iterations = 0;
            var sw = new Stopwatch();
            sw.Start();
            while ((maxIterations > 0 && iterations < maxIterations) || (timeLimit != default && sw.Elapsed < timeLimit))
            {
                iterations++;
                NextSolution();
            }
            sw.Stop();
            SolutionValue = SolutionValue;
            SolutionFound = true;
        }

        internal static double[][] GetDefaultSolutionRange(int dimension)
        {
            double[][] solutionRange = new double[dimension][];
            for (int i = 0; i < dimension; i++)
            {
                solutionRange[i] = new double[2];
                solutionRange[i][0] = 0;
                solutionRange[i][1] = 1;
            }
            return solutionRange;
        }

        internal static void ValidateArguments(int dimension, TimeSpan timeLimit, long maxIterations, double[][] solutionRange)
        {
            if (maxIterations == -1 && timeLimit == default(TimeSpan))
            {
                throw new ArgumentException("Missing timeLimit or maxIterations argument. Without them the algorithm would never stop!");
            }
            if (solutionRange != null && solutionRange.Length != dimension)
            {
                throw new ArgumentException(
                    $"Incorrect range dimension. Expected: {dimension}x2 but got first dimension: {solutionRange.Length}");
            }
            if (solutionRange != null && solutionRange.Any(x => x.Length != 2))
            {
                throw new ArgumentException(
                    $"Incorrect range dimension. Expected: {dimension}x2 but got second dimension: {solutionRange.First(x => x.Length != 2).Length}");
            }
        }

        protected void UpdateBestSolution()
        {
            var value = getValue(currentSolution);
            if (value > SolutionValue)
            {
                Array.Copy(currentSolution, BestSolutionSoFar, dimension);
                SolutionValue = value;
            }
        }
    }
}
