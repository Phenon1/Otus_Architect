using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HW7.Classes;

namespace HW7
{
    public interface IUObject
    {
        public T GetProperty<T>(string key);
        void SetProperty<T>(string key, T val);
    }

    public interface IMovingObject
    {
        Point GetLocation();
        void SetLocation(Point location);
        Vector GetVelocity();
    }

    public interface IRotateObject
    {
        Angle GetAngle();
        void SetAngle(Angle angle);

        Angle GetAnleVelocity();
    }

}
