namespace ApproximateOptimization
{
    /// <summary>
    /// Convenience class creating the 2 most useful optimizers without the need of providing generic arguments.
    /// </summary>
    public static class OptimizerFactory
    {
        /// <summary>
        /// Returns an optimizer employing MultithreadedOptimizer to run SimulatedAnnealingWithLocalAreaBinarySearch.
        /// </summary>
        public static ConcreteCompositeOptimizer GetCompositeOptiizer(
            SimulatedAnnealingWithLocalAreaBinarySearchParams optimizerParams, int threads = 8)
        {
            return new ConcreteCompositeOptimizer(
                new ConcreteMuiltiThreadedOptimizerParams(
                optimizerParams)
                {
                    threadCount = threads
                });
        }

        /// <summary>
        /// Returns an optimizer automatically re-adjuststing the range for each dimension.
        /// It runs the "internal" optimizer multiple times until results for all dimensions fall inside
        /// of the search range (or the number of iterations exceeds the configured maximum).
        /// </summary>
        public static ConcreteAutoTuningFinder GetAutoScaledCompositeOptmizer(
            SimulatedAnnealingWithLocalAreaBinarySearchParams optimizerParams,
            int maxReadjustments = 50,
            int threads = 8
            )
        {
            return new ConcreteAutoTuningFinder(new ConcreteAutoTuningParams
            {
                maxAttempts = maxReadjustments,
                dimension = optimizerParams.dimension,
                solutionFinderFactoryMethod = (solutionRange) =>
                {
                    optimizerParams.solutionRange = solutionRange;
                    return new ConcreteCompositeOptimizer(
                    new ConcreteMuiltiThreadedOptimizerParams(optimizerParams)
                    {
                        threadCount = threads,
                    });
                }
            });
        }
    }
}
