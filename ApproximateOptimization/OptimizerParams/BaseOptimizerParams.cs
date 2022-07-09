using System;
using System.Linq;
using System.Threading;

namespace ApproximateOptimization
{
    public class BaseOptimizerParams : CoreOptimizerParams
    {
        /// <summary>
        /// Maximum number of iterations: one of the possible conditions to stop looking for solutions.
        /// </summary>
        public long MaxIterations { get; set; } = -1;

        public override void Validate()
        {
            base.Validate();
            if (MaxIterations == -1 && TimeLimit == default(TimeSpan) && CancellationToken == default(CancellationToken))
            {
                throw new ArgumentException("Missing timeLimit or maxIterations argument. Without them the algorithm would never stop!");
            }
        }
    }
}
