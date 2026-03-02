using HW7.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW7.Adapters
{
    public class MovingObjectAdapter : IMovingObject
    {
        IUObject _object;
        public MovingObjectAdapter(IUObject uObject)
        {
            _object = uObject;
        }
        public Point GetLocation()
        {
            var location = _object.GetProperty<Point>("Location");
            return location;

        }

        public void SetLocation(Point p)
        {
            _object.SetProperty<Point>("Location",p);
        }

        public Vector GetVelocity()
        {
            return _object.GetProperty<Vector>("Velocity");
        }
    }
}
