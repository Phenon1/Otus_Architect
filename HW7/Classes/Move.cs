using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW7.Classes
{
    public class Move
    {
        IMovingObject _obj;

        public Move(IMovingObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            _obj.SetLocation(
                Point.MoveTo(_obj.GetLocation(), _obj.GetVelocity()));
        }
    }
}
