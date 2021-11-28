using System;

namespace ApproximateOptimization
{
    public class CompositeOptimizerParams<T> : MultiThreadedOptimizerParams<T> where T : BaseSolutionFinderParams, new()
    { }

    public class ConcreteCompositeParams : CompositeOptimizerParams<SimulatedAnnealingWithLocalAreaBinarySearchParams>
    {
        public ConcreteCompositeParams(SimulatedAnnealingWithLocalAreaBinarySearchParams problemParameters)
        {
            createSolutionFinder = (int threadId) => new SimulatedAnnealingWithLocalAreaBinarySearch<SimulatedAnnealingWithLocalAreaBinarySearchParams>(problemParameters);
        }
    }
}
