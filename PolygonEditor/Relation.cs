using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    public class Relation
    {
        public Edge M;
        public Edge N;

        public int type;
        public int number;

        public Relation()
        {

        }

        public Relation(Edge m, Edge n, int type)
        {
            M = m;
            N = n;
            this.type = type;
        }
    }
}
