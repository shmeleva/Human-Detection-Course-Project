using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    class HumanDetector
    {
        GrayscaleBitmap GrayscaleIR;
        Bitmap IR;
        Bitmap LIDAR;

        List<Rectangle> objects;

        public HumanDetector(Image IR, Image LIDAR)
        {
            /* 1.   Находим максимально стабильные области экстремума */
            /* 2.   Масштабируем найденные области в зависимости от расстояния до объектов.
             *      Слишком удаленные объекты отбрасываем. */

            //objects = new ComponentTree(this.GrayscaleIR = new GrayscaleBitmap(this.IR = new Bitmap(IR))).Stable;
            objects = (new Lidar(this.LIDAR = new Bitmap(LIDAR), (new ComponentTree(this.GrayscaleIR = new GrayscaleBitmap(this.IR = new Bitmap(IR)))).Stable)).Resized;
            
            /* 3.   Фильтруем регионы по значению dispersiveness и отсеиваем пересекающиеся.
             *      Все регионы, с dispersiveness ниже заданного порога, отбрасываются. */

            objects = TargetClassificator();
        }

        public Bitmap Draw()
        {
            var result = new Bitmap(IR);

            foreach (var region in objects)
            {
                for (int i = region.Location.X; i < region.Location.X + region.Width - 1; i++)
                {
                    result.SetPixel(i, region.Location.Y, Color.Orange);
                    result.SetPixel(i, region.Location.Y + region.Height - 1, Color.Orange);
                }
                for (int i = region.Location.Y; i < region.Location.Y + region.Height - 1; i++)
                {
                    result.SetPixel(region.Location.X, i, Color.Orange);
                    result.SetPixel(region.Location.X + region.Width - 1, i, Color.Orange);
                }
            }

            return result;
        }

        private List<Rectangle> TargetClassificator()
        {
            InsertionSort();

            List<Rectangle> result = new List<Rectangle>();

            if (objects.Count != 0 && GrayscaleIR.Crop(objects[0]).Dispersiveness > Parameters.Dispersiveness)
            {
                result.Add(objects[0]);

                for (int i = 1; i < objects.Count; i++)
                {
                    if (GrayscaleIR.Crop(objects[i]).Dispersiveness < Parameters.Dispersiveness) break;
                    if (TargetClassificator(objects[i], result)) result.Add(objects[i]); 
                }
            }

            return result;
        }
        private bool TargetClassificator(Rectangle rect, List<Rectangle> result)
        {
            for (int j = 0; j < result.Count; j++)
            {
                if (result[j].IntersectsWith(rect))
                {
                    return false;
                }
            }
            return true;
        }

        private void InsertionSort()
        {
            Rectangle temp;

            Int32 i, j;

            for (i = 1; i < objects.Count; ++i)
            {
                temp = objects[i];

                for (j = i - 1; j >= 0; --j)
                {
                    if (GrayscaleIR.Crop(temp).Dispersiveness > GrayscaleIR.Crop(objects[j]).Dispersiveness)
                    {
                        objects[j + 1] = objects[j];
                    }
                    else
                    {
                        break;
                    }
                }

                objects[j + 1] = temp;
            }
        }
    }
}
