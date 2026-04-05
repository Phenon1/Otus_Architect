using System;
using System.Collections.Generic;
using System.Text;

namespace ModelsProj.Adapters
{
    public class FuelObjectAdapter : IFuelHaveObject
    {
        IUObject _object;
        public FuelObjectAdapter(IUObject uObject)
        {
            _object = uObject;
        }
        public float GetFuel ()
        {
            var location = _object.GetProperty<float>("Fuel");
            return location;

        }
        public float GetFuelBurnVelocity()
        {
            var fuelBurnVelocity = _object.GetProperty<float>("FuelBurnVelocity");
            return fuelBurnVelocity;

        }
        
        public void SetFuel(float fuel)
        {
            if (fuel < 0 || float.IsNaN(fuel) || fuel == float.NegativeZero)
                throw new ArgumentException($"Уровень топлива не может быть {fuel}");
            _object.SetProperty<float>("Fuel", fuel);
        }

    }
}
