using System;

namespace ApproximateOptimization
{
    /// <summary>
    /// Runs gradient ascent optimization:
    /// 1. For current point, it finds gradinet in all dimensions (save it as "direction" vector)
    /// 2. Finds the length of the jump along the "direction" using binary search in the range 0..MaxJump
    ///    by attempting "jumpLengthIterations" different lengths.
    /// -> Actions 1&2 are executed "iterationCount" times unless an iteration gives no improvement.
    /// </summary>
    public class GradientAscentOptimizer : BaseSolutionFinder, IControllableGradientAscentOptimizer
    {
        private readonly bool initializeSolution;
        private readonly int iterationCount;
        private readonly double[] direction;

        public GradientAscentOptimizer(bool initializeSolution=true, int iterationCount=30, int jumpLengthIterations=10)
        {
            this.initializeSolution = initializeSolution;
            this.iterationCount = iterationCount;
        }

        public double MaxJump { get; set; }
        double[] IControllableSolutionFinder.CurrentSolution
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

        double[] IControllableSolutionFinder.BestSolutionSoFar
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

        int IControllableSolutionFinder.Dimension
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

        double IControllableSolutionFinder.SolutionValue
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

        Func<double[], double> IControllableSolutionFinder.ScoreFunction
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

        double[][] IControllableSolutionFinder.SolutionRange
        {
            get => solutionRange;
            set => solutionRange = value;
        }

        protected override void SetInitialSolution()
        {
            if (this.initializeSolution)
            {
                base.SetInitialSolution();
                for (int i = 0; i < dimension; i++)
                {
                    var rangeWidth = solutionRange[i][1] - solutionRange[i][0];
                    currentSolution[i] = solutionRange[i][0] + rangeWidth / 2;
                }
            }
        }

        void IControllableSolutionFinder.NextSolution()
        {
            NextSolution();
        }

        protected override void NextSolution()
        {
            if (initializeSolution)
            {
                Array.Copy(BestSolutionSoFar, currentSolution, dimension);
            }
            for (int i = 0; i < iterationCount; i++)
            {
                FindDirection(i);
                FindJumpLength();
            }
        }

        private void ApplyJump(double jumpLength)
        {
            for (int i = 0; i <= dimension; i++)
            {
                currentSolution[i] = BestSolutionSoFar[i] + direction[i] * jumpLength;
            }
        }

        private double getValueWithReplaedPosition(double x, int i)
        {
            var initialValue = currentSolution[i];
            currentSolution[i] = x;
            var result = getValue(currentSolution);
            currentSolution[i] = initialValue;
            return result;
        }

        private void FindGradientForDimension(int i, double smallIncrement)
        {
            var a = currentSolution[i];
            var b = a + smallIncrement;
            if (b > solutionRange[i][i])
            {
                var tmp = a;
                a = b;
                b = tmp;
            }
            direction[i] = (getValueWithReplaedPosition(b, i) - getValueWithReplaedPosition(a, i)) / (b - a);
        }

        private void FindDirection(double smallIncrement)
        {
            for (int i = 0; i<= dimension; i++)
            {
                FindGradientForDimension(i, smallIncrement);
            }
        }

        private void FindJumpLength()
        {
           // unfinished
        }

        //private void OptimizeInSingleDimension(int dimension)
        //{
        //    var rangeWidth = solutionRange[dimension][1] - solutionRange[dimension][0];
        //    var rangeBegin = Math.Max(solutionRange[dimension][0], currentSolution[dimension] - MaxJump * rangeWidth);
        //    var rangeEnd = Math.Min(solutionRange[dimension][1], currentSolution[dimension] + MaxJump * rangeWidth);

        //    var iterationsLeft = iterationsPerDimension;
        //    var bestValue = SolutionValue;
        //    var bestX = BestSolutionSoFar[dimension];

        //    while (iterationsLeft-- > 0)
        //    {
        //        var mid = (rangeBegin + rangeEnd) / 2;
        //        var justAboveMid = mid + 0.00001 * (1 - mid);
        //        var justBelowMid = mid - 0.00001 * mid;
        //        if (justAboveMid == justBelowMid) break;

        //        var justAboveMidValue = GetValueWithDimensionReplaced(dimension, justAboveMid);
        //        var justBelowMidValue = GetValueWithDimensionReplaced(dimension, justBelowMid);

        //        UpdateBests(ref bestValue, ref bestX, justAboveMidValue, justAboveMid);
        //        UpdateBests(ref bestValue, ref bestX, justBelowMidValue, justBelowMid);

        //        if (justBelowMidValue < justAboveMidValue)
        //        {
        //            rangeBegin = justBelowMid;
        //        }
        //        else if (justBelowMidValue > justAboveMidValue)
        //        {
        //            rangeEnd = justAboveMid;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    currentSolution[dimension] = bestX;
        //}

        //void UpdateBests(ref double bestValue, ref double bestX, double otherValue, double otherX)
        //{
        //    if (otherValue > bestValue)
        //    {
        //        bestValue = otherValue;
        //        bestX = otherX;
        //    }
        //}
    }
}
