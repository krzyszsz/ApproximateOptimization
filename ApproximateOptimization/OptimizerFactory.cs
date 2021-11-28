namespace ApproximateOptimization
{
    public static class OptimizerFactory
    {
        /// <summary>
        /// Returns an optimizer employing MultithreadedOptimizer to run SimulatedAnnealingWithLocalAreaBinarySearch.
        /// </summary>
        public static NonGenericCompositeOptimizer GetCompositeOptiizer(CompositeParams optimizerParams)
        {
            return new NonGenericCompositeOptimizer(optimizerParams);
        }

        // TODO - autoTuning
    }
}
