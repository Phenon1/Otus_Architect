using System.Collections.Concurrent;

namespace CommandsProj
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool stop = false;
            ConcurrentQueue <ICommand> commands = new ConcurrentQueue <ICommand>(); 
            

            while(!stop)
            {
                if (commands.TryDequeue(out ICommand? cmd))
                {
                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception ex) 
                    {
                        ExceptionHandler.Handle(commands, cmd, ex).Execute();
                    }
                }
                
            }
            
        }  
        
    }
}
