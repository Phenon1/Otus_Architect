using ModelsProj;
using ModelsProj.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommandsProj.Commands.RotateCommands
{
    public class ChangeVelocityCommand : ICommand
    {
        IDirectionChangeable _obj;
        Vector? _velocity;

        public ChangeVelocityCommand(IDirectionChangeable obj)
        {
            _obj = obj;
        }

        public ChangeVelocityCommand(IDirectionChangeable obj, Vector velocity)
        {
            _obj = obj;
            _velocity = velocity;
        }

        public void Execute()
        {
            if (_velocity != null)
            {
                _obj.SetVelocity(_velocity);
                return;
            }
            if (_obj is IRotateObject rotateObject)
            {

                Vector velocity = _obj.GetVelocity();


                double speed = Math.Sqrt(velocity.dx * velocity.dx + velocity.dy * velocity.dy);
                int dx = (int)Math.Round(speed * Math.Cos(rotateObject.GetAngle().getAngleRadian()));
                int dy = (int)Math.Round(speed * Math.Sin(rotateObject.GetAngle().getAngleRadian()));

                _obj.SetVelocity(new Vector(dx, dy));
            }
        }
    }
}
