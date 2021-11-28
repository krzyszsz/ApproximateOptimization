using System;

namespace ApproximateOptimization
{
    public class SimulatedAnnealingWithLocalAreaBinarySearchParams : SimulatedAnnealingParams
    {
        public bool localBinarySearchEnabled { get; set; } = false;
        public bool gradientOptimizerEnabled { get; set; } = true;
        public double localAreaMultiplier { get; set; } = 0.2;
        public int binarySearchIterationCount { get; set; } = 3;
        public int binarySearchIterationsPerDimension { get; set; } = 10;
        public int maxIterationsGradientSearch { get; set; } = 20;
        public int maxIterationsGradientJumps { get; set; } = 20;

        public override void ValidateArguments()
        {
            base.ValidateArguments();
            if (!localBinarySearchEnabled && !gradientOptimizerEnabled)
            {
                throw new ArgumentException("Both types of optimizers are disabled: gradientOptimizerEnabled && localBinarySearchEnabled. Please use standard SimulatedAnnealing instead.");
            }
            if (localAreaMultiplier <= 0 || localAreaMultiplier >= 1)
            {
                throw new ArgumentException("localAreaMultiplier multiplier should be a number greater than 0 and less than 1.");
            }
        }
    }
}
