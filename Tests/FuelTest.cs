using CommandsProj.CommandExceptions;
using CommandsProj.Commands.MoveCommands;
using ModelsProj;
using ModelsProj.Adapters;
using ModelsProj.Classes;
using Moq;

namespace Tests
{
    public class FuelTest
    {
        [TestFixture]
        public class AngleTests
        {

            [TestCase(float.NaN)]
            [TestCase(-1)]
            [TestCase(float.NegativeZero)]
            [TestCase(float.NegativeInfinity)]
            public void FuelObjectAdapterWrongTest(float fuel)
            {
                Mock<IUObject> mockObject1 = new Mock<IUObject>();
                var fuelObjectAdapter = new FuelObjectAdapter(mockObject1.Object);

                Assert.Throws<ArgumentException>(() => fuelObjectAdapter.SetFuel(fuel));
            }

            [TestCase(1, 0.1f)]
            [TestCase(100, 100)]
            public void CheckFuelComamndGoodTest(float fuel, float fuelBurn)
            {
                Mock<IFuelHaveObject> mockFuelObject1 = new Mock<IFuelHaveObject>();
                mockFuelObject1.Setup(x => x.GetFuel()).Returns(fuel);
                mockFuelObject1.Setup(x => x.GetFuelBurnVelocity()).Returns(fuelBurn);

                var checkFuelCommand = new CheckFuelComamnd(mockFuelObject1.Object);

                Assert.DoesNotThrow(() => checkFuelCommand.Execute());
            }

           
            [TestCase(0, 0.1f)]
            [TestCase(100, 101)]
            public void CheckFuelComamndWrongTest(float fuel, float fuelBurn)
            {
                Mock<IFuelHaveObject> mockFuelObject1 = new Mock<IFuelHaveObject>();
                mockFuelObject1.Setup(x => x.GetFuel()).Returns(fuel);
                mockFuelObject1.Setup(x => x.GetFuelBurnVelocity()).Returns(fuelBurn);

                var checkFuelCommand = new CheckFuelComamnd(mockFuelObject1.Object);

                Assert.Throws<FuelLowException>(() => checkFuelCommand.Execute());
            }

            [TestCase(1, 0.1f, 1-0.1f)]
            [TestCase(100, 100, 0)]
            public void BurnFuelCommandTest(float fuel, float fuelBurn, float expectedFuel)
            {
                float currentFuel = fuel;

                Mock<IFuelHaveObject> mockFuelObject1 = new Mock<IFuelHaveObject>();
                mockFuelObject1.Setup(x => x.GetFuel()).Returns(() => currentFuel);
                mockFuelObject1.Setup(x => x.GetFuelBurnVelocity()).Returns(fuelBurn);
                mockFuelObject1.Setup(x => x.SetFuel(It.IsAny<float>()))
                    .Callback<float>(val => currentFuel = val);

                var burnFuelCommand = new BurnFuelCommand(mockFuelObject1.Object);
                burnFuelCommand.Execute();

                Assert.That(mockFuelObject1.Object.GetFuel(), Is.EqualTo(expectedFuel).Within(0.001f));
            }

            [TestCase(1, 0.1f, 1 - 0.1f, 1 - 0.2f, 1 ,0 ,0 ,0)]
            [TestCase(100, 50, 50, 0, 1, 1, 1, 1)]
            public void MoveWithFuelCommandTest(float fuel, float fuelBurn, float expectedFuel, float expectedFuelAfterTwice, int vectorVelocityDx,
                int vectorVelocityDy, int LocationX, int LocationY)
            {
                float currentFuel = fuel;
                Vector velocity = new Vector(vectorVelocityDx, vectorVelocityDy);
                Point location = new Point(LocationX, LocationY);

                Mock<IMovingWithFuelObject> mockFuelObject1 = new Mock<IMovingWithFuelObject>();

                mockFuelObject1.Setup(x => x.GetFuel()).Returns(() => currentFuel);
                mockFuelObject1.Setup(x => x.GetFuelBurnVelocity()).Returns(fuelBurn);
                mockFuelObject1.Setup(x => x.SetFuel(It.IsAny<float>()))
                    .Callback<float>(val => currentFuel = val);

                mockFuelObject1.Setup(x => x.GetVelocity()).Returns(velocity);
                mockFuelObject1.Setup(x => x.GetLocation()).Returns(location);
                mockFuelObject1.Setup(x => x.SetLocation(It.IsAny<Point>()))
                  .Callback<Point>(val => location = val);


                var burnFuelCommand = new MoveWithFuelCommand(mockFuelObject1.Object);
                burnFuelCommand.Execute();

                Assert.That(mockFuelObject1.Object.GetFuel(), Is.EqualTo(expectedFuel).Within(0.001f));
                Assert.That(mockFuelObject1.Object.GetLocation().x, Is.EqualTo(vectorVelocityDx + LocationX).Within(0.001f));
                Assert.That(mockFuelObject1.Object.GetLocation().y, Is.EqualTo(vectorVelocityDy + LocationY).Within(0.001f));

                burnFuelCommand.Execute();

                Assert.That(mockFuelObject1.Object.GetFuel(), Is.EqualTo(expectedFuelAfterTwice).Within(0.001f));
                Assert.That(mockFuelObject1.Object.GetLocation().x, Is.EqualTo(vectorVelocityDx * 2 + LocationX).Within(0.001f));
                Assert.That(mockFuelObject1.Object.GetLocation().y, Is.EqualTo(vectorVelocityDy * 2 + LocationY ).Within(0.001f));
            }

        }
    }
}
