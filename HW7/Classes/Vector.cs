using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ModelsProj.Classes
{
    public class Vector
    {
        public int dx;
        public int dy;
        public Vector(int dx, int dy)
        {
            this.dx = dx;
            this.dy = dy;
        }
        public override bool Equals(object? other)
        {
            if (other is not Vector vector) 
                return false;

            return (vector.dx == this.dx && vector.dy == this.dy);
        }
    }
}
