using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.Commands
{
    public class LogExCommand : ICommand
    {
        public const string defExMessage = "Необработанная ошибка";
        Exception? _ex;
        public LogExCommand() { }

        public LogExCommand(Exception ex) 
        {
            _ex = ex;
        }
        public void Execute() 
        {
            if (_ex == null)
                Console.WriteLine(defExMessage);
            else
                Console.WriteLine($"Ошибка {_ex.Message} Trace: {_ex.StackTrace}");
        }
    }
}
