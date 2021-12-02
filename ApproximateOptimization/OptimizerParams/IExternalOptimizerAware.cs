namespace ApproximateOptimization
{
    public interface IExternalOptimizerAware
    {
        ExternallyInjectedOptimizerState externalOptimizerState { get; set; }
    }
}
