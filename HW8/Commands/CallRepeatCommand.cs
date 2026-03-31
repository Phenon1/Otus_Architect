using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands
{
    public class CallRepeatCommand : ICommand
    {
        ICommand _command;
        public CallRepeatCommand(ICommand command)
        {
            _command = command;
        }
        public void Execute()
        {
            new RepeatCommand(_command).Execute();
        }
    }
}
