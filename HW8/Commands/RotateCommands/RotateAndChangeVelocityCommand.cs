using ModelsProj;
using ModelsProj.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands.RotateCommands
{
    public class RotateAndChangeVelocityCommand : ICommand
    {
        IRotateObject _obj;
        public RotateAndChangeVelocityCommand(IRotateObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            Rotate rotate = new Rotate(_obj);
            rotate.Execute();

            if(_obj is IMovingObjectV2 mov)
            {
                Vector velocity = mov.GetVelocity();
                double angle =  _obj.GetAngle().getAngleRadian();

                double speed = Math.Sqrt(velocity.dx * velocity.dx + velocity.dy * velocity.dy);
                int dx = (int)Math.Round(speed * Math.Cos(angle));
                int dy = (int)Math.Round(speed * Math.Sin(angle));

                ChangeVelocityCommand changeVelocityCommand = new ChangeVelocityCommand(mov, new Vector(dx,dy));
                changeVelocityCommand.Execute();
            }

        }
         
    }
}
