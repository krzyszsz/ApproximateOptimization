using System;

namespace ApproximateOptimization
{
    public class EasyOptimizerParams : CoreOptimizerParams
    {
        public int Threads { get; set; } = Environment.ProcessorCount;

        public double RequiredPrecision { get; set; } = 0.01;

        private const double ImpossiblePrecision = 0.000_000_000_001;

        public override void Validate()
        {
            base.Validate();
            if (RequiredPrecision < ImpossiblePrecision)
            {
                throw new ArgumentException(
                    $"Incorrect RequiredPrecision. Expected more than: {ImpossiblePrecision} but got first dimension: {RequiredPrecision}");
            }
        }
    }
}
