using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    class Node
    {
        #region Приватные поля

        List<Pixel> pixels;

        public Node parent;     ///УБРАТЬ PUBLIC НЕ ЗАБУДЬ
        List<Node> children;

        const Int32 delta = 20;  /* Параметр метода MSER. */

        #endregion

        #region Публичные свойства

        public Int32 Level
        {
            get
            {
                return (Int32)pixels[0].color;
            }
        }
        public Int32 Area
        {
            get
            {
                Int32 area = pixels.Count;

                foreach (var child in children)
                {
                    area += child.Area;
                }

                return area;
            }
        }
        public Int32 Highest { set; get; }
        public Double Variation { set; get; }

        public List<Pixel> Pixels
        {
            get
            {
                return new List<Pixel>(pixels);
            }
        }
        public List<Node> Children
        {
            get
            {
                return new List<Node>(children);
            }
        }

        #endregion

        public Node(Pixel pixel)
        {
            (this.pixels = new List<Pixel>()).Add(pixel);

            this.parent = null; //@
            this.children = new List<Node>();

            this.Highest = pixel.color;
            this.Variation = Double.MaxValue;
        }

        #region Публичные методы

        public void AddPixels(List<Pixel> pixels_)
        {
            foreach (var pixel in pixels_)
            {
                this.pixels.Add(pixel);
            }
        }
        public void AddChildren(List<Node> children_)
        {
            foreach (var child in children_)
            {
                this.children.Add(child);
            }
        }

        public void AddPixel(Pixel pixel)
        {
            pixels.Add(pixel);
        }
        public void AddChild(Node child)
        {
            child.parent = this;
            children.Add(child);
        }

        public void CalculateVariation()
        {
            Node grandparent = this;

            while (grandparent.Level > this.Level - delta)
            {
                if ((grandparent = grandparent.parent) == null)
                {
                    return;
                }
            }

            Variation = (Double)(grandparent.Area - this.Area) / (Double)this.Area;
        }

        #endregion
    }

    class ComponentTree
    {
        #region Приватные поля

        Pixel[] pixels;         /* Отсортированный по интенсивности массив
                                 * (в обратном порядке, от 255 к 0) */

        //QTree:
        Int32[] QTreeParents;   /* Массив, хранящий индексы родительских элементов */
        Int32[] QTreeRanks;     /* Массив, хранящий ранк элемента (верхняю границу его
                                 * высоты, то есть длиннейшей ветви в нем) */

        //QNode:
        Int32[] QNodeParents;
        Int32[] QNodeRanks;

        Int32[] lowestNode;

        Node[] nodes;

        Int32[,] accessible;    /* Массив доступных пикселей.
                                 * (Пиксель становится доступным после его обработки.
                                 * На поле accessible записывается порядковый номер пикселя
                                 * в массиве pixels на Position пикселя на original.) */

        Int32 root;             /* Номер корневого Node */

        //const Int32 delta = 8;  /* Параметр MSER. */

        #endregion

        #region Публичные свойства

        public Int32 Length
        {
            get
            {
                return pixels.Length;
            }
        }

        #endregion

        #region Конструкторы

        public ComponentTree(GrayscaleBitmap original)
        {
            pixels = original.DecreasingCountingSort();

            accessible = new Int32[original.Width, original.Height];

            for (Int32 i = 0; i < original.Width; i++)
            {
                for (Int32 j = 0; j < original.Height; j++)
                {
                    accessible[i, j] = Int32.MinValue;
                }
            }

            QTreeParents = new Int32[Length];
            QTreeRanks = new Int32[Length];

            QNodeParents = new Int32[Length];
            QNodeRanks = new Int32[Length];

            nodes = new Node[Length];
            lowestNode = new Int32[Length];

            DisjointSets();

            CalculateVariation(nodes[root]);    //@ Вычисляем вариацию, начиная с корня
        }

        #endregion

        #region Система непересекающихся множеств (Union-Find Algorithm)

        public void QTreeMakeSet(Int32 x)
        {
            QTreeParents[x] = x;
            QTreeRanks[x] = 0;
        }
        public void QNodeMakeSet(Int32 x)
        {
            QNodeParents[x] = x;
            QNodeRanks[x] = 0;
        }

        public Int32 QTreeFind(Int32 x)
        {
            if (QTreeParents[x] == x)
            {
                return x;
            }
            return QTreeParents[x] = QTreeFind(QTreeParents[x]);
        }
        public Int32 QNodeFind(Int32 x)
        {
            if (QNodeParents[x] == x)
            {
                return x;
            }
            return QNodeParents[x] = QNodeFind(QNodeParents[x]);
        }

        public Int32 QTreeLink(Int32 x, Int32 y)
        {
            x = QTreeFind(x);
            y = QTreeFind(y);

            if (x != y)
            {
                if (QTreeRanks[x] < QTreeRanks[y])
                {
                    Swap(ref x, ref y);
                }

                QTreeParents[y] = x;

                if (QTreeRanks[x] == QTreeRanks[y])
                {
                    QTreeRanks[x]++;
                }
            }

            return x;
        }
        public Int32 QNodeLink(Int32 x, Int32 y)
        {
            x = QNodeFind(x);
            y = QNodeFind(y);

            if (x != y)
            {
                if (QNodeRanks[x] < QNodeRanks[y])
                {
                    Swap(ref x, ref y);
                }

                QNodeParents[y] = x;

                if (QNodeRanks[x] == QNodeRanks[y])
                {
                    QNodeRanks[x]++;
                }
            }

            return x;
        }

        Node MakeNode(Pixel pixel)
        {
            return new Node(pixel);
        }
        Int32 MergeNodes(Int32 node1, Int32 node2)
        {
            Int32 tmpNode = QNodeLink(node1, node2), tmpNode2;

            if (tmpNode == node2)
            {
                nodes[node2].AddPixels(nodes[node1].Pixels);
                nodes[node2].AddChildren(nodes[node1].Children);

                tmpNode2 = node1;
            }
            else
            {
                nodes[node1].AddPixels(nodes[node2].Pixels);
                nodes[node1].AddChildren(nodes[node2].Children);

                tmpNode2 = node2;
            }

            nodes[tmpNode].Highest = Math.Max(nodes[tmpNode].Highest, nodes[tmpNode2].Highest);

            return tmpNode;
        }

        List<Int32> AccessibleBorderPixels(Point central)
        {
            List<Int32> border = new List<Int32>(4);

            /*
            | N/A |0,-1| N/A |
            |-1, 0| CP |+1, 0|
            | N/A |0,+1| N/A |
            */

            /* -1, 0 */
            try
            {
                if (accessible[central.X - 1, central.Y] >= 0)
                {
                    border.Add(accessible[central.X - 1, central.Y]);
                }
            }
            catch (IndexOutOfRangeException) { }

            /* 0, -1 */
            try
            {
                if (accessible[central.X, central.Y - 1] >= 0)
                {
                    border.Add(accessible[central.X, central.Y - 1]);
                }
            }
            catch (IndexOutOfRangeException) { }

            /* 1, 0 */
            try
            {
                if (accessible[central.X + 1, central.Y] >= 0)
                {
                    border.Add(accessible[central.X + 1, central.Y]);
                }
            }
            catch (IndexOutOfRangeException) { }

            /* 0, 1 */
            try
            {
                if (accessible[central.X, central.Y + 1] >= 0)
                {
                    border.Add(accessible[central.X, central.Y + 1]);
                }
            }
            catch (IndexOutOfRangeException) { }

            return border;
        }

        void DisjointSets()
        {
            /* 2. foreach p из V do {MakeSettree(p); MakeSetnode(p);
             * nodes[p]:= MakeNode(F(p)); lowestNode[p] := p;}; */

            for (Int32 p = 0; p < Length; p++)
            {
                QTreeMakeSet(p);
                QNodeMakeSet(p);

                nodes[p] = MakeNode(pixels[p]);

                lowestNode[p] = p;
            }

            /* 3. foreach p из V in decreasing order of level for F
             * (F - интенсивность) do */
            for (Int32 p = 0; p < Length; p++)
            {
                accessible[pixels[p].Position.X, pixels[p].Position.Y] = p;

                Int32 curTree = QTreeFind(p);
                Int32 curNode = QNodeFind(lowestNode[curTree]);

                /* 6. foreach already processed neighbor
                    * (для уже обработанных, доступных) q of p with F(q) >= F(p)  */

                List<Int32> border = AccessibleBorderPixels(pixels[p].Position);

                for (Int32 q = 0; q < border.Count; q++)
                {
                    Int32 adjTree = QTreeFind(border[q]);
                    Int32 adjNode = QNodeFind(lowestNode[adjTree]);

                    if (curNode != adjNode)
                    {
                        if (nodes[curNode].Level == nodes[adjNode].Level)
                        {
                            curNode = MergeNodes(adjNode, curNode);
                        }
                        else
                        {
                            nodes[curNode].AddChild(nodes[adjNode]);
                            nodes[curNode].Highest = Math.Max(nodes[curNode].Highest, nodes[adjNode].Highest);
                        }

                        curTree = QTreeLink(adjTree, curTree);
                        lowestNode[curTree] = curNode;
                    }
                }
            }

            // 15. Root := lowestNode[Findtree(Findnode(0))] ;

            root = lowestNode[QTreeFind(QNodeFind(0))];
        }

        #endregion

        #region MSER Tracking

        private void CalculateVariation(Node current)
        {
            current.CalculateVariation();

            foreach (var child in current.Children)
            {
                child.CalculateVariation();
            }
        }

        private List<Node> MaximallyStableExtremalRegion()
        {
            // 1. Вычисляем значение вариации для всех листьев дерева

            CalculateVariation(nodes[root]);

            // 2. Ищем локальный минимум
            return new List<Node>();
        }

        #endregion

        public override string ToString()
        {

            using (System.IO.StreamWriter file = new System.IO.StreamWriter("output.txt"))
            {
                file.WriteLine("TREE:");
                file.WriteLine("Root: " + nodes[root].Pixels[0].ToString());

                file.WriteLine("Root children:" + nodes[root].Children.Count);

                foreach (var child in nodes[root].Children)
                {
                    file.WriteLine(child.Level + " (" + child.Area + ")");
                    file.WriteLine("Children: ");

                    foreach (var child_ in child.Children)
                    {
                        file.WriteLine(child_.Level + " (" + child_.Area + ")");


                        file.WriteLine("Children*: ");

                        foreach (var child__ in child_.Children)
                        {
                            file.WriteLine(child__.Level + " (" + child__.Area + ")");


                            file.WriteLine("Children**: ");
                            foreach (var child___ in child__.Children)
                            {
                                file.WriteLine(child___.Level + " (" + child___.Area + ")");

                            }
                        }
                    }

                    file.WriteLine("NODES:");

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (nodes[i].parent != null)
                        {
                            file.WriteLine(i + " is " + nodes[i].Level + " (" + nodes[i].Area + ")" + "Parent: " + nodes[i].parent.Level); 
                        }
                        //file.WriteLine(i + " is " + nodes[i].Level + " (" + nodes[i].Area + ")" + "Varioation: " + nodes[i].Variation);
                    }

                    //file.WriteLine("PARE:");

                    //for (int i = 0; i < Length; i++)
                    //{
                    //    file.WriteLine(i + " is a child of " + QTreeFind(i));
                    //}

                    file.WriteLine();
                    file.WriteLine();
                }
            }


            return "Root: " + root.ToString() + "Area: " + nodes[root].Area.ToString();
        }
   
        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}