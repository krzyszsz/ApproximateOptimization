namespace ApproximateOptimization
{
    public class ConcreteCompositeParams : CompositeOptimizerParams<SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteCompositeParams(SimulatedAnnealingWithGradientAscentOptimizerParams problemParameters)
        {
            createOptimizer = (int threadId) => new SimulatedAnnealingWithGradientAscentOptimizer<SimulatedAnnealingWithGradientAscentOptimizerParams>(problemParameters);
        }
    }
}
