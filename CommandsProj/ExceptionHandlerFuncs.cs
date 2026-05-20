using CommandsProj.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj
{
    public static class ExceptionHandlerFuncs
    {
        public static ICommand LogExCommandFunc(ICommand command, Exception ex)
        {
            return new LogExCommand(ex);
        }

        public static ICommand CallLogExCommandFunc(ICommand command, Exception ex)
        {
            return LogExCommandFunc(command, ex);
        }

        public static ICommand RepeatCommandFunc(ICommand command, Exception ex)
        {
            return new RepeatCommand(command);
            
        }

        public static ICommand CallRepeatCommandFunc(ICommand command, Exception ex)
        {
            return RepeatCommandFunc(command, ex);
        }


    }
}
