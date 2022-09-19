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
            if (SolutionFound) throw new ApplicationException("Cannot call FindMaximum twice on the same instance of optimizer.");
            BestSolutionSoFar = new double[_baseOptimizerParams.Dimension];
            Array.Copy(_baseOptimizerParams.StartSolution, BestSolutionSoFar, _baseOptimizerParams.Dimension);
            SolutionValue = _baseOptimizerParams.ScoreFunction(BestSolutionSoFar);
            for  (var i=0; i<_baseOptimizerParams.MaxIterations; i++)
            {
                for (var dimension=0; dimension<BestSolutionSoFar.Length; dimension++)
                {
                    TryChangingDimensionInBestSolution(dimension);
                }
                if (i % 2 == 1) // TODO: Parametrize!
                {
                    for (var d = 0; d < _baseOptimizerParams.Dimension; d++)
                    {
                        var width = _baseOptimizerParams.SolutionRange[d][1] - _baseOptimizerParams.SolutionRange[d][0];
                        var newWidth = width * 0.8; // TOD: Parametrize!
                        var halfNewWidth = 0.5 * newWidth;
                        _baseOptimizerParams.SolutionRange[d][0] = Math.Max(_baseOptimizerParams.SolutionRange[d][0], BestSolutionSoFar[d] - halfNewWidth);
                        _baseOptimizerParams.SolutionRange[d][1] = Math.Min(_baseOptimizerParams.SolutionRange[d][1], BestSolutionSoFar[d] + halfNewWidth);
                    }
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
                    lock (lockSync)
                    {
                        Array.Copy(BestSolutionSoFar, localSolution, _baseOptimizerParams.Dimension);
                    }

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
