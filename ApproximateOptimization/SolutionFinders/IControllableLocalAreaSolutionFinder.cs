namespace ApproximateOptimization
{

    public interface IControllableLocalAreaSolutionFinder : IControllableSolutionFinder
    {
        double LocalArea { get; set; }
    }
}
