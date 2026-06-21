using CommandsProj.States;

namespace CommandsProj
{
    public class CommandProcessorThread
    {
        private readonly QueueICommand _queue;
        private readonly QueueICommand? _moveToQueue;
        private readonly object _syncRoot = new();
        private CancellationTokenSource? _hardStopCancellation;
        private Thread? _thread;
        private volatile bool _softStopRequested;
        private volatile ICommandProcessorState? _currentState;

        public CommandProcessorThread(QueueICommand queue, QueueICommand? moveToQueue = null)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _moveToQueue = moveToQueue;
        }

        public bool IsRunning => _thread?.IsAlive == true;
        public ICommandProcessorState? CurrentState => _currentState;

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
                _currentState = new NormalState(_queue, _moveToQueue);
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

            while (!token.IsCancellationRequested && _currentState != null)
            {
                try
                {
                    _currentState = _currentState.Handle(token);

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
