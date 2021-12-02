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
        const double delta = 0.00001; // Any number below 0.5 could work?
        private double[] direction;
        private double diagonalLength;
        private ExternallyInjectedOptimizerState externalState;
        private GradientAscentOptimizerParams problemParameters;

        public GradientAscentOptimizer(GradientAscentOptimizerParams searchParams)
            :base(searchParams)
        {
            problemParameters = searchParams;
            direction = new double[searchParams.dimension];
            externalState = ((IExternalOptimizerAware)problemParameters)?.externalOptimizerState;
            if (externalState != null)
            {
                BestSolutionSoFar = externalState.CurrentSolutionAtStart;
                currentSolution = externalState.CurrentSolution;
            }
        }

        protected override double NextSolution()
        {
            if (externalState != null)
            {
                SolutionValue = externalState.SolutionValue;
            }
            var smallIncrement = problemParameters.MaxJump * delta;
            for (int i = 0; i < problemParameters.gradientFollowingIterations; i++)
            {
                FindDirection(smallIncrement);
                var moreAccurateJumpLengths =
                    problemParameters.gradientFollowingIterations - i <= problemParameters.finalJumpsNumber;
                FindJumpLength(moreAccurateJumpLengths);
            }
            var currentValue = GetCurrentValueAndUpdateBest();
            if (externalState != null)
            {
                externalState.SolutionValue = currentValue;
                Array.Copy(BestSolutionSoFar, externalState.BestSolutionSoFar, problemParameters.dimension);
            }
            return currentValue;
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

        private double GetScoreForReplacedDimension(int i, double x)
        {
            var initialValue = currentSolution[i];
            currentSolution[i] = x;
            var result = problemParameters.scoreFunction(currentSolution);
            currentSolution[i] = initialValue;
            return result;
        }

        private double GetScoreForJump(double jumpLength)
        {
            ApplyJump(jumpLength);
            return problemParameters.scoreFunction(currentSolution);
        }

        private void FindGradientForDimension(int i, double smallIncrement)
        {
            var a = currentSolution[i];
            var b = a + smallIncrement;
            var reversingMultiplier = 1.0;
            if (b > problemParameters.solutionRange[i][1])
            {
                var tmp = a;
                a = b;
                b = tmp;
                reversingMultiplier = -1;
            }
            direction[i] = reversingMultiplier * (GetScoreForReplacedDimension(i, b) - GetScoreForReplacedDimension(i, a)) / (b - a);
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

        private void FindJumpLength(bool moreAccurateJumpLengths)
        {
            var rangeBegin = 0.0;
            var rangeEnd = problemParameters.MaxJump * diagonalLength;

            var iterationsLeft = moreAccurateJumpLengths
                ? problemParameters.jumpLengthIterationsFinal
                : problemParameters.jumpLengthIterationsInitial;
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
                GetCurrentValueAndUpdateBest();
            }
        }
    }
}
