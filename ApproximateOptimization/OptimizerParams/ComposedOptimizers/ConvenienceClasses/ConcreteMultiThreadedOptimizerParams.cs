namespace ApproximateOptimization
{
    /// <summary>
    /// Convenience class with all generic arguments provided.
    /// </summary>
    public class ConcreteMultiThreadedOptimizerParams : MultiThreadedOptimizerParams<SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteMultiThreadedOptimizerParams(SimulatedAnnealingWithGradientAscentOptimizerParams problemParameters)
        {
            CreateOptimizer = (int threadNumber) =>
            {
                problemParameters.RandomSeed = threadNumber;
                return new SimulatedAnnealingWithGradientAscentOptimizer(problemParameters);
            };
        }
    }
}
