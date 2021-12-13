namespace ApproximateOptimization
{
    public class ConcreteCompositeParams : CompositeOptimizerParams<SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteCompositeParams(SimulatedAnnealingWithGradientAscentOptimizerParams problemParameters)
        {
            CreateOptimizer = (int threadNumber) =>
            {
                problemParameters.RandomSeed = threadNumber;
                return new SimulatedAnnealingWithGradientAscentOptimizer(problemParameters);
            };
        }
    }
}
