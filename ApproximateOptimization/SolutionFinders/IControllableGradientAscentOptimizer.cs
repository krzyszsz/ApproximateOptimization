namespace ApproximateOptimization
{
    public interface IControllableGradientAscentOptimizer : IControllableSolutionFinder
    {
        double MaxJump { get; set; }
    }
}
