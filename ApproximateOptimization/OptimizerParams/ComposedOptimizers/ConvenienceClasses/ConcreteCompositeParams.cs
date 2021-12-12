namespace ApproximateOptimization
{
    public class ConcreteCompositeParams : CompositeOptimizerParams<SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteCompositeParams(SimulatedAnnealingWithGradientAscentOptimizerParams problemParameters)
        {
            CreateOptimizer = (int threadId) => new SimulatedAnnealingWithGradientAscentOptimizer(problemParameters);
        }
    }
}
