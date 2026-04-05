using ModelsProj.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelsProj.Adapters
{
    public class MovingObjectV2Adapter : MovingObjectAdapter,IMovingObjectV2
    {
        public MovingObjectV2Adapter(IUObject uObject): base(uObject){}

        public void SetVelocity(Vector vector)
        {
            _object.SetProperty<Vector>("Velocity", vector);
        }

    }
}
