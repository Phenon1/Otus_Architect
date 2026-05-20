using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands
{
    public class SimpleLogCommand : ICommand
    {
        string _message;
        public SimpleLogCommand(string message)
        {
            _message = message;
        }

        public void Execute()
        {
            Console.WriteLine(_message);
        }
      
    }
}
