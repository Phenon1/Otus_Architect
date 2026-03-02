using Microsoft.VisualStudio.TestPlatform.TestHost;
using HW7;
using NUnit.Framework;
using Moq;
using HW7.Classes;
using HW7.Adapters;
using HW7.TypesObject;

namespace Tests
{
    public class HW7Test
    {
        [TestFixture]
        public class AngleTests
        {
          
            [TestCase((sbyte)0, 0.0)]
            [TestCase((sbyte)1, 9.0)]
            [TestCase((sbyte)2, 18.0)]
            [TestCase((sbyte)39, 351.0)]
            [TestCase((sbyte)41, 9.0)]
            [TestCase((sbyte)-1, 360.0-9.0)]
            [TestCase((sbyte)-40, 0.0)]
            public void GetAngleTest(sbyte sector, double expectedAngle)
            {
                var a = new Angle(sector);
                Assert.That(a.getAngle(), Is.EqualTo(expectedAngle).Within(1e-10));
            }

            [TestCase(0.0, 0)]
            [TestCase(0.0001, 0)]
            [TestCase(8.999999, 0)]
            [TestCase(9.0, 1)]
            [TestCase(17.999999, 1)]
            [TestCase(18.0, 2)]
            [TestCase(350.999999, 38)]
            [TestCase(351.0, 39)]
            [TestCase(359.999999, 39)]
            [TestCase(720.0, 0)]
            [TestCase(361.0, 0)]  
            [TestCase(369.0, 1)]   
            [TestCase(719.9999, 39)]
            public void GetSectorTests(double angle, byte expectedSector)
            {
                var a = new Angle(angle);
                Assert.That(a.getSector(), Is.EqualTo(expectedSector));
            }

            [TestCase(-1.0, 39)]    
            [TestCase(-9.0, 39)]    
            [TestCase(-10.0, 38)]   
            [TestCase(-360.0, 0)]
            [TestCase(-361.0, 39)]
            public void NegativeNormalizeTest(double angle, byte expectedSector)
            {
                var a = new Angle(angle);
                Assert.That(a.getSector(), Is.EqualTo(expectedSector));
            }

        }

        
        public class MoveTests
        {
            [Test]
            public void MoveNormalTest()
            {
                var obj = new SpaceShip();
                obj.SetProperty<Vector>("Velocity",new Vector(-7,3));
                obj.SetProperty<Point>("Location", new Point(12, 5));

                MovingObjectAdapter movingObjectAdapter = new MovingObjectAdapter(obj);
                Move moveExecutor = new Move(movingObjectAdapter);
                moveExecutor.Execute();

                Assert.That(movingObjectAdapter.GetLocation().x, Is.EqualTo(5));
                Assert.That(movingObjectAdapter.GetLocation().y, Is.EqualTo(8));
            }

            [Test]
            public void MoveExceptionTest()
            {
                Mock<IMovingObject> mockObject1 = new Mock<IMovingObject>();
                Mock<IMovingObject> mockObject2 = new Mock<IMovingObject>();
                Mock<IMovingObject> mockObject3 = new Mock<IMovingObject>();

                mockObject1.Setup(x => x.GetVelocity()).Throws(new NotImplementedException());
                mockObject2.Setup(x => x.GetLocation()).Throws(new NotImplementedException());

                var zeroPoint = new Point(0, 0);
                var zeroVector = new Vector(0, 0);
                mockObject3.Setup(x => x.GetLocation()).Returns(zeroPoint);
                mockObject3.Setup(x => x.GetVelocity()).Returns(zeroVector);
                mockObject3.Setup(x => x.SetLocation(It.IsAny<Point>())).Throws(new NotImplementedException());

                Move move1 = new Move(mockObject1.Object);
                Move move2 = new Move(mockObject2.Object);
                Move move3 = new Move(mockObject3.Object);

                Assert.Multiple(() =>
                {
                    Assert.Throws<NotImplementedException>(() => move1.Execute(), "Ошибка в move1");
                    Assert.Throws<NotImplementedException>(() => move2.Execute(), "Ошибка в move2");
                    Assert.Throws<NotImplementedException>(() => move3.Execute(), "Ошибка в move3");
                });
            }

        }

        public class RotateTests
        {
            [Test]
            public void RotateExceptionTest()
            {
                Mock<IRotateObject> mockObject1 = new Mock<IRotateObject>();
                Mock<IRotateObject> mockObject2 = new Mock<IRotateObject>();
                Mock<IRotateObject> mockObject3 = new Mock<IRotateObject>();

                mockObject1.Setup(x => x.GetAnleVelocity()).Throws(new NotImplementedException());
                mockObject1.Setup(x => x.GetAngle()).Returns(new Angle(1));

                mockObject2.Setup(x => x.GetAngle()).Throws(new NotImplementedException());
                mockObject2.Setup(x => x.GetAnleVelocity()).Returns(new Angle(1));


                mockObject3.Setup(x => x.GetAnleVelocity()).Returns(new Angle(1));
                mockObject3.Setup(x => x.GetAngle()).Returns(new Angle(1));
                mockObject3.Setup(x => x.SetAngle(It.IsAny<Angle>())).Throws(new NotImplementedException());

                Rotate rotate1 = new Rotate(mockObject1.Object);
                Rotate rotate2 = new Rotate(mockObject2.Object);
                Rotate rotate3 = new Rotate(mockObject3.Object);

                Assert.Multiple(() =>
                {
                    Assert.Throws<NotImplementedException>(() => rotate1.Execute(), "Ошибка в rotate1");
                    Assert.Throws<NotImplementedException>(() => rotate2.Execute(), "Ошибка в rotate2");
                    Assert.Throws<NotImplementedException>(() => rotate3.Execute(), "Ошибка в rotate3");
                });
            }

        }

    }
}
