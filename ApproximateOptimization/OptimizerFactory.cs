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
        /// <param name="rangeDiscoveryMaxAttempts">Maximum number of times the optimizer will be run
        /// with widening solution range. Parameter only works when rangeDiscovery is passed as true.</param>
        /// <returns>An optimizer employing MultithreadedOptimizer to run SimulatedAnnealingWithGradientAscent</returns>
        public static IOptimizer GetCompositeOptimizer(
            MultiStrategyOptimizerParams optimizerParams, int threads = 8, bool rangeDiscovery = false, int rangeDiscoveryMaxAttempts=50)
        {
            if (!rangeDiscovery)
            {
                return new ConcreteCompositeOptimizer(
                    new ConcreteMultiThreadedOptimizerParams(
                    optimizerParams)
                    {
                        ThreadCount = threads
                    });
            }
            return
                new ConcreteOptimizerWithRangeDiscovery(new ConcreteOptimizerWithRangeDiscoveryParams
            {
                MaxAttempts = rangeDiscoveryMaxAttempts,
                Dimension = optimizerParams.Dimension,
                OptimizerFactoryMethod = (solutionRange) =>
                {
                    optimizerParams.SolutionRange = solutionRange;
                    return new ConcreteCompositeOptimizer(
                    new ConcreteMultiThreadedOptimizerParams(optimizerParams)
                    {
                        ThreadCount = threads,
                    });
                }
            });
        }

        /// <summary>
        /// Factory method creating an easy to use optimazer automatically adjusting number of iterations to achieve requested precision.
        /// </summary>
        /// <param name="optimizerParams">Problem definition.</param>
        /// <param name="threads">Number of threads to run.</param>
        /// <returns>Simple optimizer automatically adjusting number of iterations to achieve requested precision</returns>
        public static IOptimizer GetEasyOptimizer(
            EasyOptimizerParams optimizerParams)
        {
            return new EasyOptimizer(optimizerParams);
        }
    }
}
