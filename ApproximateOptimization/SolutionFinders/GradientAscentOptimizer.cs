using System;
using System.Linq;

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
        const double delta = 0.00001; // Any number below 0.5 could work?

        private readonly bool isSelfContained;
        private readonly int iterationCount;
        private double[] direction;
        private double diagonalLength;

        public GradientAscentOptimizer(bool initializeSolution=true, int iterationCount=30, int jumpLengthIterations=10)
        {
            this.isSelfContained = initializeSolution;
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

        protected override void Initialize()
        {
            direction = new double[dimension];
            if (isSelfContained)
            {
                base.Initialize();
                MaxJump = 1.0;
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
            if (isSelfContained)
            {
                Array.Copy(BestSolutionSoFar, currentSolution, dimension);
            }
            for (int i = 0; i < iterationCount; i++)
            {
                var smallIncrement = MaxJump * delta * diagonalLength;
                FindDirection(smallIncrement);
                FindJumpLength();
                // TODO: Break when no improvement!
            }
            if (isSelfContained)
            {
                UpdateBestSolution();
            }
        }

        private void ApplyJump(double jumpLength)
        {
            for (int i = 0; i < dimension; i++)
            {
                currentSolution[i] = BestSolutionSoFar[i] + direction[i] * jumpLength;
                currentSolution[i] = Math.Max(solutionRange[i][0], Math.Min(solutionRange[i][1], currentSolution[i]));
            }
        }

        private double GetValueForReplacedDimension(int i, double x)
        {
            var initialValue = currentSolution[i];
            currentSolution[i] = x;
            var result = getValue(currentSolution);
            currentSolution[i] = initialValue;
            return result;
        }

        private double GetValueForJump(double jumpLength)
        {
            ApplyJump(jumpLength);
            return getValue(currentSolution);
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
            direction[i] = (GetValueForReplacedDimension(i, b) - GetValueForReplacedDimension(i, a)) / (b - a);
        }

        private double GetVectorLength(double[] vector)
        {
            return Math.Sqrt(vector.Sum(x => x * x));
        }



        private void FindDirection(double smallIncrement)
        {
            for (int i = 0; i< dimension; i++)
            {
                FindGradientForDimension(i, smallIncrement);
            }
            var vectorLength = GetVectorLength(direction);
            for (int i = 0; i < dimension; i++)
            {
                direction[i] = direction[i] / vectorLength;
            }

            // Below could be better (diagonalLength could be smaller) but it would be more complex to find it
            // so the distance to the "corner" is good enough.
            diagonalLength = 0;
            for (int i = 0; i < dimension; i++)
            {
                var distance = BestSolutionSoFar[i] -
                    (
                        direction[i] >= 0 ? solutionRange[i][1] : solutionRange[i][1]
                    );
                diagonalLength += distance * distance;
            }
            diagonalLength = Math.Sqrt(diagonalLength);
        }

        private void FindJumpLength()
        {
            var rangeBegin = 0.0;
            var rangeEnd = MaxJump * diagonalLength;

            var iterationsLeft = iterationCount;
            var bestJumpLength = 0.0;
            var valueForBestJumpLength = SolutionValue;

            while (iterationsLeft-- > 0)
            {
                var mid = (rangeBegin + rangeEnd) * 0.5;
                var rangeWidth = rangeEnd - rangeBegin;
                var smallIncrement = delta * rangeWidth;
                var justAboveMid = mid + smallIncrement;
                var justBelowMid = mid - smallIncrement;
                if (justAboveMid == justBelowMid) break;

                var justAboveMidValue = GetValueForJump(justAboveMid);
                var justBelowMidValue = GetValueForJump(justBelowMid);

                ApplyJump(mid);
                var currentValue = getValue(currentSolution);
                if (currentValue > valueForBestJumpLength)
                {
                    bestJumpLength = mid;
                }

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
            if (valueForBestJumpLength > SolutionValue)
            {
                ApplyJump(bestJumpLength);
            }
        }
    }
}
