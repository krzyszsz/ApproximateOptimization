using ApproximateOptimization.Utils;
using System;
using System.Diagnostics;
using System.Threading;

namespace ApproximateOptimization
{
    /// <summary>
    /// Runs composite optimizer as long as the required precision is achieved or time exceeded.
    /// </summary>
    public class EasyOptimizer : IOptimizer, IOptimizerStats
    {
        private EasyOptimizerParams _problemParameters;

        public double[] BestSolutionSoFar { get; protected set; }

        public double SolutionValue { get; protected set; }

        public bool SolutionFound { get; private set; }

        public TimeSpan ElapsedTime { get; protected set; }

        public long IterationsExecuted { get; protected set; }

        public double LocalAreaAtTheEnd { get; protected set; }

        public EasyOptimizer(EasyOptimizerParams optimizerParams)
        {
            optimizerParams.ProcessStandardParametersForConstructor();
            _problemParameters = optimizerParams;
            BestSolutionSoFar = new double[_problemParameters.Dimension];
        }

        public void FindMaximum()
        {
            SolutionValue = _problemParameters.StartSolution == null ? double.NegativeInfinity : _problemParameters.ScoreFunction(BestSolutionSoFar);
            var sw = new Stopwatch();
            sw.Start();
            var loopNumber = 0;
            var timeLeft = _problemParameters.TimeLimit != default ? _problemParameters.TimeLimit - sw.Elapsed : default;
            while (true)
            {
                var requiredIterations = (int)(_problemParameters.InitialIterations
                    * Math.Pow(_problemParameters.IterationsScaler, loopNumber++)); // Each time "IterationsScaler" x more iterations

                var optimizer = OptimizerFactory.GetCompositeOptimizer(
                    new SimulatedAnnealingWithGradientAscentOptimizerParams
                    {
                        ScoreFunction = _problemParameters.ScoreFunction,
                        Dimension = _problemParameters.Dimension,
                        MaxIterations = requiredIterations,
                        SolutionRange = _problemParameters.SolutionRange,
                        InitialTemperature = 10.0,
                        TemperatureMultiplier = Math.Pow(_problemParameters.RequiredPrecision / 10.0, 1.0 / requiredIterations),
                        CancellationToken = _problemParameters.CancellationToken,
                        StartSolution = _problemParameters.StartSolution,
                        TimeLimit = timeLeft,
                    }, rangeDiscovery: false, threads: _problemParameters.Threads);
                optimizer.FindMaximum();

                var optimizerStats = optimizer as IOptimizerStats;
                if (optimizerStats != null)
                {
                    IterationsExecuted += optimizerStats.IterationsExecuted;
                }

                var solutionImprovement = optimizer.SolutionValue - SolutionValue;

                if (optimizer.SolutionValue > SolutionValue)
                {
                    SolutionValue = optimizer.SolutionValue;
                    Array.Copy(optimizer.BestSolutionSoFar, BestSolutionSoFar, BestSolutionSoFar.Length);
                }

                timeLeft = _problemParameters.TimeLimit != default ? _problemParameters.TimeLimit - sw.Elapsed : default;
                if (_problemParameters.TimeLimit != default && timeLeft <= TimeSpan.Zero) break;
                if (_problemParameters.CancellationToken != default(CancellationToken)
                    && _problemParameters.CancellationToken.IsCancellationRequested)
                    break;
                if (solutionImprovement < _problemParameters.RequiredPrecision)
                {
                    break;
                }
            }
            sw.Stop();
            ElapsedTime = sw.Elapsed;
            SolutionFound = true;
        }
    }
}
