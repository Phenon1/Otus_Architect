using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW7.Classes
{
    public class Rotate
    {

        IRotateObject _obj;

        public Rotate(IRotateObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            _obj.SetAngle(Angle.RotateTo(_obj.GetAngle(),_obj.GetAnleVelocity()));
        }
    }
}

