using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    class RegionList
    {
        List<Rectangle> regions;
        Size size;

        public RegionList(List<Rectangle> regions, Size size)
        {
            this.regions = new List<Rectangle>(regions);
            this.size = size;
        }

        public List<Rectangle> IntersectingRegionsFilter()
        {
            List<Rectangle> intersecting = InsertionSort();

            List<Rectangle> result = new List<Rectangle>();

            if (regions.Count != 0)
            {
                result.Add(regions[0]);
            }

            for (int i = 1; i < regions.Count; i++)
            {
                if (IntersectingRegionsFilterCheck(regions[i], result))
                {
                    result.Add(regions[i]);
                }
            }

            return result;
        }

        public static bool IntersectingRegionsFilterCheck(Rectangle check, List<Rectangle> result)
        {
            for (int j = 0; j < result.Count; j++)
            {
                if (result[j].IntersectsWith(check))
                {
                    return false;
                }
            }
            return true;
        }

        private List<Rectangle> TargetClassificator(List<Rectangle> overlapping)
        {
            List<Rectangle> result = new List<Rectangle>();


            return result;
        }

        public List<Rectangle> InsertionSort()
        {
            List<Rectangle> result = new List<Rectangle>(regions);

            Rectangle temp;

            Int32 i, j;

            for (i = 1; i < regions.Count; ++i)
            {
                temp = regions[i];

                for (j = i - 1; j >= 0; --j)
                {
                    if (Index(temp) < Index(regions[j]))
                    {
                        regions[j + 1] = regions[j];
                    }
                    else
                    {
                        break;
                    }
                }

                regions[j + 1] = temp;
            }

            return result;
        }

        public Int32 Index(Rectangle region)
        {
            return region.Y * size.Width + region.X;
        }

        public static Int32 Area(Rectangle region)
        {
            return region.Width * region.Height;
        }
    }
}
