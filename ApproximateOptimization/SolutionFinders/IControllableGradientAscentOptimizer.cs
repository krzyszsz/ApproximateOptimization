namespace ApproximateOptimization
{
    public interface IControllableGradientAscentOptimizer : IControllableSolutionFinder
    {
        /// <summary>
        /// 1.0 means a vector length across the whole range (all dimensions).
        /// </summary>
        double MaxJump { get; set; }
    }
}
