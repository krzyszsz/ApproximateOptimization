namespace ApproximateOptimization
{
    public interface IExternalOptimazerAware
    {
        ExternallyInjectedOptimizerState externalOptimizerState { get; set; }
    }
}
