using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    class Lidar
    {
        Bitmap lidar;
        List<Rectangle> objects;

        /* Возвращает список масштабированных областей, исключая отдаленные области */
        public List<Rectangle> Resized
        {
            get
            {
                var resized = new List<Rectangle>(objects.Count);

                Int32 height;

                foreach (Rectangle rect in objects)
                {
                    if ((height = Height(lidar.GetPixel(Center(rect).X, Center(rect).Y))) > 0)
                    {
                        resized.Add(Rectangle.Intersect(Resize(rect, height / 2, height), (new Rectangle(0, 0, lidar.Width, lidar.Height))));
                    }
                }

                return resized;
            }
        }

        public Lidar(Bitmap lidar, List<Rectangle> objects)
        {
            this.lidar = new Bitmap(lidar);
            this.objects = new List<Rectangle>(objects);
        }

        /* Возвращает порядковый номер (от 0 до 1020) цвета color в спектре.
         * Для неспектральных цветов возвращает -1. */
        static Int32 SpectralColorOrdinalNumber(Color color)
        {
            Int32 min = Math.Min(Math.Min(color.R, color.G), color.B);
            Int32 max = Math.Max(Math.Max(color.R, color.G), color.B);

            if (color.R == max && color.B == min)
            {
                return color.G;
            }

            if (color.G == max)
            {
                Int32 count = 255;

                if (color.B == min)
                {
                    return (count + Byte.MaxValue - color.R);
                }

                if (color.R == min)
                {
                    return (count + 255 + color.B);
                }
            }

            if (color.R == min && color.B == max)
            {
                return (3 * 255 + Byte.MaxValue - color.G);
            }

            return -1;
        }

        /* Возвращает расстояние до объекта цвета color в метрах.
         * В случае, если цвет объекта неспектральный, возвращает 
         * отрицательное число. */
        static Double SpectralColorToDistance(Color color)
        {
            return SpectralColorOrdinalNumber(color) / (1020 / Parameters.Range); //meters
        }

        static Point Center(Rectangle rect)
        {
            return new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        static Int32 Height(Color color)
        {
            Double distance = (SpectralColorToDistance(color));
            return (distance < Parameters.LimitRange ? (Int32)((Parameters.ObjectHeight * Parameters.FocalLengthEquivalent) / distance) : 0);
        }

        static Rectangle Resize(Rectangle rect, Int32 width, Int32 height)
        {
            return new Rectangle(rect.X - (width - rect.Width) / 2, rect.Y, width, height);
        }
    }
}
