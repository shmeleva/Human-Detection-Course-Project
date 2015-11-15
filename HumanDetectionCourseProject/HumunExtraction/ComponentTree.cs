using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    class ComponentTree
    {
        #region Приватные поля

        Pixel[] pixels;         /* Отсортированный по интенсивности массив пикселей
                                * (в обратном порядке, от 255 к 0) */

        DisjointSet QTree;
        DisjointSet QNode;

        Int32[] lowestNode;

        Node[] nodes;

        Int32[,] accessible;    /* Массив доступных пикселей.
                                * (Пиксель становится доступным после его обработки.
                                * На поле accessible записывается порядковый номер пикселя
                                * в массиве pixels на Position пикселя на original.) */

        Int32 root;             /* Номер корня дерева. */

        List<Node> stable;      /* Максимально стабильные области экстремума */

        #endregion

        #region Публичные свойства

        public Int32 Length
        {
            get
            {
                return pixels.Length;
            }
        }

        public List<Rectangle> Stable
        {
            get
            {
                List<Rectangle> result = new List<Rectangle>(stable.Count);

                foreach (var region in stable)
                {
                    result.Add(region.ToRectangle());
                }

                return result;
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

            QNode = new DisjointSet(Length);
            QTree = new DisjointSet(Length);

            nodes = new Node[Length];
            lowestNode = new Int32[Length];

            stable = new List<Node>();

            MSERDetector();
        }

        #endregion

        #region Система непересекающихся множеств (Union-Find Algorithm)

        Int32 MergeNodes(Int32 node1, Int32 node2)
        {
            Int32 tmpNode = QNode.Link(node1, node2), tmpNode2;


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

        void DisjointSetForest()
        {
            /* 2. foreach p из V do {MakeSettree(p); MakeSetnode(p);
             * nodes[p]:= MakeNode(F(p)); lowestNode[p] := p;}; */

            for (Int32 p = 0; p < Length; p++)
            {
                QTree.MakeSet(p);
                QNode.MakeSet(p);

                /* MakeNode() */
                nodes[p] = new Node(pixels[p]);

                lowestNode[p] = p;
            }

            /* 3. foreach p из V in decreasing order of level for F
             * (F - интенсивность) do */
            for (Int32 p = 0; p < Length; p++)
            {
                accessible[pixels[p].Position.X, pixels[p].Position.Y] = p;

                Int32 curTree = QTree.Find(p);
                Int32 curNode = QNode.Find(lowestNode[curTree]);

                /* 6. foreach already processed neighbor
                    * (для уже обработанных, доступных) q of p with F(q) >= F(p)  */

                List<Int32> border = AccessibleBorderPixels(pixels[p].Position);

                for (Int32 q = 0; q < border.Count; q++)
                {
                    Int32 adjTree = QTree.Find(border[q]);
                    Int32 adjNode = QNode.Find(lowestNode[adjTree]);

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

                        curTree = QTree.Link(adjTree, curTree);
                        lowestNode[curTree] = curNode;
                    }
                }
            }

            // 15. Root := lowestNode[Findtree(Findnode(0))] ;

            root = lowestNode[QTree.Find(QNode.Find(0))];
        }

        #endregion

        #region MSER Tracking

        private void MSERDetector()
        {
            DisjointSetForest();
            CalculateVariation(nodes[root]);
            MSERDetector((nodes[root]));
        }

        private void CalculateVariation(Node current)
        {
            current.CalculateVariation();

            foreach (var child in current.Children)
            {
                CalculateVariation(child);
            }
        }

        private void MSERDetector(Node current)
        {
            if ((Double)current.Area < Parameters.MinArea * (Double)Length)
            {
                return;
            }

            if ((Double)current.Area < Parameters.MaxArea * (Double)Length && current.LocalMinimum())
            {
                stable.Add(current);
                return;
            }

            foreach (var child in current.Children)
            {
                MSERDetector(child);
            }
        }

        #endregion
    }
}