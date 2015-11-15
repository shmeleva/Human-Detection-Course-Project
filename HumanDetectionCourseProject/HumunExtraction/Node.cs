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

        Node parent;
        List<Node> children;

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

        #region Конструкторы

        public Node(Pixel pixel)
        {
            (this.pixels = new List<Pixel>()).Add(pixel);

            this.parent = null;
            this.children = new List<Node>();

            this.Highest = pixel.color;
            this.Variation = Double.MaxValue;
        }

        #endregion

        #region Приватные методы

        private void ToRectangle(Node current, ref Int32 minX, ref Int32 minY, ref Int32 maxX, ref Int32 maxY)
        {
            foreach (var pixel in current.Pixels)
            {
                if (pixel.Position.X < minX) minX = pixel.Position.X;
                if (pixel.Position.Y < minY) minY = pixel.Position.Y;
                if (pixel.Position.X > maxX) maxX = pixel.Position.X;
                if (pixel.Position.Y > maxY) maxY = pixel.Position.Y;
            }

            foreach (var child in current.Children)
                ToRectangle(child, ref minX, ref minY, ref maxX, ref maxY);
        }

        #endregion

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

            while (grandparent.Level > this.Level - Parameters.Delta)
            {
                if ((grandparent = grandparent.parent) == null)
                {
                    return;
                }
            }

            Variation = (Double)(grandparent.Area - this.Area) / (Double)this.Area;
        }
        public bool LocalMinimum()
        {
            if (this.parent == null || this.children.Count == 0 || this.Variation >= this.parent.Variation)
            {
                return false;
            }

            foreach (var child in this.children)
            {
                if (this.Variation < child.Variation)
                {
                    return true;
                }
            }

            return false;
        }

        public Rectangle ToRectangle()
        {
            Int32 minX = Int32.MaxValue, minY = Int32.MaxValue, maxX = Int32.MinValue, maxY = Int32.MinValue;
            ToRectangle(this, ref minX, ref minY, ref maxX, ref maxY);
            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        #endregion

    }
}
