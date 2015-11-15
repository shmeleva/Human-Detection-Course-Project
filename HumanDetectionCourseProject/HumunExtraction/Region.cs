using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    public class Region
    {
        #region Публичные свойства

        public Region Parent { get; set; }

        public Byte Level { get; private set; }
        public Rectangle Location { get; private set; }
        public Int32 Area { get; private set; }

        public Double Variation { get; set; }
        public Boolean Stable { get; set; }

        public GrayscaleBitmap GrayscaleMask { get; set; }

        public List<Region> Children { get; set; }

        #endregion

        #region Конструкторы

        public Region(GrayscaleBitmap mask, Rectangle location, Int32 level, Int32 area, Region parent = null, List<Region> children = null, Boolean stable = false)
        {
            this.GrayscaleMask = mask;
            this.Location = location;
            this.Level = (byte)level;
            this.Area = area;
            this.Variation = -1.0;
            this.Stable = stable;

            this.Parent = parent;
            this.Children = children;
        }

        #endregion
    }
}
