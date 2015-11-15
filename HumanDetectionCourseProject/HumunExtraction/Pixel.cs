using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace HumanDetection
{
    public class Pixel
    {
        #region Публичные свойства

        public Point Position { get; set; }
        public Byte color { get; set; } 

        #endregion

        #region Конструктор

        public Pixel(Point Position, Byte color)
        {
            this.Position = Position;
            this.color = color;
        }

        #endregion

        public override string ToString()
        {
            return "Color: " + color.ToString() + " Position: (" + Position.X + ", " + Position.Y + ")";
        }
    }
}