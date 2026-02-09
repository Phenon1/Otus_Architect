using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Tests
{
    public class TestsSolve
    {
        [Test]
        public void Solve_RootEmpty()
        {
            var result = Program.Solve(1d, 0, 1d);
            Assert.That(result, Is.Empty);
        }
        [Test]
        public void Solve_TwoRoots()
        {
            var result =  Program.Solve(1d, 0, -1d);
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result, Is.EquivalentTo(new[] { 1d, -1d }));
        }

        [Test]
        public void Solve_OneRootsTwice()
        {
            var result = Program.Solve(1d, 2d, 1d);
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result, Is.EquivalentTo(new[] { -1d, -1d }));

        }

        [Test]
        public void Solve_ANotZero()
        {
            double eps = 0.001d;
            Assert.Throws<Program.AZeroException>(() => Program.Solve(0, 2d, 1d));
            Assert.Throws<Program.AZeroException>(() => Program.Solve(eps - 0.000001, 2d, 1d, eps));
            Assert.Throws<Program.AZeroException>(() => Program.Solve(-eps + 0.000001, 2d, 1d, eps));
            Assert.DoesNotThrow(() => Program.Solve(eps + 0.000001, 2d, 1d, eps));

        }

        [Test]
        public void Solve_DAroundZero()
        {
            double eps = 0.000001d;
            var result = Program.Solve(0.0001d, 0, 1d,eps);
            Assert.That(result, Is.Empty);

        }
        [Test]
        public void Solve_InvalidArg()
        {
            Assert.Throws<Program.InfinityException>(() => Program.Solve(double.PositiveInfinity, 2d, 1d));
            Assert.Throws<Program.InfinityException>(() => Program.Solve(1d, double.NegativeInfinity, 1d));
            Assert.Throws<Program.InfinityException>(() => Program.Solve(1d, double.NegativeInfinity, 1d));

        }

    }
}
