using System;

namespace ApproximateOptimization
{
    public class CompositeOptimizerParams<T> : MultiThreadedOptimizerParams<T> where T : BaseSolutionFinderParams, new()
    {
        public T ActualOptimizerParams { get; set; } = new T();

        public override void Validate()
        {
            base.Validate();
            if (ActualOptimizerParams != null)
            {
                ActualOptimizerParams.Validate();
            }
            else
            {
                throw new ArgumentException("Missing argument ActualOptimizerParams");
            }
        }
    }
}
