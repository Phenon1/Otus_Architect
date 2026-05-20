using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands
{

    public class MacroCommand : ICommand
    {
        List<ICommand> _cmds;
        public MacroCommand(List<ICommand> cmds)
        {
            _cmds = cmds;
        }

        public void Execute()
        {
            for (int i = 0; i < _cmds.Count(); i++)
            {
                _cmds[i].Execute();
            }
        }
    }
}
