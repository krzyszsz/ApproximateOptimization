using NUnit.Framework;
using System;

namespace ApproximateOptimization.Tests
{
    public class GradientAscentOptimizerTests
    {
        private IOptimizer GetSut(Func<double[], double> func, int iterationsNumber = 20)
        {
            return new GradientAscentOptimizer(new GradientAscentOptimizerParams
            {
                ScoreFunction = func,
                Dimension = 2,
                MaxIterations = iterationsNumber
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

        [TestCase(0.000001, 1)]
        public void FindsGoodSolutionAcrossWholeRange(double precision, int iterationsNumber)
        {
            Random random = new Random(0);
            for (int i = 0; i < 100; i++)
            {
                double expectedX = random.NextDouble();
                double expectedY = random.NextDouble();
                double expectedBestValue = random.NextDouble();
                Func<double[], double> func = (double[] vector) =>
                    -Math.Pow(vector[0] - expectedX, 2) - Math.Pow(vector[1] - expectedY, 2) + expectedBestValue;
                var sut = GetSut(func, iterationsNumber);

                sut.FindMaximum();

                Assert.That(sut.BestSolutionSoFar.Length, Is.EqualTo(2));
                Assert.That(sut.SolutionFound, Is.EqualTo(true));
                Assert.That(sut.BestSolutionSoFar[0], Is.EqualTo(expectedX).Within(precision));
                Assert.That(sut.BestSolutionSoFar[1], Is.EqualTo(expectedY).Within(precision));
                Assert.That(sut.SolutionValue, Is.EqualTo(expectedBestValue).Within(precision));
            }
        }
    }

}