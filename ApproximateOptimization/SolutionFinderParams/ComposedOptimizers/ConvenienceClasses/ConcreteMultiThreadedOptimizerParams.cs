﻿namespace ApproximateOptimization
{
    /// <summary>
    /// Convenience class with all generic arguments provided.
    /// </summary>
    public class ConcreteMultiThreadedOptimizerParams : MultiThreadedOptimizerParams<SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteMultiThreadedOptimizerParams(SimulatedAnnealingWithGradientAscentOptimizerParams problemParameters)
        {
            createOptimizer = (int threadNumber) => new SimulatedAnnealingWithGradientAscentOptimizer<SimulatedAnnealingWithGradientAscentOptimizerParams>(problemParameters);
        }
    }
}