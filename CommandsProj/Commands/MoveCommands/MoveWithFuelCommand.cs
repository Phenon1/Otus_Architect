using ModelsProj;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands.MoveCommands
{
    public class MoveWithFuelCommand : ICommand
    {
        IMovingWithFuelObject _obj;

        public MoveWithFuelCommand(IMovingWithFuelObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            List<ICommand> cmds = new List<ICommand>();

            cmds.Add(new CheckFuelComamnd(_obj));
            cmds.Add(new MoveCommand(_obj));
            cmds.Add(new BurnFuelCommand(_obj));

            MacroCommand macroCommand = new MacroCommand(cmds);
            macroCommand.Execute();

        }
    }
}
