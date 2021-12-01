namespace ApproximateOptimization
{
    public class ConcreteCompositeParams : CompositeOptimizerParams<SimulatedAnnealingWithLocalAreaBinarySearchParams>
    {
        public ConcreteCompositeParams(SimulatedAnnealingWithLocalAreaBinarySearchParams problemParameters)
        {
            createSolutionFinder = (int threadId) => new SimulatedAnnealingWithLocalAreaBinarySearch<SimulatedAnnealingWithLocalAreaBinarySearchParams>(problemParameters);
        }
    }
}
