using System;
using System.Collections.Generic;
using System.Text;

namespace CommandsProj.CommandExceptions
{
    public class QueueAddCommandException : InvalidOperationException
    {
        public QueueAddCommandException(string text) : base(text) { }
    }
}
