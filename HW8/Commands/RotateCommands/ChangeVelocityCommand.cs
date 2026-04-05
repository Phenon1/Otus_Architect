using ModelsProj;
using ModelsProj.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands.RotateCommands
{
    public class ChangeVelocityCommand : ICommand
    {
        IMovingObjectV2 _obj;
        Vector _velocity;

        public ChangeVelocityCommand(IMovingObjectV2 obj, Vector velocity)
        {
            _obj = obj;
            _velocity = velocity;
        }

        public void Execute()
        {
            _obj.SetVelocity(_velocity);
        }
    }
}
