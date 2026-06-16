using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsProj.TypesObject
{
    public abstract class Uobject : IUObject
    {
        protected Dictionary<string, object> _values = new Dictionary<string, object>();
        public virtual T GetProperty<T>(string key)
        {
            return (T)_values[key];
        }
        public virtual void SetProperty<T>(string key, T val)
        {
            _values[key] = val!;
        }
    }
}
