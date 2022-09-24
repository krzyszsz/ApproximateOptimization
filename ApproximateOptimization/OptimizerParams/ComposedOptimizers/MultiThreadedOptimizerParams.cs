using System;

namespace ApproximateOptimization
{
    public class MultiThreadedOptimizerParams<T> where T: new()
    {
        public Func<long, IOptimizer> CreateOptimizer { get; set; }
        public int ThreadCount { get; set; } = 8;
        public int? Partitions { get; set; } // When not set, this is the same as threadCount and each thread can do one partition.
        public ILogger Logger { get; set; } = ThreadSafeConsoleLogger.Instance;

        public Func<double[], double> ScoreFunction { get; set; }
        public int GAPopulation { get; set; } = 4;
        public int GAChildrenPerSolution { get; set; } = 2;
        public int GAGenerations { get; set; } = 3;

        /// <summary>
        /// By default random generator is initialized with the same seed to make debugging easier.
        /// In some scenarios you may want to change the default and make it more random to make each execution different.
        /// </summary>
        public bool NonRepeatableRandom = false;

        /// <summary>
        /// When set as true, the optimizer will skip points in the areas that have been already checked.
        /// This requires extra memory allocations to maintain points already checked so it is only beneficial
        /// if checking each point is expensive.
        /// 
        /// Taboo list is maintained for all problem partitions and require locking.
        /// </summary>
        public bool TabooSearch = false;

        /// <summary>
        /// By default the same is used for all dimensions (simplification, can be changed in future versions).
        /// </summary>
        public double TabooAreaForAllDimensions = 0.01;

        public virtual void Validate()
        {
            if (CreateOptimizer == null)
            {
                throw new ArgumentException("Missing CreateOptimizer argument.");
            }
            if (ThreadCount < 1)
            {
                throw new ArgumentException("Thread count argument should be at least 1.");
            }
            if (Partitions != null && ThreadCount > Partitions)
            {
                throw new ArgumentException("Number of partitions is lower than number of threads. ");
            }
            if (Logger == null)
            {
                throw new ArgumentException("Missing Logger argument.");
            }
        }
    }
}
