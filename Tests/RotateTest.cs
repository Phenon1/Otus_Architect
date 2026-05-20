
using CommandsProj.Commands.RotateCommands;
using ModelsProj;
using ModelsProj.Classes;
using Moq;

namespace Tests
{
    public interface IMoveAndRotateObject : IDirectionChangeable, IRotateObject;
    public class RotateTest
    {
        [TestCase(1, 1, 1 ,- 1)]
        [TestCase(0, 0, 1, 2)]
        public void ChangeVelocityCommandTest(int velocityDx,int velocityDy, int newVelocityDx, int newVelocityDy)
        {
            Vector velocity = new Vector(velocityDx, velocityDy);
            Vector newVelocity = new Vector(newVelocityDx, newVelocityDy);

            Mock<IDirectionChangeable> mockFuelObject1 = new Mock<IDirectionChangeable>();

            mockFuelObject1.As<IMovingObject>().Setup(x => x.GetVelocity()).Returns(() => velocity);
            mockFuelObject1.Setup(x => x.SetVelocity(It.IsAny<Vector>()))
                .Callback<Vector>(val => velocity = val);


            var changeVelocityCommand = new ChangeVelocityCommand(mockFuelObject1.Object, newVelocity);
            changeVelocityCommand.Execute();

            Assert.That(mockFuelObject1.As<IMovingObject>().Object.GetVelocity(), Is.EqualTo(newVelocity));
            
        }

        [TestCase(100, 100, 5, 0, 100, 100)]
        [TestCase(100, 100, 5, 10, -100, 100)]
        [TestCase(100, 100, 5, 20, -100, -100)]
        [TestCase(100, 100, 5, 30, 100, -100)]
        [TestCase(100, 100, 5, 40, 100, 100)]
        public void RotateAndChangeVelocityCommandTest(int velocityDx, int velocityDy, sbyte currAngleSector, sbyte rotateAngleSector, int expectVelocityDx, int expectVelocityDy)
        {
            Vector velocity = new Vector(velocityDx, velocityDy);
            Angle angle1RotateVelocity = new Angle(rotateAngleSector);
            Angle angle1 = new Angle(currAngleSector);


            Mock<IMoveAndRotateObject> mockFuelObject1 = new Mock<IMoveAndRotateObject>();

            mockFuelObject1.As<IDirectionChangeable>().Setup(x => x.GetVelocity()).Returns(() => velocity);
            mockFuelObject1.Setup(x => x.SetVelocity(It.IsAny<Vector>()))
                .Callback<Vector>(val => velocity = val);

            mockFuelObject1.Setup(x => x.GetAngle()).Returns(() => angle1);
            mockFuelObject1.Setup(x => x.SetAngle(It.IsAny<Angle>()))
               .Callback<Angle>(val => angle1 = val);
            mockFuelObject1.Setup(x => x.GetAnleVelocity()).Returns(() => angle1RotateVelocity);


            RotateCommand rotateCommand = new RotateCommand(mockFuelObject1.Object);
            ChangeVelocityCommand changeVelocityCommand = new ChangeVelocityCommand(mockFuelObject1.Object);

            var rotateAndChangeVelocityCommand = new RotateAndChangeVelocityCommand(rotateCommand, changeVelocityCommand);
            rotateAndChangeVelocityCommand.Execute();

            Vector newVelocity = mockFuelObject1.As<IDirectionChangeable>().Object.GetVelocity();

            Assert.That(newVelocity.dx, Is.EqualTo(expectVelocityDx).Within(1.0));
            Assert.That(newVelocity.dy, Is.EqualTo(expectVelocityDy).Within(1.0));


           
        }

    }
}
