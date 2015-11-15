using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HumanDetection
{
    public class CCL
    {
        #region Member Variables

        //private const Double minArea = 0.0002;
        private Bitmap input;
        private Int32[,] board;
        private Int32 width;
        private Int32 height;
        private Byte level;
        private Boolean grayscale;

        #endregion

        #region Публичные методы

        public List<Region> Process(Bitmap input, Byte level, Boolean grayscale = true)
        {
            this.input = input;
            this.width = input.Width;
            this.height = input.Height;
            this.board = new int[width, height];
            this.level = level;
            this.grayscale = grayscale;

            Dictionary<int, List<Pixel>> patterns = Find();
            var regions = new List<Region>();

            foreach (KeyValuePair<int, List<Pixel>> pattern in patterns)
            {
                //Region region = CreateRegion(pattern.Value);
                //if (region != null)
                //{
                //    regions.Add(region);
                //}
                regions.Add(CreateRegion(pattern.Value));
            }

            return regions;
        }

        public List<Region> Process(Region input, Byte level, Boolean grayscale = true)
        {
            this.input = input.Mask;
            this.width = input.Mask.Width;
            this.height = input.Mask.Height;
            this.board = new int[width, height];
            this.level = level;
            this.grayscale = grayscale;

            Dictionary<int, List<Pixel>> patterns = Find();
            var regions = new List<Region>();

            foreach (KeyValuePair<int, List<Pixel>> pattern in patterns)
            {
                //Region region = CreateRegion(pattern.Value);
                //if (region != null)
                //{
                //    regions.Add(region);
                //}
                regions.Add(CreateRegion(pattern.Value));
            }

            return regions;
        }

        #endregion

        #region Приватные методы

        private bool Background(Pixel currentPixel)
        {
            return (grayscale && currentPixel.color.R < level) || (currentPixel.color.R < level && currentPixel.color.G < level && currentPixel.color.B < level);
            //return (grayscale && currentPixel.color.R == level) || (currentPixel.color.R == level && currentPixel.color.G == level && currentPixel.color.B == level);
        }

        private Dictionary<int, List<Pixel>> Find()
        {
            int labelCount = 1;
            var allLabels = new Dictionary<int, Label>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Pixel currentPixel = new Pixel(new Point(j, i), input.GetPixel(j, i));

                    if (Background(currentPixel))
                    {
                        continue;
                    }

                    IEnumerable<int> neighboringLabels = GetNeighboringLabels(currentPixel);
                    int currentLabel;

                    if (!neighboringLabels.Any()) 
                    {
                        currentLabel = labelCount;
                        allLabels.Add(currentLabel, new Label(currentLabel));
                        labelCount++;
                    }
                    else
                    {
                        currentLabel = neighboringLabels.Min(n => allLabels[n].GetRoot().Name);
                        Label root = allLabels[currentLabel].GetRoot();

                        foreach (var neighbor in neighboringLabels)
                        {
                            if (root.Name != allLabels[neighbor].GetRoot().Name)
                            {
                                allLabels[neighbor].Join(allLabels[currentLabel]);
                            }
                        }
                    }

                    board[j, i] = currentLabel;
                }
            }


            Dictionary<int, List<Pixel>> patterns = AggregatePatterns(allLabels);

            return patterns;
        }

        private IEnumerable<int> GetNeighboringLabels(Pixel pix)
        {
            var neighboringLabels = new List<int>();

            for (int i = pix.Position.Y - 1; i <= pix.Position.Y + 2 && i < height - 1; i++)
            {
                for (int j = pix.Position.X - 1; j <= pix.Position.X + 2 && j < width - 1; j++)
                {
                    if (i > -1 && j > -1 && board[j, i] != 0)
                    {
                        neighboringLabels.Add(board[j, i]);
                    }
                }
            }

            return neighboringLabels;
        }

        private Dictionary<int, List<Pixel>> AggregatePatterns(Dictionary<int, Label> allLabels)
        {
            var patterns = new Dictionary<int, List<Pixel>>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int patternNumber = board[j, i];

                    if (patternNumber != 0)
                    {
                        patternNumber = allLabels[patternNumber].GetRoot().Name;

                        if (!patterns.ContainsKey(patternNumber))
                        {
                            patterns[patternNumber] = new List<Pixel>();
                        }

                        //patterns[patternNumber].Add(new Pixel(new Point(j, i), Color.Black)); //!!! СОХРАНЕНИЕ ЦВЕТА
                        patterns[patternNumber].Add(new Pixel(new Point(j, i), input.GetPixel(j, i))); 
                    }
                }
            }

            return patterns;
        }

        private Region CreateRegion(List<Pixel> pattern)
        {
            //if (pattern.Count < width * height * minArea)
            //{
            //    return null;
            //}

            int minX = pattern.Min(p => p.Position.X);
            int maxX = pattern.Max(p => p.Position.X);

            int minY = pattern.Min(p => p.Position.Y);
            int maxY = pattern.Max(p => p.Position.Y);

            int regionWidth = maxX + 1 - minX;
            int regionHeight = maxY + 1 - minY;

            var bmp = new Bitmap(regionWidth, regionHeight);

            foreach (Pixel pix in pattern)
            {
                bmp.SetPixel(pix.Position.X - minX, pix.Position.Y - minY, pix.color);
            }

            return new Region(bmp, new Rectangle(minX, minY, regionWidth, regionHeight), 0, pattern.Count);
            //+Parent, null
        }

        #endregion
    }
}
