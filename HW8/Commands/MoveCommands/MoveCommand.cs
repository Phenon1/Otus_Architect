using System;
using System.Collections.Generic;
using System.Text;
using ModelsProj;
using ModelsProj.Classes;

namespace CommandsProj.Commands.MoveCommands
{
    public class MoveCommand : ICommand
    {
        IMovingObject _obj;

        public MoveCommand(IMovingObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            _obj.SetLocation(
                Point.MoveTo(_obj.GetLocation(), _obj.GetVelocity()));
        }
    }
}
