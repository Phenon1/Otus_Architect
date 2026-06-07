using CommandsProj.CommandExceptions;
using System.Collections.Concurrent;

namespace CommandsProj
{
    public class QueueICommand
    {
        private readonly BlockingCollection<ICommand> _commands;
        public volatile bool isAddingStopped;
        public int counterExecuteCommand = 0;

        public QueueICommand()
        {
            _commands = new BlockingCollection<ICommand>(new ConcurrentQueue<ICommand>());
        }

        public QueueICommand(ConcurrentStack<ICommand> commands)
            : this()
        {
            foreach (var command in commands.Reverse())
            {
                Enqueue(command);
            }
        }

        public bool IsEmpty => _commands.Count == 0;


        public void Enqueue(ICommand command)
        {
            if (!TryEnqueue(command))
            {
                throw new QueueAddCommandException("Очередь остановлена для добавления новых комманд");
            }
        }

        public bool TryEnqueue(ICommand command)
        {
            if (isAddingStopped)
            {
                return false;
            }

            _commands.Add(command);
            return true;
        }

        public void StopAdding()
        {
            isAddingStopped = true;
        }

        public ICommand Dequeue(CancellationToken cancel)
        {
            return _commands.Take(cancel);
        }

        public bool TryDequeue(out ICommand? command)
        {
            return _commands.TryTake(out command);
        }

        internal void ExecuteCommand(ICommand command)
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
                    TryEnqueue(recoveryCommand);
                }
            }
        }

        public void Execute(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                if (TryDequeue(out ICommand? command) && command != null)
                {
                    ExecuteCommand(command);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
