namespace ApproximateOptimization
{
    public interface ILocalAreaBinarySearch
    {
        void NextSolution();
        double LocalArea { get; set; }
    }
}
