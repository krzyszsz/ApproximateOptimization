using System;

namespace ApproximateOptimization
{
    public class AutoTuningParams<P> : BaseSolutionFinderParams where P : BaseSolutionFinderParams
    {
        public Func<ISolutionFinder<P>> solutionFinderFactoryMethod { get; set; }
        public int maxAttempts { get; set; } = 50;
    }
}
