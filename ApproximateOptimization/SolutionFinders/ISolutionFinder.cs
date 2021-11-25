using System;

namespace ApproximateOptimization
{
    public interface ISolutionFinder
    {
        /// <summary>
        /// Returns the solution with the best valuation.
        /// Solution is a vector of dimension given as the first argument where all numbers are in the range 0..1.
        /// </summary>
        /// <param name="dimension">The dimension </param>
        /// <param name="getValue">A func that for each solution provides number indicating how good the solution is.
        /// The higher the number, the better the solution.</param>
        /// <param name="timeLimit">Time limit after which the algorithm will not attempt any next iterations.</param>
        /// <param name="maxIterations">Algorithm stops after given number of iterations. Argument ignored when negative.</param>
        /// <param name="solutionRange">Range defined for each dimension. When omtted, all dimensions have assumed range 0..1.</param>
        void FindMaximum(
            int dimension,
            Func<double[], double> getValue,
            TimeSpan timeLimit = default,
            long maxIterations=-1,
            double[][] solutionRange = null);

        double[] BestSolutionSoFar { get; }

        double SolutionValue { get; }

        bool SolutionFound { get;  }
    }
}
