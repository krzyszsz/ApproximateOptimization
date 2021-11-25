using NUnit.Framework;
using System;

namespace ApproximateOptimization.Tests
{
    public class AutoTuningTests
    {
        private AutoTuningFinder GetSut()
        {
            return new AutoTuningFinder(() => new SimulatedAnnealing(0.99), 2);
        }

        [Test]
        public void FindsGoodSolutionForLinearFunctionGrowingInBothDimentions()
        {
            Func<double[], double> func = (double[] vector) => vector[0] + vector[1];
            var sut = GetSut();

            sut.FindMaximum(2, func, default(TimeSpan), 100);

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.GreaterThan(115));
            Assert.That(sut.BestSolutionSoFar[1], Is.GreaterThan(115));
            Assert.That(sut.SolutionValue, Is.GreaterThan(115));
        }

        [Test]
        public void FindsGoodSolutionForLinearFuncDecreasingInBothDimensions()
        {
            Func<double[], double> func = (double[] vector) => -vector[0] - vector[1];
            var sut = GetSut();

            sut.FindMaximum(2, func, default(TimeSpan), 100);

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.BestSolutionSoFar[0], Is.LessThanOrEqualTo(-115));
            Assert.That(sut.BestSolutionSoFar[1], Is.LessThanOrEqualTo(-115));
            Assert.That(sut.SolutionValue, Is.GreaterThanOrEqualTo(+115.0*2));
        }

        [Test]
        public void FindsGoodSolutionForSinCos()
        {
            Func<double[], double> func = (double[] vector) =>
                Math.Sin(vector[0] * (2 * Math.PI)) + Math.Cos((vector[1] - 0.4) * (2 * Math.PI));
            var sut = GetSut();
            double expectedX = 0.25;
            double expectedY = 0.4;
            double expectedBestValue = 2;

            sut.FindMaximum(2, func, default(TimeSpan), 1000);

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
            var sut = GetSut();
            double expectedX = 0.25 * 5;
            double expectedY = 0.4 * 6;
            double expectedBestValue = 2;

            sut.FindMaximum(2, func, default(TimeSpan), 1000);

            Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
            Assert.That(sut.SolutionFound, Is.EqualTo(true));
            Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(0.01));
            Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(0.01));
            Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(0.01));
        }
    }
}