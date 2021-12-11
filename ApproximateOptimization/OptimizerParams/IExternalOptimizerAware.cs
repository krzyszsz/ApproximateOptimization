namespace ApproximateOptimization
{
    public interface IExternalOptimizerAware
    {
        ExternallyInjectedOptimizerState ExternalOptimizerState { get; set; }
    }
}
