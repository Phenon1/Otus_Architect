using ModelsProj;
using ModelsProj.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands.RotateCommands
{
    public class RotateCommand : ICommand
    {

        internal IRotateObject _obj;

        public RotateCommand(IRotateObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            _obj.SetAngle(Angle.RotateTo(_obj.GetAngle(), _obj.GetAnleVelocity()));
        }
    }
}
