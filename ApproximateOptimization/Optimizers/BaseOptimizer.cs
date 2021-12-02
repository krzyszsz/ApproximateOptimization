using ApproximateOptimization.Utils;
using System;
using System.Diagnostics;

namespace ApproximateOptimization
{
    public abstract class BaseOptimizer : IOptimizer
    {
        protected double[] currentSolution;
        private BaseOptimizerParams problemParameters;

        public double[] BestSolutionSoFar { get; protected set; }

        public double SolutionValue { get; protected set; }

        public bool SolutionFound { get; private set; }

        /// <summary>
        /// Implementations of this method should update "currentSolution" and not change "BestSolutionSoFar"
        /// but they can read "BestSolutionSoFar" to create next iteration based on it.
        /// </summary>
        protected abstract double NextSolution();

        public BaseOptimizer(BaseOptimizerParams optimizerParams)
        {
            optimizerParams.ProcessStandardParametersForConstructor();
            problemParameters = optimizerParams;
            BestSolutionSoFar = new double[problemParameters.dimension];
            currentSolution = new double[problemParameters.dimension];
            var paramsFromExternalOptimizer = problemParameters as IExternalOptimizerAware;
            if (paramsFromExternalOptimizer?.externalOptimizerState != null)
            {
                paramsFromExternalOptimizer.externalOptimizerState.RequestNextSolution = NextSolution;
            }
            else
            {
                SetInitialSolution();
            }
        }

        public void FindMaximum()
        {
            Array.Copy(currentSolution, BestSolutionSoFar, problemParameters.dimension);
            SolutionValue = problemParameters.scoreFunction(BestSolutionSoFar);
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
            SolutionFound = true;
        }

        protected double GetCurrentValueAndUpdateBest()
        {
            var value = problemParameters.scoreFunction(currentSolution);
            if (value > SolutionValue)
            {
                Array.Copy(currentSolution, BestSolutionSoFar, problemParameters.dimension);
                SolutionValue = value;
            }
            return value;
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
