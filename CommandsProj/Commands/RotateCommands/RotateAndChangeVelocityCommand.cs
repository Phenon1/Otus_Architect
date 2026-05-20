using ModelsProj;
using ModelsProj.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommandsProj.Commands.RotateCommands
{
    public class RotateAndChangeVelocityCommand : MacroCommand
    {
        public RotateAndChangeVelocityCommand(RotateCommand rotateCommand, ChangeVelocityCommand changeVelocityCommand)
        : base(
            new List<ICommand> { 
                rotateCommand,
                changeVelocityCommand
            }
        ){}
   
    }
}
