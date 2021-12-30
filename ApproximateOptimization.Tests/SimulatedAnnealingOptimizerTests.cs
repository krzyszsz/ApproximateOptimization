using NUnit.Framework;
using System;

namespace ApproximateOptimization.Tests
{
    public class SimulatedAnnealingOptimizerTests
    {
        private IOptimizer GetSut(
            Func<double[],
                double> func,
                int maxIterations=500,
                double temperatureMultiplier = 0.99,
                double initialTemperature=2.0)
        {
            return new SimulatedAnnealingOptimizer(
            new SimulatedAnnealingOptimizerParams
            {
                ScoreFunction = func,
                Dimension = 2,
                MaxIterations = maxIterations,
                TemperatureMultiplier = temperatureMultiplier,
                InitialTemperature = initialTemperature
            });
        }

        [Test]
        public void FindsGoodSolutionForLinearFunctionGrowingInBothDimentions()
        {
            Func<double[], double> func = (double[] vector) => vector[0] + vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(1).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(1).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(2).Within(0.01));
        }

        [Test]
        public void FindsGoodSolutionForLinearFuncDecreasingInBothDimensions()
        {
            Func<double[], double> func = (double[] vector) => -vector[0] - vector[1];
            var sut = GetSut(func);

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(0).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(0).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(0).Within(0.01));
        }

        [Test]
        public void FindsGoodSolutionForSinCos()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0] * (2 * Math.PI)) + Math.Cos((vector[1] - 0.4) * (2 * Math.PI));
            var sut = GetSut(func);
            double expectedX = 0.25;
            double expectedY = 0.4;
            double expectedBestValue = 2;

            sut.FindMaximum();

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.SolutionFound, Is.EqualTo(true));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(0.01));
        }

        [TestCase(0.0001, 1000, 0.99, 3.0)]
        [TestCase(0.01, 117, 0.95, 5.0)]
        [TestCase(0.02, 100, 0.9, 5.0)]
        public void FindsGoodSolutionAcrossWholeRange(double precision, int maxIterations, double temperatureMultiplier, double initialTemperature)
        {
            Random random = new Random(0);
            for (int i = 0; i < 100; i++)
            {
                double expectedX = random.NextDouble();
                double expectedY = random.NextDouble();
                double expectedBestValue = random.NextDouble();
                Func<double[], double> func = (double[] vector) =>
                    -Math.Pow(vector[0] - expectedX, 2) - Math.Pow(vector[1] - expectedY, 2) + expectedBestValue;
                var sut = GetSut(func, maxIterations, temperatureMultiplier, initialTemperature);

                sut.FindMaximum();

                Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
                Assert.That(sut.SolutionFound, Is.EqualTo(true));
                Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(precision));
                Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(precision));
                Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(precision));
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
                    + ((Math.Abs(vector[0] - expectedX) < 0.1 && Math.Abs(vector[1] - expectedY) < 0.1)
                    ? 0
                    : -Math.Pow(vector[0] - expectedLocalX, 2)/2 - Math.Pow(vector[1] - expectedLocalY, 2)/2);
                var sut = GetSut(func, maxIterations: 1000, temperatureMultiplier: 0.99, initialTemperature: 3.0);

                sut.FindMaximum();

                Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
                Assert.That(sut.SolutionFound, Is.EqualTo(true));
                Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.0001));
                Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.0001));
            }
        }
    }
}