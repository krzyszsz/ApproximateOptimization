using NUnit.Framework;
using System;

namespace ApproximateOptimization.Tests
{
    public class OptimizerWithRangeDiscoveryTests
    {
        private IOptimizer GetSut(
            Func<double[], double> scoreFunc,
            long maxIterations = 500,
            double temperatureMultiplier = 0.98,
            double initialTemperature = 3.0)
        {
            return new ConcreteOptimizerWithRangeDiscovery(new ConcreteOptimizerWithRangeDiscoveryParams
            {
                MaxAttempts = 50,
                Dimension = 2,
                OptimizerFactoryMethod = (solutionRange) =>
                {
                    return new SimulatedAnnealingOptimizer(
                    new SimulatedAnnealingOptimizerParams
                    {
                        Dimension = 2,
                        RandomSeed = 0,
                        ScoreFunction = scoreFunc,
                        SolutionRange = solutionRange,
                        MaxIterations = maxIterations,
                        TemperatureMultiplier = temperatureMultiplier,
                        InitialTemperature = initialTemperature
                    });
                },
            });
        }

        [Test]
        public void FindsGoodSolutionForLinearFunctionGrowingInBothDimentions()
        {
            Func<double[], double> func = (double[] vector) => vector[0] + vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.GreaterThan(115));
            Assert.That(sut.BestSolutionSoFar[1], Is.GreaterThan(115));
            Assert.That(sut.SolutionValue, Is.GreaterThan(115));
        }

        [Test]
        public void FindsGoodSolutionForLinearFuncDecreasingInBothDimensions()
        {
            Func<double[], double> func = (double[] vector) => -vector[0] - vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.LessThanOrEqualTo(-115));
            Assert.That(sut.BestSolutionSoFar[1], Is.LessThanOrEqualTo(-115));
            Assert.That(sut.SolutionValue, Is.GreaterThanOrEqualTo(+115.0*2));
        }

        [Test]
        public void FindsGoodSolutionForSinCos()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0]/100 * (2 * Math.PI)) + Math.Cos((vector[1]/123 - 0.4) * (2 * Math.PI));
            var sut = GetSut(func);
            double expectedX = 0.25*100;
            double expectedY = 0.4*123;
            double expectedBestValue = 2;

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.SolutionFound, Is.EqualTo(true));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(0.01));
        }

        public void FindsGoodSolutionForSinCosWidened()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0] * (2 * Math.PI) / 5) + Math.Cos((vector[1] / 6 - 0.4) * (2 * Math.PI));
            var sut = GetSut(func);
            double expectedX = 0.25 * 5;
            double expectedY = 0.4 * 6;
            double expectedBestValue = 2;

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.SolutionFound, Is.EqualTo(true));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(0.01));
        }

        [Test]
        public void FindsGoodSolutionAcrossWholeRange()
        {
            Random random = new Random(0);
            for (int i = 0; i < 100; i++)
            {
                double expectedX = random.NextDouble() * 10 - 5;
                double expectedY = random.NextDouble() * 10 - 5;
                double expectedBestValue = random.NextDouble();
                Func<double[], double> func = (double[] vector) =>
                    -Math.Pow(vector[0] - expectedX, 2) - Math.Pow(vector[1] - expectedY, 2) + expectedBestValue;
                var sut = GetSut(
                    func,
                    maxIterations: 1000,
                    temperatureMultiplier: 0.99,
                    initialTemperature: 3);

                sut.FindMaximum();

                Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
                Assert.That(sut.SolutionFound, Is.EqualTo(true));
                Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
                Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
                Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(0.01));
            }
        }

        [Test]
        public void FindsGlobalMaximumIgnoringLocalMaximum()
        {
            Random random = new Random(0);
            for (int i = 0; i < 100; i++)
            {
                double expectedX = random.NextDouble();
                double expectedY = random.NextDouble();
                double expectedLocalX = random.NextDouble();
                double expectedLocalY = random.NextDouble();
                Func<double[], double> func = (double[] vector) =>
                    -Math.Pow(vector[0] - expectedX, 2) - Math.Pow(vector[1] - expectedY, 2)
                    + ((Math.Abs(vector[0] - expectedX) < 0.5 && Math.Abs(vector[1] - expectedY) < 0.5)
                    ? 0
                    : -Math.Pow(vector[0] - expectedLocalX, 2) / 2 - Math.Pow(vector[1] - expectedLocalY, 2) / 2);
                var sut = GetSut(
                    func,
                    maxIterations: 1000,
                    temperatureMultiplier: 0.99,
                    initialTemperature: 3);

                sut.FindMaximum();

                Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
                Assert.That(sut.SolutionFound, Is.EqualTo(true));
                Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
                Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
            }
        }
    }
}