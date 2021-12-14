namespace ApproximateOptimization
{
    public class ConcreteCompositeParams : CompositeOptimizerParams<SimulatedAnnealingWithGradientAscentOptimizerParams>
    {
        public ConcreteCompositeParams(SimulatedAnnealingWithGradientAscentOptimizerParams problemParameters)
        {
            CreateOptimizer = (int threadNumber) =>
            {
                var newProblemParameters = problemParameters.ShallowClone();
                newProblemParameters.RandomSeed = threadNumber;
                return new SimulatedAnnealingWithGradientAscentOptimizer(newProblemParameters);
            };
        }
    }
}
