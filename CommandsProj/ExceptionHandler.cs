using CommandsProj.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommandsProj
{
    public static class ExceptionHandler
    {
         private static ConcurrentDictionary<
            Type,
            ConcurrentDictionary<
                Type,
                Func< ICommand, Exception, ICommand>
            >
        > _store = new();

        public static readonly ICommand? defExCommand = null;


        public static ICommand? Handle(ICommand command, Exception ex)
        {
            Type? type = command.GetType();
            Type? exType = ex.GetType();

            if (!_store.TryGetValue(type, out var dicExType))
                return defExCommand;

            if (!dicExType.TryGetValue(exType, out var func))
                return defExCommand;

            return func(command, ex);
        }

        public static void Register(Type comType, Type exType, Func<ICommand, Exception, ICommand> f)
        {
            var exDict = _store.GetOrAdd(comType, _ => new ConcurrentDictionary<
              Type,
              Func< ICommand, Exception, ICommand>
            >());
            _store[comType][exType] = f;
        }

        public static void Clear(){ _store.Clear(); }

    }
}
