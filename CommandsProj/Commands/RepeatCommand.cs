using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands
{
    public class RepeatCommand : ICommand
    {
        ICommand _command;
        public RepeatCommand(ICommand command)
        {
            _command = command;
        }
        public void Execute()
        {
            _command.Execute();
        }
    }
}
