using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ApproximateOptimization
{
    public class SeparateDimensionsOptimizer : IOptimizer
    {
        private BaseOptimizerParams _baseOptimizerParams;
        private int _threads;

        public SeparateDimensionsOptimizer(BaseOptimizerParams baseOptimizerParams, int threads=16)
        {
            _baseOptimizerParams = baseOptimizerParams;
            _threads = threads;
        }

        public double[] BestSolutionSoFar { get; private set; }

        public double SolutionValue { get; private set; }

        public bool SolutionFound { get; private set; }

        //public TimeSpan ElapsedTime { get; private set; }

        //public long IterationsExecuted { get; private set; }

        //public double LocalAreaAtTheEnd { get; private set; }

        public void FindMaximum()
        {
            SolutionValue = double.NegativeInfinity;
            BestSolutionSoFar = new double[_baseOptimizerParams.Dimension];
            for  (var i=0; i<_baseOptimizerParams.MaxIterations; i++)
            {
                Array.Copy(_baseOptimizerParams.StartSolution, BestSolutionSoFar, _baseOptimizerParams.Dimension);
                for (var dimension=0; dimension<BestSolutionSoFar.Length; dimension++)
                {
                    TryChangingDimensionInBestSolution(dimension);
                }
            }
        }

        private void TryChangingDimensionInBestSolution(int dimension)
        {
            var lockSync = new object();
            var threads = new List<Thread>();
            for (var i=0; i<_threads; i++)
            {
                threads.Add(new Thread((threadNum) =>
                {
                    var localSolution = new double[_baseOptimizerParams.Dimension];
                    Array.Copy(BestSolutionSoFar, localSolution, _baseOptimizerParams.Dimension);

                    var width = _baseOptimizerParams.SolutionRange[dimension][1] - _baseOptimizerParams.SolutionRange[dimension][0];
                    localSolution[dimension] = _baseOptimizerParams.SolutionRange[dimension][0] + (_threads <= 1 ? 0.0 : width * ((int)threadNum / (_threads - 1.0) ));

                    var value = _baseOptimizerParams.ScoreFunction(localSolution);
                    lock (lockSync)
                    {
                        if (value > SolutionValue)
                        {
                            SolutionValue = value;
                            Array.Copy(localSolution, BestSolutionSoFar, _baseOptimizerParams.Dimension);
                            SolutionFound = true;
                        }
                    }

                }));
                threads.Last().Start(i);
            }

            foreach (var thread in threads) thread.Join();
        }
    }
}
