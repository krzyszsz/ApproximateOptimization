using System;

namespace ApproximateOptimization
{
    public class MultiThreadedOptimizerParams<T> where T: new()
    {
        public Func<int, IOptimizer> createOptimizer { get; set; }
        public int threadCount { get; set; } = 8;
        public ILogger logger { get; set; } = ThreadSafeConsoleLogger.Instance;

        public virtual void Validate()
        {
            if (createOptimizer == null)
            {
                throw new ArgumentException("Missing createOptimizer argument.");
            }
            if (threadCount < 1)
            {
                throw new ArgumentException("Thread count argument should be at least 1.");
            }
            if (logger == null)
            {
                throw new ArgumentException("Missing logger argument.");
            }
        }
    }
}
