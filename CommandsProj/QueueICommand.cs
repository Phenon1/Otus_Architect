using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace CommandsProj
{
    public class QueueICommand
    {
        public readonly ConcurrentStack<ICommand> commands;
        public int counterExecuteCommand = 0;

        public QueueICommand()
        {
            this.commands = new ConcurrentStack<ICommand>();
        }
        public QueueICommand(ConcurrentStack<ICommand> commands)
        {
            this.commands = commands;
        }

        public void Execute(CancellationToken cancel) 
        {
            while (!cancel.IsCancellationRequested)
            {
                commands.TryPop(out ICommand? command);
                if (command != null)
                {
                    try
                    {
                        Interlocked.Add(ref counterExecuteCommand, 1);
                        command.Execute();
                    }
                    catch (Exception ex)
                    {
                        ICommand? recoveryCommand = ExceptionHandler.Handle(command, ex);
                        if (recoveryCommand != null)
                        {
                            commands.Push(recoveryCommand);
                        }
                    
                    }
                }
                else
                    break;
            }
        }

    }

   
}
