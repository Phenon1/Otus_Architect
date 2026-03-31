using CommandsProj;
using CommandsProj.Commands;
using CommandsProj.CommandExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal class TestWrongCommand1 : ICommand
    {
        public void Execute()
        {
            throw new CommandException();
        }

    }

    internal class TestWrongCommand2 : ICommand
    {
        public void Execute()
        {
            throw new CommandException();
        }

    }
    public class HW8Test
    {
        static CancellationTokenSource cts = new CancellationTokenSource();


        public class ExceptionHandlerTest
        {
            [Test]
            public void LogExCommandTest()
            {
                LogExCommand logExCommand1 = new LogExCommand();
                LogExCommand logExCommand2 = new LogExCommand(new Exception());

                using StringWriter sw = new StringWriter();
                Console.SetOut(sw);

                logExCommand1.Execute();
                Assert.That(sw.ToString(), !Is.Empty);
                sw.GetStringBuilder().Clear();


                logExCommand2.Execute();
                Assert.That(sw.ToString(), !Is.Empty);
                sw.GetStringBuilder().Clear();


                QueueICommand queue = new QueueICommand();
                TestWrongCommand1 wrongCommand1 = new TestWrongCommand1();
                queue.commands.Enqueue(wrongCommand1);  

                queue.Execute(cts.Token);
                Console.SetOut(sw);
                Assert.That(sw.ToString().Trim(), Is.EqualTo(LogExCommand.defExMessage));
                sw.GetStringBuilder().Clear();



                queue = new QueueICommand();
                ExceptionHandler.Register(typeof(TestWrongCommand2), typeof(CommandException), ExceptionHandlerFuncs.AddLogExCommandFunc);
                TestWrongCommand2 wrongCommand2 = new TestWrongCommand2();
                queue.commands.Enqueue(wrongCommand2);
                queue.Execute(cts.Token);
                Console.SetOut(sw);
                Assert.That(sw.ToString().Trim(), Is.Not.EqualTo(LogExCommand.defExMessage));
                Assert.That(sw.ToString().Trim(), Is.Not.Empty);
                Assert.That(queue.counterExecuteCommand, Is.EqualTo(2));
                sw.GetStringBuilder().Clear();


            }
            [Test]
            public void RepeatCommandTest()
            {
                ExceptionHandler.Clear();
                using StringWriter sw = new StringWriter();
                Console.SetOut(sw);

                QueueICommand queue = new QueueICommand();
                ExceptionHandler.Register(typeof(TestWrongCommand1), typeof(CommandException), ExceptionHandlerFuncs.RepeatCommandFunc);

                TestWrongCommand1 wrongCommand1 = new TestWrongCommand1();
                queue.commands.Enqueue(wrongCommand1);
                queue.Execute(cts.Token);
                Assert.That(queue.counterExecuteCommand, Is.EqualTo(2));
                Assert.That(sw.ToString().Trim(), Is.EqualTo(LogExCommand.defExMessage));

                sw.GetStringBuilder().Clear();


                queue = new QueueICommand();
                ExceptionHandler.Register(typeof(RepeatCommand), typeof(CommandException), ExceptionHandlerFuncs.LogExCommandFunc);
                queue.commands.Enqueue(wrongCommand1);
                queue.Execute(cts.Token);
                Assert.That(sw.ToString().Trim(), Is.Not.EqualTo(LogExCommand.defExMessage));
                Assert.That(sw.ToString().Trim(), Is.Not.Empty);
                Assert.That(queue.counterExecuteCommand, Is.EqualTo(2));

                sw.GetStringBuilder().Clear();



                queue = new QueueICommand();
                ExceptionHandler.Clear();

                ExceptionHandler.Register(typeof(TestWrongCommand1), typeof(CommandException), ExceptionHandlerFuncs.CallRepeatCommandFunc);
                ExceptionHandler.Register(typeof(CallRepeatCommand), typeof(CommandException), ExceptionHandlerFuncs.RepeatCommandFunc);
                ExceptionHandler.Register(typeof(RepeatCommand), typeof(CommandException), ExceptionHandlerFuncs.LogExCommandFunc);

                queue.commands.Enqueue(wrongCommand1);
                queue.Execute(cts.Token);
                Assert.That(sw.ToString().Trim(), Is.Not.EqualTo(LogExCommand.defExMessage));
                Assert.That(sw.ToString().Trim(), Is.Not.Empty);
                Assert.That(queue.counterExecuteCommand, Is.EqualTo(3));

                sw.GetStringBuilder().Clear();




            }
        }


    
    }
    
}
