using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelsProj.Classes;

namespace ModelsProj
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
    public interface IMovingObjectV2 : IMovingObject
    {
        void SetVelocity(Vector vector);
    }

    public interface IFuelHaveObject
    {
        float GetFuel();
        void SetFuel(float fuel);
        float GetFuelBurnVelocity();

    }

    public interface IRotateObject
    {
        Angle GetAngle();
        void SetAngle(Angle angle);

        Angle GetAnleVelocity();
    }

    public interface IMovingWithFuelObject : IMovingObject, IFuelHaveObject { }



}
