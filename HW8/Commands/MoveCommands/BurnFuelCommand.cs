using CommandsProj.CommandExceptions;
using ModelsProj;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands.MoveCommands
{
    public class BurnFuelCommand : ICommand
    {
        IFuelHaveObject _obj;

        public BurnFuelCommand(IFuelHaveObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            _obj.SetFuel(_obj.GetFuel() - _obj.GetFuelBurnVelocity());
        }
    }
}
