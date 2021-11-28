using System;
using System.Linq;

namespace ApproximateOptimization
{
    /// <summary>
    /// Runs gradient ascent optimization:
    /// 1. For current point, it finds gradinet in all dimensions (save it as "direction" vector)
    /// 2. Finds the length of the jump along the "direction" using binary search in the range 0..MaxJump
    ///    by attempting "jumpLengthIterations" different lengths.
    /// -> Actions 1&2 are executed "iterationCount" times.
    /// </summary>
    public class GradientAscentOptimizer<T> : BaseSolutionFinder<T> where T: GradientAscentOptimizerParams
    {
        const double delta = 0.00001; // Any number below 0.5 could work?
        private double[] direction;
        private double diagonalLength;

        public GradientAscentOptimizer(T searchParams)
            :base(searchParams)
        {
            direction = new double[searchParams.dimension];
        }

        protected override void NextSolution()
        {
            if (isSelfContained)
            {
                Array.Copy(BestSolutionSoFar, currentSolution, problemParameters.dimension);
            }
            for (int i = 0; i < problemParameters.iterationCount; i++)
            {
                var smallIncrement = problemParameters.MaxJump * delta;
                FindDirection(smallIncrement);
                FindJumpLength();
            }
            if (isSelfContained)
            {
                UpdateBestSolution();
            }
        }

        private void ApplyJump(double jumpLength)
        {
            for (int i = 0; i < problemParameters.dimension; i++)
            {
                currentSolution[i] = BestSolutionSoFar[i] + direction[i] * jumpLength / diagonalLength;
                currentSolution[i] = Math.Max(problemParameters.solutionRange[i][0],
                    Math.Min(problemParameters.solutionRange[i][1], currentSolution[i]));
            }
        }

        private double GetValueForReplacedDimension(int i, double x)
        {
            var initialValue = currentSolution[i];
            currentSolution[i] = x;
            var result = problemParameters.getValue(currentSolution);
            currentSolution[i] = initialValue;
            return result;
        }

        private double GetValueForJump(double jumpLength)
        {
            ApplyJump(jumpLength);
            return problemParameters.getValue(currentSolution);
        }

        private void FindGradientForDimension(int i, double smallIncrement)
        {
            var a = currentSolution[i];
            var b = a + smallIncrement;
            if (b > problemParameters.solutionRange[i][1])
            {
                var tmp = a;
                a = b;
                b = tmp;
            }
            direction[i] = (GetValueForReplacedDimension(i, b) - GetValueForReplacedDimension(i, a)) / (b - a);
        }

        private double GetVectorLength(double[] vector)
        {
            return Math.Sqrt(vector.Sum(x => x * x));
        }

        private void FindDirection(double smallIncrement)
        {
            for (int i = 0; i< problemParameters.dimension; i++)
            {
                var rangeWidth = (problemParameters.solutionRange[i][1] - problemParameters.solutionRange[i][0]);
                FindGradientForDimension(i, smallIncrement * rangeWidth);
            }
            var vectorLength = GetVectorLength(direction);
            for (int i = 0; i < problemParameters.dimension; i++)
            {
                direction[i] = direction[i] / vectorLength;
            }

            // Below could be better (diagonalLength could be smaller) but it would be more complex to find it
            // so the distance to the "corner" is good enough.
            diagonalLength = 0;
            for (int i = 0; i < problemParameters.dimension; i++)
            {
                var distance = BestSolutionSoFar[i] -
                    (
                        direction[i] >= 0 ? problemParameters.solutionRange[i][1] : problemParameters.solutionRange[i][0]
                    );
                diagonalLength += distance * distance;
            }
            diagonalLength = Math.Sqrt(diagonalLength);
        }

        private void FindJumpLength()
        {
            var rangeBegin = 0.0;
            var rangeEnd = problemParameters.MaxJump * diagonalLength;

            var iterationsLeft = problemParameters.iterationCount;
            var bestJumpLength = 0.0;
            var valueForBestJumpLength = SolutionValue;

            while (iterationsLeft-- > 0)
            {
                var rangeWidth = rangeEnd - rangeBegin;
                var mid = rangeBegin + rangeWidth * 0.5;
                var smallIncrement = delta * rangeWidth;
                var justAboveMid = mid + smallIncrement;
                var justBelowMid = mid - smallIncrement;
                if (justAboveMid == justBelowMid) break;

                var justAboveMidValue = GetValueForJump(justAboveMid);
                var justBelowMidValue = GetValueForJump(justBelowMid);
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
                else if (change < 0)
                {
                    rangeEnd = justAboveMid;
                }
                else
                {
                    break;
                }
            }
            if (valueForBestJumpLength > SolutionValue)
            {
                ApplyJump(bestJumpLength);
                UpdateBestSolution();
            }
        }
    }
}
