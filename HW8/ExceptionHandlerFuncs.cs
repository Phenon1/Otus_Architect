using CommandsProj.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj
{
    public static class ExceptionHandlerFuncs
    {
        public static ICommand LogExCommandFunc(ConcurrentQueue<ICommand> commands, ICommand command, Exception ex)
        {
            return new LogExCommand(ex);
        }

        public static ICommand AddLogExCommandFunc(ConcurrentQueue<ICommand> commands, ICommand command, Exception ex)
        {
            commands.Enqueue(new LogExCommand(ex));
            return new EmptyCommand();
        }

        public static ICommand RepeatCommandFunc(ConcurrentQueue<ICommand> commands, ICommand command, Exception ex)
        {
            commands.Enqueue(new RepeatCommand(command));
            return new EmptyCommand();
        }

        public static ICommand CallRepeatCommandFunc(ConcurrentQueue<ICommand> commands, ICommand command, Exception ex)
        {
            commands.Enqueue(new CallRepeatCommand(command));
            return new EmptyCommand();
        }


    }
}
