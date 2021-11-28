namespace ApproximateOptimization
{
    public class ConcreteAutoTuningFinder : AutoTuningFinder<
                ConcreteAutoTuningParams, SimulatedAnnealingWithLocalAreaBinarySearchParams>
    {
        public ConcreteAutoTuningFinder(ConcreteAutoTuningParams problemParameters) : base(problemParameters)
        {
        }
    }
}