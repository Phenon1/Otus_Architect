using CommandsProj;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoCProj.Commands
{
    public class RegisterCommand : ICommand
    {
        private readonly string _key;
        private readonly IoC.DependencyStrategy _strategy;
        public RegisterCommand(string key, IoC.DependencyStrategy strategy) { _key = key; _strategy = strategy; }
        public void Execute() => IoC.GetCurrentStorage()[_key] = _strategy;
    }

}
