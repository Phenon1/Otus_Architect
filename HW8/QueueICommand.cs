using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace CommandsProj
{
    public class QueueICommand
    {
        public readonly ConcurrentQueue<ICommand> commands;
        public int counterExecuteCommand = 0;

        public QueueICommand()
        {
            this.commands = new ConcurrentQueue<ICommand>();
        }
        public QueueICommand(ConcurrentQueue<ICommand> commands)
        {
            this.commands = commands;
        }

        public void Execute(CancellationToken cancel) 
        {
            while (!cancel.IsCancellationRequested)
            {
                commands.TryDequeue(out ICommand? command);
                if (command != null)
                {
                    try
                    {
                        Interlocked.Add(ref counterExecuteCommand, 1);
                        command.Execute();
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.Handle(commands, command, ex).Execute();
                    }

                }
                else
                    break;
            }
        }

    }

   
}
