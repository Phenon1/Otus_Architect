using CommandsProj;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoCProj.Commands
{
    internal class CurrentScopeCommand : ICommand
    {
        private readonly string _scopeId;
        public CurrentScopeCommand(string scopeId) => _scopeId = scopeId;

        public void Execute()
        {
             
            var targetStorage = IoC.Resolve<IDictionary<string, IoC.DependencyStrategy>>($"Scopes.Id.{_scopeId}");
            IoC.SetCurrentScope(targetStorage);
        }
    }
}
