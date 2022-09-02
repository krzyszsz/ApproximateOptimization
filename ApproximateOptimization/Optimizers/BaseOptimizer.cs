using ApproximateOptimization.Utils;
using System;
using System.Diagnostics;
using System.Threading;

namespace ApproximateOptimization
{

    public abstract class BaseOptimizer : IOptimizer, IOptimizerStats
    {
        protected double[] _currentSolution;
        private BaseOptimizerParams _problemParameters;

        public double[] BestSolutionSoFar { get; protected set; }

        public double SolutionValue { get; protected set; }

        public bool SolutionFound { get; private set; }

        public TimeSpan ElapsedTime { get; protected set; }

        public long IterationsExecuted { get; protected set; }

        public double LocalAreaAtTheEnd { get; protected set; }

        /// <summary>
        /// Implementations of this method should update "currentSolution" and not change "BestSolutionSoFar"
        /// but they can read "BestSolutionSoFar" to create next iteration based on it.
        /// 
        /// This method can create multiple new solutions to check - each one should be passed as an argument of the callback.
        /// The callback will return true if the new solution is an improvement.
        /// </summary>
        protected abstract void RequestNextSolutions(Action<double[], double?> nextSolutionSuggestedCallback);

        public BaseOptimizer(BaseOptimizerParams optimizerParams)
        {
            optimizerParams.ProcessStandardParametersForConstructor();
            _problemParameters = optimizerParams;
            BestSolutionSoFar = new double[_problemParameters.Dimension];
            _currentSolution = new double[_problemParameters.Dimension];
            SetInitialSolution();
        }

        private void CheckSolution(double[] solution, double? givenValue)
        {
            if (solution != _currentSolution)
            {
                Array.Copy(solution, _currentSolution, _problemParameters.Dimension);
            }
            GetCurrentValueAndUpdateBest(givenValue);
        }

        public void FindMaximum()
        {
            Array.Copy(_currentSolution, BestSolutionSoFar, _problemParameters.Dimension);
            SolutionValue = _problemParameters.StartSolution == null ? double.NegativeInfinity : _problemParameters.ScoreFunction(BestSolutionSoFar);
            long iterations = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                iterations++;
                RequestNextSolutions(CheckSolution);

                if (_problemParameters.MaxIterations > 0 && iterations >= _problemParameters.MaxIterations) break;
                if (_problemParameters.TimeLimit != default && sw.Elapsed >= _problemParameters.TimeLimit && iterations >= _problemParameters.MinIterations) break;
                if (_problemParameters.CancellationToken != default(CancellationToken)
                    && _problemParameters.CancellationToken.IsCancellationRequested)
                    break;
            }
            sw.Stop();
            ElapsedTime = sw.Elapsed;
            IterationsExecuted = iterations;
            SolutionFound = true;
        }

        public event Action UpdatedBestSolution;

        private double GetCurrentValueAndUpdateBest(double? givenValue)
        {
            var value = givenValue ?? _problemParameters.ScoreFunction(_currentSolution);
            if (value > SolutionValue)
            {
                Array.Copy(_currentSolution, BestSolutionSoFar, _problemParameters.Dimension);
                SolutionValue = value;
                UpdatedBestSolution?.Invoke();
            }
            return value;
        }

        protected void SetInitialSolution()
        {
            if (_problemParameters.StartSolution != null)
            {
                Array.Copy(_problemParameters.StartSolution, _currentSolution, _problemParameters.Dimension);
                if (_problemParameters.StartSolutionValue != null)
                {
                    SolutionValue = _problemParameters.StartSolutionValue.Value;
                }
                return;
            }
            for (int i = 0; i < _problemParameters.Dimension; i++)
            {
                var rangeWidth = _problemParameters.SolutionRange[i][1] - _problemParameters.SolutionRange[i][0];
                _currentSolution[i] = _problemParameters.SolutionRange[i][0] + rangeWidth / 2;
            }
        }
    }
}
