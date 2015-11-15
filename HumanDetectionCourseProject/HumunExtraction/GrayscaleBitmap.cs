using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    /*
     * Инкапсулирует полутоновый точечный рисунок.
     */
    public class GrayscaleBitmap : IEnumerable<Byte>
    {
        #region Приватные поля

        private Byte[,] bitmap;

        #endregion

        #region Публичные свойства

        public Int32 Width
        {
            get
            {
                return bitmap.GetLength(0);
            }
        }
        public Int32 Height
        {
            get
            {
                return bitmap.GetLength(1);
            }
        }

        public Int32 Perimeter
        {
            get
            {
                Int32 p = 0;

                foreach (var elem in this.Gaussian().Sobel())
                {
                    if (elem == Byte.MinValue) p++;
                }

                return p;
            }
        }
        public Double Dispersiveness
        {
            get
            {
                return Math.Pow(Perimeter, 2) / (Width * Height);
            }
        }

        public Byte this[int i, int j]
        {
            get
            {
                return bitmap[i, j];
            }
            set
            {
                bitmap[i, j] = value;
            }
        }

        #endregion

        #region Конструкторы

        public GrayscaleBitmap(Int32 width, Int32 height)
        {
            bitmap = new Byte[width, height];
        }

        public GrayscaleBitmap(Bitmap original)
        {
            bitmap = new Byte[original.Width, original.Height];

            var newBitmap = new Bitmap(original.Width, original.Height);
            Graphics g = Graphics.FromImage(newBitmap);

            /*
             *  ColorMatrix определяет матрицу 5 x 5, которая содержит координаты для пространства RGBAW.
             *  Несколько методов класса ImageAttributes настраивают цвета изображения с помощью цветовой матрицы.
             *  Y' = 0.299R + 0.587G + 0.114B
             */
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
               {
                   new float[] {.299f,  .299f,  .299f,  0,  0},
                   new float[] {.587f,  .587f,  .587f,  0,  0},
                   new float[] {.114f,  .114f,  .114f,  0,  0},
                   new float[] {0,      0,      0,      1,  0},
                   new float[] {0,      0,      0,      0,  1}
               });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0,
                original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            for (Int32 i = 0; i < Width; i++)
            {
                for (Int32 j = 0; j < Height; j++)
                {
                    bitmap[i, j] = original.GetPixel(i, j).R;
                }
            }
        }

        #endregion

        #region Приватные методы

        /*
         * 1. Фильтр кубического размытия 5x5(аппроксимирует фильтр Гаусса);
         * 2. Фильтр Гаусса 5x5;
         * 3. Оператор Собеля;
         * 4. Оператор Кэннии (использует фильтр Гаусса и оператор Собеля).
         * 
         * Методы возвращают новую GrayscaleBitmap.
         */
        private GrayscaleBitmap BoxBlur()
        {
            /* Матрицей преобразования является матрица 5x5, все элементы которой - единицы, 
             * поэтому матрицу мы не храним. */

            GrayscaleBitmap result = new GrayscaleBitmap(Width, Height);

            const Int32 offset = 2;
            const Double factor = 1.0 / 25.0;
            Double color;

            for (int offsetY = offset; offsetY < Height - offset; offsetY++)
            {
                for (int offsetX = offset; offsetX < Width - offset; offsetX++)
                {
                    color = 0;

                    for (int filterY = -offset; filterY <= offset; filterY++)
                    {
                        for (int filterX = -offset; filterX <= offset; filterX++)
                        {
                            color += (Double)(bitmap[offsetX + filterX, offsetY + filterY]);
                        }
                    }
                    color *= factor;
                    result[offsetX, offsetY] = (Byte)(color > 255 ? 255 : (color < 0 ? 0 : color));
                }
            }
            return result;
        }
        public GrayscaleBitmap Gaussian()
        {
            GrayscaleBitmap result = new GrayscaleBitmap(Width, Height);

            Double[,] filterMatrix = new Double[,]
            {
                { 2, 4, 5, 4, 2 },
                { 4, 9, 12, 9, 4 },
                { 5, 12, 15, 12, 5 },
                { 4, 9, 12, 9, 4 },
                { 2, 4, 5, 4, 2 }
            };

            const Int32 offset = 2;
            const Double factor = 1.0 / 159.0;
            Double color;

            for (int offsetY = offset; offsetY < Height - offset; offsetY++)
            {
                for (int offsetX = offset; offsetX < Width - offset; offsetX++)
                {
                    color = 0;

                    for (int filterY = -offset; filterY <= offset; filterY++)
                    {
                        for (int filterX = -offset; filterX <= offset; filterX++)
                        {
                            color += (Double)(bitmap[offsetX + filterX, offsetY + filterY]) *
                                      filterMatrix[filterY + offset, filterX + offset];
                        }
                    }
                    color *= factor;
                    result[offsetX, offsetY] = (Byte)(color > 255 ? 255 : (color < 0 ? 0 : color));
                }
            }
            return result;
        }

        private GrayscaleBitmap Sobel()
        {
            Int32[,] xFilterMatrix = new Int32[,]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };
            Int32[,] yFilterMatrix = new Int32[,]
            {
                {  1,  2,  1 },
                {  0,  0,  0 },
                { -1, -2, -1 }
            };

            GrayscaleBitmap result = new GrayscaleBitmap(Width, Height);

            const Int32 offset = 1;
            Double x, y;

            for (int offsetY = offset; offsetY < Height - offset; offsetY++)
            {
                for (int offsetX = offset; offsetX < Width - offset; offsetX++)
                {
                    x = y = 0;

                    for (int filterY = -offset; filterY <= offset; filterY++)
                    {
                        for (int filterX = -offset; filterX <= offset; filterX++)
                        {
                            x += (Double)(bitmap[offsetX + filterX, offsetY + filterY]) *
                                      xFilterMatrix[filterY + offset, filterX + offset];

                            y += (Double)(bitmap[offsetX + filterX, offsetY + filterY]) *
                                      yFilterMatrix[filterY + offset, filterX + offset];
                        }
                    }

                    result[offsetX, offsetY] = (Byte)(Math.Sqrt((x * x) + (y * y)) > 128 ? 0 : 255);
                }
            }
            return result;
        }
        public GrayscaleBitmap Canny()
        {
            Int32[,] xFilterMatrix = new Int32[,]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };
            Int32[,] yFilterMatrix = new Int32[,]
            {
                {  1,  2,  1 },
                {  0,  0,  0 },
                { -1, -2, -1 }
            };

            GrayscaleBitmap Gradient = new GrayscaleBitmap(Width, Height);
            GrayscaleBitmap DerivativeX = new GrayscaleBitmap(Width, Height);
            GrayscaleBitmap DerivativeY = new GrayscaleBitmap(Width, Height);

            const Int32 offset = 1;
            Double x, y;

            for (int offsetY = offset; offsetY < Height - offset; offsetY++)
            {
                for (int offsetX = offset; offsetX < Width - offset; offsetX++)
                {
                    x = y = 0;

                    for (int filterY = -offset; filterY <= offset; filterY++)
                    {
                        for (int filterX = -offset; filterX <= offset; filterX++)
                        {
                            x += (Double)(bitmap[offsetX + filterX, offsetY + filterY]) *
                                      xFilterMatrix[filterY + offset, filterX + offset];

                            y += (Double)(bitmap[offsetX + filterX, offsetY + filterY]) *
                                      yFilterMatrix[filterY + offset, filterX + offset];
                        }
                    }

                    DerivativeX[offsetX, offsetY] = (Byte)(x > 128 ? 0 : 255);
                    DerivativeY[offsetX, offsetY] = (Byte)(y > 128 ? 0 : 255);

                    Gradient[offsetX, offsetY] = (Byte)(Math.Sqrt((x * x) + (y * y)) > 255 ? 255 : (Math.Sqrt((x * x) + (y * y)) < 0 ? 0 : Math.Sqrt((x * x) + (y * y))));
                }
            }

            GrayscaleBitmap NonMax = new GrayscaleBitmap(Width, Height);

            for (int i = 0; i <= (Width - 1); i++)
            {
                for (int j = 0; j <= (Height - 1); j++)
                {
                    NonMax[i, j] = Gradient[i, j];
                }
            }

            Double Tangent;

            for (int i = offset; i <= (Width - offset) - 1; i++)
            {
                for (int j = offset; j <= (Height - offset) - 1; j++)
                {

                    if (DerivativeX[i, j] == 0)
                    {
                        Tangent = 90.0;
                    }
                    else
                    {
                        /* Приведение радиан к градусам */
                        Tangent = (Double)(Math.Atan(DerivativeY[i, j] / DerivativeX[i, j]) * 180 / Math.PI);
                    }



                    /* Горизонтальный угол */
                    if (((-22.5 < Tangent) && (Tangent <= 22.5)) || ((157.5 < Tangent) && (Tangent <= -157.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i, j + 1]) || (Gradient[i, j] < Gradient[i, j - 1]))
                            NonMax[i, j] = 0;
                    }


                    /* Верикальный угол */
                    if (((-112.5 < Tangent) && (Tangent <= -67.5)) || ((67.5 < Tangent) && (Tangent <= 112.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j]) || (Gradient[i, j] < Gradient[i - 1, j]))
                            NonMax[i, j] = 0;
                    }

                    /* +45 градусов */
                    if (((-67.5 < Tangent) && (Tangent <= -22.5)) || ((112.5 < Tangent) && (Tangent <= 157.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j - 1]) || (Gradient[i, j] < Gradient[i - 1, j + 1]))
                            NonMax[i, j] = 0;
                    }

                    /* -45 градусов */
                    if (((-157.5 < Tangent) && (Tangent <= -112.5)) || ((67.5 < Tangent) && (Tangent <= 22.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j + 1]) || (Gradient[i, j] < Gradient[i - 1, j - 1]))
                            NonMax[i, j] = 0;
                    }

                }
            }

            /* Пиксели возвращаются на матрицу, число граничных (белых) пикселей сохраняется в perimeter */
            for (int i = 0; i <= (Width - 1); i++)
            {
                for (int j = 0; j <= (Height - 1); j++)
                {
                    NonMax[i, j] = (Byte)(NonMax[i, j] < 128 ? Byte.MinValue : Byte.MaxValue);
                }
            }

            return NonMax;
        }

        #endregion

        #region Публичные методы

        /*
         * Преобразует GrayscaleBitmap к полутоновой Bitmap.
         * Если GrayscaleBitmap - пустое изображение,
         * возвращает null.
         */
        public Bitmap ToBitmap()
        {
            if (Math.Min(Width, Height) == 0) return null;

            Bitmap result = new Bitmap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    result.SetPixel(i, j, Color.FromArgb(bitmap[i, j], bitmap[i, j], bitmap[i, j]));
                }
            }
            return result;
        }

        /* Модифицированная сортировка подсчетом в прямом и обратном порядке. */
        public Pixel[] CountingSort()
        {
            Int32[] count = new Int32[Byte.MaxValue - Byte.MinValue + 1];
            Int32[] offset = new Int32[Byte.MaxValue - Byte.MinValue + 1];

            foreach (var elem in bitmap)
            {
                count[elem - Byte.MinValue]++;
            }

            offset[0] = 0;
            for (int i = 1; i < count.Length; i++)
            {
                offset[i] = offset[i - 1] + count[i - 1];
            }

            Pixel[] result = new Pixel[bitmap.Length];

            for (int x = 0; x < bitmap.GetLength(0); x++)
            {
                for (int y = 0; y < bitmap.GetLength(1); y++)
                {
                    result[offset[bitmap[x, y]]] = new Pixel(new Point(x, y), bitmap[x, y]);
                    offset[bitmap[x, y]]++;
                }
            }            
            return result;
        }
        public Pixel[] DecreasingCountingSort()
        {
            Int32[] count = new Int32[Byte.MaxValue - Byte.MinValue + 1];
            Int32[] offset = new Int32[Byte.MaxValue - Byte.MinValue + 1];

            foreach (var elem in bitmap)
            {
                count[elem - Byte.MinValue]++;
            }

            offset[count.Length - 1] = 0;
            for (int i = count.Length - 1; i > 0; i--)
            {
                offset[i - 1] = offset[i] + count[i];
            }

            Pixel[] result = new Pixel[bitmap.Length];

            for (int x = bitmap.GetLength(0) - 1; x >= 0; x--)
            {
                for (int y = bitmap.GetLength(1) - 1; y >= 0; y--)
                {
                    result[offset[bitmap[x, y]]] = new Pixel(new Point(x, y), bitmap[x, y]);
                    offset[bitmap[x, y]]++;
                }
            }

            return result;
        }

        /* Возвращает GrayscaleBitmap, представляющий собой фрагмент изображения,
         * ограниченный прямоугольником location. Если прямоугольник выходит за границы 
         * изображение, возвращается только та его часть, что входит в изображение.
         * Если у прямоугольника и изображения нет точек пересечения, позвращается пустой
         * Rectangle.
         */
        public GrayscaleBitmap Crop(Rectangle location)
        {
            location.Intersect(new Rectangle(0, 0, this.Width, this.Height));

            GrayscaleBitmap cropped = new GrayscaleBitmap(location.Width, location.Height);

            for (int i = 0; i < cropped.Width; i++)
            {
                for (int j = 0; j < cropped.Height; j++)
                {
                    cropped[i, j] = bitmap[i + location.X, j + location.Y];
                }
            }
            
            return cropped;
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator<Byte> IEnumerable<Byte>.GetEnumerator()
        {
            for (Int32 i = 0; i < Width; i++)
            {
                for (Int32 j = 0; j < Height; j++)
                {
                    yield return bitmap[i, j];
                }
            }
        }

        #endregion
    }

}
