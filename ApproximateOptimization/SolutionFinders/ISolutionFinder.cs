namespace ApproximateOptimization
{

    public interface ISolutionFinder<T> where T : BaseSolutionFinderParams
    {
        /// <summary>
        /// Runs the actual solving algorithm. This may take considerable time.
        /// </summary>
        void FindMaximum();

        /// <summary>
        /// After running the search, the best found solution can be found in this property.
        /// </summary>
        double[] BestSolutionSoFar { get; }

        /// <summary>
        /// A score of the best found solution.
        /// You may need to check this property to establish how good is the solution found in "BestSolutionSoFar".
        /// </summary>
        double SolutionValue { get; }

        /// <summary>
        /// A flag indicating if any solution was found (even one with a bad SolutionValue).
        /// </summary>
        bool SolutionFound { get;  }
    }
}
