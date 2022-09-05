using System;

namespace ApproximateOptimization
{
    public class EasyOptimizerParams : CoreOptimizerParams
    {
        public int Threads { get; set; } = Environment.ProcessorCount;

        public double RequiredPrecision { get; set; } = 0.01;
        public long MaxIterations { get; set; } = -1;

        public long InitialIterations { get; set; } = 130;
        public double IterationsScaler { get; set; } = 3.0;

        /// <summary>
        /// After how many iterations it should start gradient ascent.
        /// </summary>
        public long SwitchingFreq { get; set; } = 120;

        public bool GAEnabled { get; set; } = true;
        public int GAPeriod { get; set; } = 20; // Every 20 iterations
        public int GAPopulation { get; set; } = 4; // we get 4 best solutions to create cross-overs of them (this will generate 8 new iterations - each with random other x2)
        public int GAChildrenPerSolution { get; set; } = 2;

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
