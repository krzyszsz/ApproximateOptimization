namespace ApproximateOptimization
{
    /// <summary>
    /// Convenience class creating the 2 most useful optimizers without the need of providing generic arguments.
    /// </summary>
    public static class OptimizerFactory
    {
        /// <summary>
        /// Main factory method creating the composite of all optimizers defined in this project.
        /// </summary>
        /// <param name="optimizerParams">Problem definition.</param>
        /// <param name="threads">Number of threads to run.</param>
        /// <param name="rangeDiscovery">Range discovery (by default it's disabled).
        /// Please be aware that it may be misleading;
        /// it's better to give the expected range in the beginning because range discovery is very simple
        /// and only widens the range when the maximum is found at the end of the range; 
        /// you may accidentally have a local maximum inside of the range and range discovery
        /// will not trigger range widening, potentially missing global maximum).
        /// </param>
        /// <returns>An optimizer employing MultithreadedOptimizer to run SimulatedAnnealingWithGradientAscent</returns>
        public static IOptimizer GetCompositeOptimizer(
            SimulatedAnnealingWithGradientAscentOptimizerParams optimizerParams, int threads = 8, bool rangeDiscovery = false)
        {
            if (!rangeDiscovery)
            {
                return new ConcreteCompositeOptimizer(
                    new ConcreteMultiThreadedOptimizerParams(
                    optimizerParams)
                    {
                        threadCount = threads
                    });
            }
            return
                new ConcreteOptimizerWithRangeDiscovery(new ConcreteOptimizerWithRangeDiscoveryParams
            {
                maxAttempts = 50,
                dimension = optimizerParams.dimension,
                optimizerFactoryMethod = (solutionRange) =>
                {
                    optimizerParams.solutionRange = solutionRange;
                    return new ConcreteCompositeOptimizer(
                    new ConcreteMultiThreadedOptimizerParams(optimizerParams)
                    {
                        threadCount = threads,
                    });
                }
            });
        }
    }
}
