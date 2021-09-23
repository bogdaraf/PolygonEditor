using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    public class Edge
    {
        public Point A;
        public Point B;

        public Edge(Point a, Point b)
        {
            A = a;
            B = b;
        }
    }
}
