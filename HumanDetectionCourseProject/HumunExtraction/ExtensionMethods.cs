using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    public static class ExtensionMethods
    {
        public static Bitmap Grayscale(this Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);
            /*
             * GDI+ — это интерфейс Windows для представления графических объектов и передачи их на устройства
             * отображения, такие как мониторы и принтеры.
             * Перед тем как рисовать линии и фигуры, отображать текст, выводить изображения и управлять ими
             * в GDI+ необходимо создать объект Graphics. Объект Graphics представляет поверхность рисования GDI+
             * и используется для создания графических изображений.
             * Объект Graphics можно создать из любого объекта, производного от класса Image. 
             */
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

            /*
             * Класс ImageAttributes содержит сведения о том, каким образом обрабатываются цвета
             * точечных рисунков и метафайлов во время отрисовки.
             * SetColorMatrix задает матрицу настройки цвета.
             */
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            /*
             * 1.   Объект Image для рисования.
             * 2.   Структура Rectangle, которая задает расположение и размер создаваемого изображения.
             *      (Rectangle содержит набор из четырех целых чисел, определяющих расположение и размер прямоугольника.)
             *      Изображение масштабируется по размерам прямоугольника.
             * 3.   Координата X верхнего левого угла отображаемой части исходного изображения. 
             * 4.   Координата Y верхнего левого угла отображаемой части исходного изображения. 
             * 5.   Ширина отображаемой части исходного изображения. 
             * 6.   Высота отображаемой части исходного изображения. 
             * 7.   Член перечисления GraphicsUnit, задающий единицу измерения,
             *      используемую для определения исходного прямоугольника.
             *      (GraphicsUnit.Pixel	задает в качестве единицы измерения пиксель устройства.)
             * 8.   Атрибуты ImageAttributes, содержащие сведения о гамме и изменении цвета объекта image. 
            */
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0,
                original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            return newBitmap;
        }
        public static Bitmap Blur(this Bitmap original)
        {
            Bitmap sourceBitmap = original;
            double factor = 1.0 / 25.0;
            int bias = 0;

            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                                                          ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            double color;

            int filterOffset = 2;
            int calcOffset = 0;
            int byteOffset = 0;
            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    color = 0;
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);
                            color += (double)(pixelBuffer[calcOffset]);
                        }
                    }
                    color = factor * color + bias;
                    color = (color > 255 ? 255 : (color < 0 ? 0 : color));

                    resultBuffer[byteOffset] = resultBuffer[byteOffset + 1] = resultBuffer[byteOffset + 2] = (byte)color;
                    resultBuffer[byteOffset + 3] = (byte)255;
                }
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                                                          ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }
        public static Bitmap Sobel(this Bitmap original)
        {
            int[,] xFilterMatrix = new int[,]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };
            int[,] yFilterMatrix = new int[,]
            {
                { 1, 2, 1 },
                { 0, 0, 0 },
                { -1, -2, -1 }
            };

            BitmapData sourceData = original.LockBits(new Rectangle(0, 0, original.Width, original.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            original.UnlockBits(sourceData);

            double x = 0, y = 0, total = 0;

            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;

            for (int offsetY = filterOffset; offsetY < original.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < original.Width - filterOffset; offsetX++)
                {
                    x = y = total = 0;

                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            x += (double)(pixelBuffer[calcOffset]) *
                                      xFilterMatrix[filterY + filterOffset, filterX + filterOffset];

                            y += (double)(pixelBuffer[calcOffset]) *
                                      yFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    total = Math.Sqrt((x * x) + (y * y));
                    total = (total > 128 ? 255 : 0);

                    resultBuffer[byteOffset] = resultBuffer[byteOffset + 1] = resultBuffer[byteOffset + 2] = (byte)(total);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            BitmapData resultData = newBitmap.LockBits(new Rectangle(0, 0,
                                     newBitmap.Width, newBitmap.Height),
                                                      ImageLockMode.WriteOnly,
                                                  PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            
            newBitmap.UnlockBits(resultData);

            return newBitmap;
        }

        public static Bitmap Fit(this Bitmap original, UInt32 width, UInt32 height)
        {
            if (original.Width > original.Height)
                return original.FitHorisontal(width, height);

            if (original.Width < original.Height)
                return original.FitVertical(width, height);

            return original.FitSquare(Math.Min(width, height));
        }
        public static Bitmap FitHorisontal(this Bitmap original, UInt32 width, UInt32 height)
        {

            return new Bitmap(original);
        }
        public static Bitmap FitVertical(this Bitmap original, UInt32 width, UInt32 height)
        {


            return new Bitmap(original);
        }
        public static Bitmap FitSquare(this Bitmap original, UInt32 width)
        {


            return new Bitmap(original);
        }

        public static Byte[,] GrayscaleToByte(this Bitmap original)
        {
            Byte[,] result = new Byte[original.Width, original.Height];
            for (int i = 0; i < original.Height; i++)
            {
                for (int j = 0; j < original.Width; j++)
                {
                    //result[i * original.Height + j] = (original.GetPixel(i ,j).R);
                    result[i, j] = (original.GetPixel(i, j).R);
                }
            }
            return result;
        }

        public static Bitmap Detect(this Bitmap original)
        {
            //Int32 m = original.Width, n = original.Height;
            //Int32 km = 0, kn = 0;



            //return new Bitmap(m, n);
            return null;
        }
    }
}


        //public static Bitmap Detect(this Bitmap original)
        //{
        //    //Boolean[,] labels = new Boolean[original.Width, original.Height];
        //    Int32[,] labels = new Int32[original.Width, original.Height];
        //    Int32 l = 1;
        //    Bitmap test = new Bitmap(original.Width, original.Height);

        //    for (int y = 0; y < original.Height; y++)
        //        for (int x = 0; x < original.Width; x++)
        //            original.Fill(test, labels, x, y, l++);

        //    return test;
        //}
        //private static void Fill(this Bitmap original, Bitmap result, Int32[,] labels, Int32 x, Int32 y, Int32 l)
        //{
        //    if (labels[x, y] == 0 && original.GetPixel(x, y).R != 0)
        //    {
        //        labels[x, y] = l;
        //        //result.SetPixel(x, y, Color.Beige);

        //        if (x > 0)
        //            original.Fill(result, labels, x - 1, y, l);

        //        if (x < original.Width - 1)
        //            original.Fill(result, labels, x + 1, y, l);

        //        if (y > 0)
        //            original.Fill(result, labels, x, y - 1, l);

        //        if (y < original.Height - 1)
        //            original.Fill(result, labels, x, y + 1, l);
        //    }
        //}