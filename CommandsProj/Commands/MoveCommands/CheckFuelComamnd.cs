using CommandsProj.CommandExceptions;
using ModelsProj;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands.MoveCommands
{
    public class CheckFuelComamnd : ICommand
    {
        IFuelHaveObject _obj;

        public CheckFuelComamnd( IFuelHaveObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            if (_obj.GetFuel() - _obj.GetFuelBurnVelocity() < 0)
                throw new FuelLowException();
        }
    }
}
