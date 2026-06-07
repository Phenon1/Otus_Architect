namespace CommandsProj
{
    public class CommandProcessorThread
    {
        private readonly QueueICommand _queue;
        private readonly object _syncRoot = new();
        private CancellationTokenSource? _hardStopCancellation;
        private Thread? _thread;
        private volatile bool _softStopRequested;

        public CommandProcessorThread(QueueICommand queue)
        {
            _queue = queue;
        }

        public bool IsRunning => _thread?.IsAlive == true;

        public void Start()
        {
            lock (_syncRoot)
            {
                if (IsRunning)
                {
                    return;
                }

                _softStopRequested = false;
                _hardStopCancellation = new CancellationTokenSource();
                _thread = new Thread(ProcessCommands)
                {
                    IsBackground = true,
                    Name = nameof(CommandProcessorThread)
                };
                _thread.Start();
            }
        }

        public void HardStop()
        {
            _hardStopCancellation?.Cancel();
        }

        public void SoftStop()
        {
            _softStopRequested = true;
            _queue.StopAdding();

            if (_queue.IsEmpty)
            {
                _hardStopCancellation?.Cancel();
            }
        }

        private void ProcessCommands()
        {
            CancellationToken token = _hardStopCancellation?.Token ?? CancellationToken.None;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    ICommand command = _queue.Dequeue(token);
                    _queue.ExecuteCommand(command);

                    if (_softStopRequested && _queue.IsEmpty)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
