using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    class DisjointSet
    {
        Int32[] parents;    /* Массив, хранящий индексы родительских элементов */
        Int32[] ranks;      /* Массив, хранящий ранк элемента (верхняю границу его
                             * высоты, то есть длиннейшей ветви в нем) */

        public Int32 Length
        {
            get
            {
                return parents.Length;
            }
        }

        public DisjointSet(Int32 length)
        {
            parents = new Int32[length];
            ranks = new Int32[length];
        }

        public void MakeSet(Int32 x)
        {
            parents[x] = x;
            ranks[x] = 0;
        }
        public Int32 Find(Int32 x)
        {
            if (parents[x] == x)
            {
                return x;
            }
            return parents[x] = Find(parents[x]);
        }
        public Int32 Link(Int32 x, Int32 y)
        {
            x = Find(x);
            y = Find(y);

            /* Union-By-Size */
            if (x != y)
            {
                if (ranks[x] < ranks[y])
                {
                    Swap(ref x, ref y);
                }

                parents[y] = x;

                if (ranks[x] == ranks[y])
                {
                    ranks[x]++;
                }
            }

            return x;
        }

        static private void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
