using CommandsProj;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace IoCProj.Commands
{

    public class NewScopeCommand : ICommand
    {
        private readonly string _scopeId;
        public NewScopeCommand(string scopeId) => _scopeId = scopeId;

        public void Execute()
        {
            var newStorage = new Dictionary<string, IoC.DependencyStrategy>();
            IoC.Resolve<ICommand>("IoC.Register", $"Scopes.Id.{_scopeId}", (IoC.DependencyStrategy)((args) => newStorage)).Execute();
            IoC.SetCurrentScope(newStorage);
        }
    }


}
