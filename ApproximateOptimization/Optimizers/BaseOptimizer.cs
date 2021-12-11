using ApproximateOptimization.Utils;
using System;
using System.Diagnostics;
using System.Threading;

namespace ApproximateOptimization
{

    public abstract class BaseOptimizer : IOptimizer, IOptimizerStats
    {
        protected double[] currentSolution;
        private BaseOptimizerParams problemParameters;

        public double[] BestSolutionSoFar { get; protected set; }

        public double SolutionValue { get; protected set; }

        public bool SolutionFound { get; private set; }

        public TimeSpan ElapsedTime { get; protected set; }

        public long IterationsExecuted { get; protected set; }

        public double LocalAreaAtTheEnd { get; protected set; }

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
            while (true)
            {
                iterations++;
                NextSolution();

                if (problemParameters.maxIterations > 0 && iterations >= problemParameters.maxIterations) break;
                if (problemParameters.timeLimit != default && sw.Elapsed >= problemParameters.timeLimit) break;
                if (problemParameters.CancellationToken != default(CancellationToken)
                    && problemParameters.CancellationToken.IsCancellationRequested)
                    break;
            }
            sw.Stop();
            ElapsedTime = sw.Elapsed;
            IterationsExecuted = iterations;
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
