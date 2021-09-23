using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    public class MyPoint
    {
        public int X;
        public int Y;

        public MyPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public MyPoint(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
    }
}
