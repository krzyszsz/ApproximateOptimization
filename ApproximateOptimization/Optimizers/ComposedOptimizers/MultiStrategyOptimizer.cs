using System;
using System.Collections.Generic;

namespace ApproximateOptimization
{
    /// <summary>
    /// This combines multiple optimizers to improve results for some problems:
    /// After a configurable number of iterations of simulated annealing it runs GradientAscentOptimizer
    /// to find local maximum in the area.
    /// </summary>
    public class MultiStrategyOptimizer : SimulatedAnnealingOptimizer
    {
        private MultiStrategyOptimizerParams _problemParameters;
        private int _switchingCounter;
        private double[] _buffer;
        private PriorityQueue<double[], double> _bestSolutionsForGA;
        private long _counter = 0;

        public MultiStrategyOptimizer(MultiStrategyOptimizerParams searchParams)
            : base(searchParams)
        {
            _problemParameters = searchParams;
            _buffer = new double[searchParams.Dimension];
            if (_problemParameters.GAEnabled)
            {
                _bestSolutionsForGA = new PriorityQueue<double[], double>(_problemParameters.GAPopulation+1);
            }
            UpdatedBestSolution += UpdateGA;
        }

        protected override void RequestNextSolutions(Action<double[], double?> nextSolutionSuggestedCallback)
        {
            base.RequestNextSolutions(nextSolutionSuggestedCallback);
            if (_switchingCounter++ % _problemParameters.SwitchingFreq == _problemParameters.SwitchingFreq - 1)
            {
                CallGradientAscent(nextSolutionSuggestedCallback);
            }

            if (_problemParameters.GAEnabled && ++_counter % _problemParameters.GAPeriod == 0)
            {
                RunGA(nextSolutionSuggestedCallback);
            }
        }

        private void CallGradientAscent(Action<double[], double?> nextSolutionSuggestedCallback)
        {
            var gradientAscentOptimizerParams = new GradientAscentOptimizerParams
            {
                Dimension = _problemParameters.Dimension,
                ScoreFunction = _problemParameters.ScoreFunction,
                JumpLengthIterationsFinal = _problemParameters.JumpLengthIterationsFinal,
                JumpLengthIterationsInitial = _problemParameters.JumpLengthIterationsInitial,
                FinalJumpsNumber = _problemParameters.FinalJumpsNumber,
                MaxIterations = _problemParameters.GradientFollowingIterations,
                SolutionRange = _problemParameters.SolutionRange,
                TimeLimit = _problemParameters.TimeLimit,
                MaxJump = _problemParameters.LocalAreaMultiplier * _temperature / _problemParameters.InitialTemperature,
                StartSolution = BestSolutionSoFar,
                StartSolutionValue = SolutionValue
            };
            var gradientAscentOptimizer = new GradientAscentOptimizer(
                gradientAscentOptimizerParams);

            gradientAscentOptimizer.FindMaximum();

            var bestSolutionWasFoundByAscending = !ArraysEqual(gradientAscentOptimizer.BestSolutionSoFar, BestSolutionSoFar);
            if (bestSolutionWasFoundByAscending)
            {
                nextSolutionSuggestedCallback(gradientAscentOptimizer.BestSolutionSoFar, gradientAscentOptimizer.SolutionValue);
            }
        }


        private void UpdateGA()
        {
            if (_problemParameters.GAEnabled)
            {
                var clonedBest = new double[_problemParameters.Dimension];
                Array.Copy(BestSolutionSoFar, clonedBest, _problemParameters.Dimension);
                _bestSolutionsForGA.Enqueue(clonedBest, SolutionValue);
                if (_bestSolutionsForGA.Count > _problemParameters.GAPopulation)
                {
                    _bestSolutionsForGA.Dequeue();
                }
            }
        }

        private void RunGA(Action<double[], double?> nextSolutionSuggestedCallback)
        {
            var items = new List<double[]>();
            foreach (var item in _bestSolutionsForGA.UnorderedItems)
            {
                items.Add(item.Element);
            }
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                for (var j = 0; j < _problemParameters.GAChildrenPerSolution; j++)
                {
                    if (items.Count < 2) break;
                    var otherElementIdx = _random.Next(items.Count - 1);
                    if (otherElementIdx >= i) otherElementIdx++;
                    var newSolutionToCheck = CrossOver(item, items[otherElementIdx]);
                    nextSolutionSuggestedCallback(newSolutionToCheck, null);
                }
            }
        }

        private double[] CrossOver(double[] item1, double[] item2)
        {
            var result = _buffer;
            for (var i = 0; i < item1.Length; i++)
            {
                var weight = _random.NextDouble();
                result[i] = weight * item1[i] + (1 - weight) * item2[i];
            }
            return result;
        }

        private bool ArraysEqual(double[] a, double[] b)
        {
            for (int i=0; i<a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}
