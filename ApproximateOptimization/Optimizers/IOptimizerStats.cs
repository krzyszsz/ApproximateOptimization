using System;

namespace ApproximateOptimization
{
    public interface IOptimizerStats
    {
        TimeSpan ElapsedTime { get; }

        long IterationsExecuted { get; }

        double LocalAreaAtTheEnd { get; }
    }
}
