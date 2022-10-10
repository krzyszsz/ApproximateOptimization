using System;

namespace ApproximateOptimization
{
    public class EasyOptimizerParams : CoreOptimizerParams
    {
        public int Threads { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// This is by default same as the number of threads but can be higher (extra partitions will be queued and executed in first free thread).
        /// </summary>
        public int? Partitions { get; set; } = null;

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
        public int GaGenerations { get; set; } = 3;

        private const double ImpossiblePrecision = 0.000_000_000_001;

        /// <summary>
        /// When set as true, the optimizer will skip points in the areas that have been already checked.
        /// This requires extra memory allocations to maintain points already checked so it is only beneficial
        /// if checking each point is expensive.
        /// 
        /// Taboo list is maintained for all problem partitions and require locking.
        /// </summary>
        public bool TabooSearch = true;

        /// <summary>
        /// By default the same is used for all dimensions (simplification, can be changed in future versions).
        /// </summary>
        public double TabooAreaForAllDimensions = 0.001;


        public override void Validate()
        {
            base.Validate();
            if (RequiredPrecision < ImpossiblePrecision)
            {
                throw new ArgumentException(
                    $"Incorrect RequiredPrecision. Expected more than: {ImpossiblePrecision} but got first dimension: {RequiredPrecision}");
            }

            if (TabooAreaForAllDimensions <= 0)
            {
                throw new ArgumentException(
                    $"Incorrect TabooAreaForAllDimensions. Should be positive but got: {TabooAreaForAllDimensions}");
            }
        }
    }
}
