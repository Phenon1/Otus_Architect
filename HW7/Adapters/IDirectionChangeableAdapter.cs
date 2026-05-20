using ModelsProj.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelsProj.Adapters
{
    public class IDirectionChangeableAdapter : IDirectionChangeable
    {
        protected IUObject _object;
        public IDirectionChangeableAdapter(IUObject uObject)
        {
            _object = uObject;
        }
        public void SetVelocity(Vector vector)
        {
            _object.SetProperty<Vector>("Velocity", vector);
        }
        public Vector GetVelocity()
        {
            return _object.GetProperty<Vector>("Velocity");
        }


    }
}
