using System;
using System.Diagnostics;

namespace ApproximateOptimization
{
    public abstract class BaseSolutionFinder<T> : ISolutionFinder<T> where T : BaseSolutionFinderParams
    {
        private bool isInitialized;
        protected bool isSelfContained;
        protected double[] currentSolution;
        protected T problemParameters;

        public double[] BestSolutionSoFar { get; protected set; }

        public double SolutionValue { get; protected set; }

        public bool SolutionFound { get; private set; }

        /// <summary>
        /// Implementations of this method should update "currentSolution" and not change "BestSolutionSoFar"
        /// but they can read "BestSolutionSoFar" to create next iteration based on it.
        /// </summary>
        protected abstract void NextSolution();

        public virtual void Initialize(T solutionFinderParams)
        {
            solutionFinderParams.ValidateArguments();
            isInitialized = true;
            problemParameters = solutionFinderParams;
            BestSolutionSoFar = new double[problemParameters.dimension];
            currentSolution = new double[problemParameters.dimension];
            var paramsFromExternalOptimizer = problemParameters as IExternalOptimazerAware;
            if (paramsFromExternalOptimizer?.externalOptimizerState != null)
            {
                paramsFromExternalOptimizer.externalOptimizerState.RequestNextSolution = NextSolution;
            }
            else
            {
                isSelfContained = true;
                SetInitialSolution();
            }
        }

        private void ValidateInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Not initialized. Please call Initialize before FindMaximum.");
            }
        }

        public void FindMaximum()
        {
            ValidateInitialized();
            Array.Copy(currentSolution, BestSolutionSoFar, problemParameters.dimension);
            SolutionValue = problemParameters.getValue(BestSolutionSoFar);
            long iterations = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (
                (problemParameters.maxIterations > 0 && iterations < problemParameters.maxIterations) || 
                (problemParameters.timeLimit != default && sw.Elapsed < problemParameters.timeLimit))
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

        protected void UpdateBestSolution()
        {
            var value = problemParameters.getValue(currentSolution);
            if (value > SolutionValue)
            {
                Array.Copy(currentSolution, BestSolutionSoFar, problemParameters.dimension);
                SolutionValue = value;
            }
        }

        protected void SetInitialSolution()
        {
            for (int i = 0; i < problemParameters.dimension; i++)
            {
                var rangeWidth = problemParameters.solutionRange[i][1] - problemParameters.solutionRange[i][0];
                currentSolution[i] = problemParameters.solutionRange[i][0] + rangeWidth / 2;
            }
        }
    }
}
