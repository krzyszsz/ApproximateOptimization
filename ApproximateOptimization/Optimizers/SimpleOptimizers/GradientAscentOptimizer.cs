using System;
using System.Linq;

namespace ApproximateOptimization
{
    /// <summary>
    /// Runs gradient ascent optimization:
    /// 1. For current point, it finds gradient in all dimensions (save it as "direction" vector)
    /// 2. Finds the length of the jump along the "direction" using binary search in the range 0..MaxJump
    ///    by attempting "jumpLengthIterations" different lengths.
    /// -> Actions 1&2 are executed "iterationsNumber" times.
    /// </summary>
    public class GradientAscentOptimizer: BaseOptimizer
    {
        const double _delta = 0.001;
        private double[] _direction;
        private double _diagonalLength;
        private GradientAscentOptimizerParams _problemParameters;

        public GradientAscentOptimizer(GradientAscentOptimizerParams searchParams)
            :base(searchParams)
        {
            _problemParameters = searchParams;
            _direction = new double[searchParams.Dimension];
        }

        protected override void RequestNextSolutions(Action<double[], double?> nextSolutionSuggestedCallback)
        {
            var smallIncrement = _problemParameters.MaxJump * _delta;
            for (int i = 0; i < _problemParameters.MaxIterations; i++)
            {
                FindDirection(smallIncrement);
                var moreAccurateJumpLengths =
                    _problemParameters.MaxIterations - i <= _problemParameters.FinalJumpsNumber;
                FindJumpLength(moreAccurateJumpLengths, nextSolutionSuggestedCallback);
            }
        }

        private void ApplyJump(double jumpLength)
        {
            for (int i = 0; i < _problemParameters.Dimension; i++)
            {
                _currentSolution[i] = BestSolutionSoFar[i] + _direction[i] * jumpLength / _diagonalLength;
                _currentSolution[i] = Math.Max(_problemParameters.SolutionRange[i][0],
                    Math.Min(_problemParameters.SolutionRange[i][1], _currentSolution[i]));
            }
        }

        private double GetScoreForReplacedDimension(int i, double x)
        {
            var initialValue = _currentSolution[i];
            _currentSolution[i] = x;
            var result = _problemParameters.ScoreFunction(_currentSolution);
            _currentSolution[i] = initialValue;
            return result;
        }

        private double GetScoreForJump(double jumpLength)
        {
            ApplyJump(jumpLength);
            return _problemParameters.ScoreFunction(_currentSolution);
        }

        private void FindGradientForDimension(int i, double smallIncrement)
        {
            var a = _currentSolution[i] - smallIncrement;
            var b = _currentSolution[i] + smallIncrement;
            _direction[i] = (GetScoreForReplacedDimension(i, b) - GetScoreForReplacedDimension(i, a)) / (b - a);
        }

        private double GetVectorLength(double[] vector)
        {
            return Math.Sqrt(vector.Sum(x => x * x));
        }

        private void FindDirection(double smallIncrement)
        {
            for (int i = 0; i< _problemParameters.Dimension; i++)
            {
                var rangeWidth = (_problemParameters.SolutionRange[i][1] - _problemParameters.SolutionRange[i][0]);
                FindGradientForDimension(i, smallIncrement * rangeWidth);
            }
            var vectorLength = GetVectorLength(_direction);
            for (int i = 0; i < _problemParameters.Dimension; i++)
            {
                _direction[i] = _direction[i] / vectorLength;
            }

            // Below could be better (diagonalLength could be smaller) but it would be more complex to find it
            // so the distance to the "corner" is good enough.
            _diagonalLength = 0;
            for (int i = 0; i < _problemParameters.Dimension; i++)
            {
                var distance = BestSolutionSoFar[i] -
                    (
                        _direction[i] >= 0 ? _problemParameters.SolutionRange[i][1] : _problemParameters.SolutionRange[i][0]
                    );
                _diagonalLength += distance * distance;
            }
            _diagonalLength = Math.Sqrt(_diagonalLength);
        }

        private void FindJumpLength(bool moreAccurateJumpLengths, Action<double[], double?> nextSolutionSuggestedCallback)
        {
            var rangeBegin = 0.0;
            var rangeEnd = _problemParameters.MaxJump * _diagonalLength;

            var iterationsLeft = moreAccurateJumpLengths
                ? _problemParameters.JumpLengthIterationsFinal
                : _problemParameters.JumpLengthIterationsInitial;
            var bestJumpLength = 0.0;
            var valueForBestJumpLength = SolutionValue;

            while (iterationsLeft-- > 0)
            {
                var rangeWidth = rangeEnd - rangeBegin;
                var mid = rangeBegin + rangeWidth * 0.5;
                var smallIncrement = _delta * rangeWidth;
                var justAboveMid = mid + smallIncrement;
                var justBelowMid = mid - smallIncrement;
                if (justAboveMid == justBelowMid) break;

                var justAboveMidValue = GetScoreForJump(justAboveMid);
                var justBelowMidValue = GetScoreForJump(justBelowMid);
                var change = justAboveMidValue - justBelowMidValue;

                if (change > 0)
                {
                    rangeBegin = justBelowMid;

                    // ApplyJump(justBelowMid);
                    // var currentValue = getValue(currentSolution);
                    if (justBelowMidValue > valueForBestJumpLength)
                    {
                        bestJumpLength = justBelowMid;
                        valueForBestJumpLength = justBelowMidValue;
                    }
                }
                else
                {
                    rangeEnd = justAboveMid;
                }
            }
            if (valueForBestJumpLength > SolutionValue)
            {
                ApplyJump(bestJumpLength);
                nextSolutionSuggestedCallback(_currentSolution, valueForBestJumpLength);
            }
        }
    }
}
