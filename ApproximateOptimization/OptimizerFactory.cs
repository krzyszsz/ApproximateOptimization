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
            ConcreteMuiltiThreadedOptimizerParams optimizerParams)
        {
            return new ConcreteCompositeOptimizer(optimizerParams);
        }

        /// <summary>
        /// Returns an optimizer automatically re-adjuststing the range for each dimension.
        /// It runs the "internal" optimizer multiple times until results for all dimensions fall inside
        /// of the search range (or the number of iterations exceeds the configured maximum).
        /// </summary>
        public static ConcreteAutoTuningFinder GetAutoSizingCompositeOptmizer(
            ConcreteMuiltiThreadedOptimizerParams optimizerParams,
            int maxReadjustments = 50
            )
        {
            return new ConcreteAutoTuningFinder(new ConcreteAutoTuningParams
            {
                maxAttempts = maxReadjustments,
                solutionFinderFactoryMethod = () => new ConcreteAutoTuningFinder(optimizerParams)
            });
        }
    }
}
