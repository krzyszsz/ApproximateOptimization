using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// This search improves solution on each dimension independently of the other dimensions.
    /// The improvement is done locally, that is only in a small space around the current solution.
    /// The algorithm uses binary search to find the maximum faster but it only works well for cases with single maximum
    /// in the checked area with the assumption that the function is monotonic on the left of the max and is monotonic on the right
    /// (it will find sub-optimal solution when there are two local maximums for example).
    /// For that reason, the best results will be achieved for smaller areas (which should contain fewer local maximums).
    /// It may give no improvement at all for certain functions or it may start improving hugely when the localArea is small enough
    /// to only fit one maximum with two monotonic sub-ranges.
    /// 
    /// Another limitation is that it searches each dimension independently so even if the correct max
    /// is found in one dimension, it may not be the global maximum.
    /// To make is sligtly better, the process is iterative - all dimentions are searched "iterationCount" times.
    /// </summary>
    public class LocalAreaBinarySearch<T> : BaseSolutionFinder<T> where T : LocalAreaBinarySearchParams
    {
        protected override void NextSolution()
        {
            if (isSelfContained)
            {
                Array.Copy(BestSolutionSoFar, currentSolution, problemParameters.dimension);
            }
            for (int x = 0; x < problemParameters.maxBinarySearchIterations; x++)
                for (int i = 0; i < problemParameters.dimension; i++)
            {
                OptimizeInSingleDimension(i);
            }
            if (isSelfContained)
            {
                UpdateBestSolution();
            }
        }

        private double GetValueWithDimensionReplaced(int dimension, double value)
        {
            var originalValue = BestSolutionSoFar[dimension];
            currentSolution[dimension] = value;
            var result = problemParameters.getValue(currentSolution);
            return result;
        }

        private void OptimizeInSingleDimension(int dimension)
        {
            var rangeWidth = problemParameters.solutionRange[dimension][1] - problemParameters.solutionRange[dimension][0];
            var rangeBegin = Math.Max(problemParameters.solutionRange[dimension][0],
                currentSolution[dimension] - localArea * rangeWidth);
            var rangeEnd = Math.Min(problemParameters.solutionRange[dimension][1],
                currentSolution[dimension] + localArea * rangeWidth);

            var iterationsLeft = iterationsPerDimension;
            var bestValue = SolutionValue;
            var bestX = BestSolutionSoFar[dimension];

            while (iterationsLeft-- > 0)
            {
                var mid = (rangeBegin + rangeEnd) / 2;
                rangeWidth = rangeEnd - rangeBegin;
                var smallIncrement = 0.00001 * rangeWidth;
                var justAboveMid = mid + smallIncrement;
                var justBelowMid = mid - smallIncrement;
                if (justAboveMid == justBelowMid) break;

                var justAboveMidValue = GetValueWithDimensionReplaced(dimension, justAboveMid);
                var justBelowMidValue = GetValueWithDimensionReplaced(dimension, justBelowMid);

                UpdateBests(ref bestValue, ref bestX, justAboveMidValue, justAboveMid);
                UpdateBests(ref bestValue, ref bestX, justBelowMidValue, justBelowMid);

                if (justBelowMidValue < justAboveMidValue)
                {
                    rangeBegin = justBelowMid;
                }
                else if (justBelowMidValue > justAboveMidValue)
                {
                    rangeEnd = justAboveMid;
                }
                else
                {
                    break;
                }
            }
            currentSolution[dimension] = bestX;
        }

        void UpdateBests(ref double bestValue, ref double bestX, double otherValue, double otherX)
        {
            if (otherValue > bestValue)
            {
                bestValue = otherValue;
                bestX = otherX;
            }
        }
    }
}
