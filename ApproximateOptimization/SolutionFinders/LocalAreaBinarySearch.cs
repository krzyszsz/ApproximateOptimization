using System;

namespace ApproximateOptimization
{

    /// <summary>
    /// This search improves solution on each dimension independently of the other dimensions.
    /// The improvement is done locally, that is only in a small space around the current solution.
    /// The algorithm uses bi-section to find the maximum faster but it only works well for cases with single maximum
    /// in the checked area with the assumption that the function is monotonic on the left of the max and is monotonic on the right
    /// (it will find sub-optimal solution when there are two local maximums for example).
    /// For that reason, the best results will be achieved for smaller areas (which should contain fewer local maximums).
    /// It may give no improvement at all for certain functions or it may start improving hugely when the localArea is small enough
    /// to only fit one maximum with two monotonic sub-ranges.
    /// 
    /// Another limitation is that it searches each dimension independently so even if the correct max
    /// is found in one dimension, it may not be the global maximum.
    /// To make is sligtly better, the process is iterative - all dimentions are searched "iterationCount" times.
    /// 
    /// LocalArea is exposed as public property so that this algorithm can be used in combination with others
    /// and the LocalArea can be changed between calls to "NextSolution".
    /// </summary>
    public class LocalAreaBinarySearch : BaseSolutionFinder, IControllableLocalAreaSolutionFinder
    {
        private readonly int iterationCount;
        private readonly int iterationsPerDimension;
        private double localArea;
        private bool initializeSolution;

        public LocalAreaBinarySearch(double localArea = 1.0, int iterationCount = 3, int iterationsPerDimension = 10, bool initializeSolution = true)
        {
            this.localArea = localArea;
            this.iterationCount = iterationCount;
            this.iterationsPerDimension = iterationsPerDimension;
            this.initializeSolution = initializeSolution;
        }

        double IControllableLocalAreaSolutionFinder.LocalArea
        {
            get
            {
                return localArea;
            }
            set
            {
                localArea = value;
            }
        }

        double[] IControllableLocalAreaSolutionFinder.CurrentSolution
        {
            get
            {
                return currentSolution;
            }
            
            set
            {
                currentSolution = value;
            }
        }

        double[] IControllableLocalAreaSolutionFinder.BestSolutionSoFar
        {
            get
            {
                return BestSolutionSoFar;
            }

            set
            {
                BestSolutionSoFar = value;
            }
        }

        int IControllableLocalAreaSolutionFinder.Dimension
        {
            get
            {
                return this.dimension;
            }

            set
            {
                dimension = value;
            }
        }

        double IControllableLocalAreaSolutionFinder.SolutionValue
        {
            get
            {
                return SolutionValue;
            }

            set
            {
                SolutionValue = value;
            }
        }

        Func<double[], double> IControllableLocalAreaSolutionFinder.ScoreFunction
        {
            get
            {
                return this.getValue;
            }

            set
            {
                getValue = value;
            }
        }

        protected override void SetInitialSolution()
        {
            if (this.initializeSolution)
            {
                base.SetInitialSolution();
                for (int i = 0; i < dimension; i++)
                {
                    currentSolution[i] = 0.5;
                }
            }
        }

        void IControllableLocalAreaSolutionFinder.NextSolution()
        {
            NextSolution();
        }

        protected override void NextSolution()
        {
            if (initializeSolution)
            {
                Array.Copy(BestSolutionSoFar, currentSolution, dimension);
            }
            for (int x=0; x<iterationCount; x++) for (int i=0; i<dimension; i++)
            {
                OptimizeInSingleDimension(i);
            }
        }

        private double GetValueWithDimensionReplaced(int dimension, double value)
        {
            var originalValue = BestSolutionSoFar[dimension];
            currentSolution[dimension] = value;
            var result = this.getValue(currentSolution);
            return result;
        }

        private void OptimizeInSingleDimension(int dimension)
        {
            var rangeBegin = Math.Max(0.0, currentSolution[dimension] - localArea);
            var rangeEnd = Math.Min(1.0, currentSolution[dimension] + localArea);
            
            var iterationsLeft = iterationsPerDimension;
            var bestValue = SolutionValue;
            var bestX = BestSolutionSoFar[dimension];

            while (iterationsLeft-- > 0)
            {
                var mid = (rangeBegin + rangeEnd) / 2;
                var justAboveMid = mid + 0.00001 * (1 - mid);
                var justBelowMid = mid - 0.00001 * mid;
                if (justAboveMid == justBelowMid) break;

                var justAboveMidValue = GetValueWithDimensionReplaced(dimension, justAboveMid);
                var justBelowMidValue = GetValueWithDimensionReplaced(dimension, justBelowMid);

                UpdateBests(ref bestValue, ref bestX, justAboveMidValue, justAboveMid);
                UpdateBests(ref bestValue, ref bestX, justBelowMidValue, justBelowMid);

                if (justBelowMidValue < justAboveMidValue)
                {
                    rangeBegin = justBelowMid;
                } else if (justBelowMidValue > justAboveMidValue)
                {
                    rangeEnd = justAboveMid;
                } else
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
