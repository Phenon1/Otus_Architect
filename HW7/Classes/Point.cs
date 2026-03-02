using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW7.Classes
{
    public class Point
    {
        public int x;
        public int y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point MoveTo(Point point, Vector vector)
        {
            point.x += vector.dx;
            point.y += vector.dy;
            return point;
        }
    }
}
