namespace ApproximateOptimization
{
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

        // TODO - autoTuning
    }
}
