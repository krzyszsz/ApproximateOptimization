using System;
using System.Linq;
using System.Threading;

namespace ApproximateOptimization
{
    public class BaseOptimizerParams
    {
        /// <summary>
        /// Dimension is the size of array that is a solution of the problem.
        /// </summary>
        public int Dimension { get; set; }

        /// <summary>
        /// The function which parametrized with an array of size "dimension" returns
        /// one number indicating how good the solution is, in relation to other possible solutions.
        /// The higher the returned number, the more preferable it is.
        /// 
        /// By providing this function, you actually define the problem to be found by this optimizer
        /// which will attempt to find the array of numbers with the highest score.
        /// 
        /// This function should be possibly fast because it will be called multiple times
        /// It should also be a pure function (give consistent results and have no state).
        /// It should not throw exceptions for any array of numbers with the size "dimension"
        /// and it should not modify the input array. 
        /// </summary>
        public Func<double[], double> ScoreFunction { get; set; }

        /// <summary>
        /// Time limit: one of the possible conditions to stop looking for solutions.
        /// </summary>
        public TimeSpan TimeLimit { get; set; } = default(TimeSpan);

        /// <summary>
        /// Maximum number of iterations: one of the possible conditions to stop looking for solutions.
        /// </summary>
        public long MaxIterations { get; set; } = -1;

        /// <summary>
        /// CancellationToken passed into optimizer to better control its lifetime optimizer.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = default(CancellationToken);

        /// <summary>
        /// Range defined for each dimension. When omitted, all dimensions have assumed range 0..1.
        /// </summary>
        public double[][] SolutionRange { get; set; } = null;

        public virtual void Validate()
        {
            if (MaxIterations == -1 && TimeLimit == default(TimeSpan) && CancellationToken == default(CancellationToken))
            {
                throw new ArgumentException("Missing timeLimit or maxIterations argument. Without them the algorithm would never stop!");
            }
            if (SolutionRange == null)
            {
                throw new ArgumentException(
                    $"Solution range argument is missing");
            }
            if (SolutionRange != null && SolutionRange.Length != Dimension)
            {
                throw new ArgumentException(
                    $"Incorrect range dimension. Expected: {Dimension}x2 but got first dimension: {SolutionRange.Length}");
            }
            if (SolutionRange != null && SolutionRange.Any(x => x.Length != 2))
            {
                throw new ArgumentException(
                    $"Incorrect solution range size. Expected: {Dimension}x2 but got second dimension: {SolutionRange.First(x => x.Length != 2).Length}");
            }
        }
    }
}
