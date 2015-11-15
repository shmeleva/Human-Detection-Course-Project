using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetection
{
    enum Multiplicator
    {
        Parent = 2,
        This = 1,
        Child = 0
    };

    class RegionTree
    {
        #region Приватные поля

        private ConnectedComponents helper;

        private const Int32 delta = 4;
        private const Int32 h = 1;
        private const Int32 startingLevel = 0;

        #endregion

        #region Публичные свойства

        public Region Root { get; private set; }
        public List<Rectangle> Stable { get; private set; }

        //System.IO.StreamWriter file;
        #endregion

        #region Конструкторы

        public RegionTree(Bitmap original)
        {
            this.Root = new Region(new GrayscaleBitmap(original), new Rectangle(0, 0, original.Width, original.Height), 0, original.Width * original.Height);

            Initialize();
        }

        public RegionTree(Region root)
        {
            this.Root = root;

            Initialize();
        }

        #endregion

        #region Приватные методы

        private void Initialize()
        {
            Stable = new List<Rectangle>();

            helper = new ConnectedComponents();

            //file = new System.IO.StreamWriter("C:\\Users\\Rina\\Desktop\\Tree.txt");
            Construct(Root);
        }

        private void Construct(Region region)
        {
            try
            {
                if (region.Level >= startingLevel + 2 * delta * h)
                {
                    QRegion(region, delta, Multiplicator.This).Variation = Variation(region);

                }

                if (region.Level >= startingLevel + 2 * (delta + 1) * h)
                {
                    if (QRegion(region, 1, Multiplicator.This).Stable = Stability(region))
                    {
                        Stable.Add(QRegion(region, 1, Multiplicator.This).Location); //Добавляем его в список стабильных
                        return;
                    }
                }
            }
            catch (NullReferenceException) { }

            region.Children = helper.Process(region, h);

            //file.WriteLine(region.Area + region.Location.ToString() + ":");
            //foreach (Region child in region.Children)
            //{
            //    file.WriteLine(child.Area + child.Location.ToString());
            //}
            //file.WriteLine();

            foreach (Region child in region.Children)
            {
                Construct(child);
            }
        }


        private Region QRegion(Region r, Int32 step, Multiplicator m)
        {
            Region next = r;

            for (Int32 i = 0; i < (Int32)m * step; i++)
            {
                if ((next = r.Parent) == null)
                {
                    throw new NullReferenceException();
                }
            }
            
            return next;
        }
        
        private Double Variation(Region r)
        {
            //return (QRegion(r, delta, Multiplicator.Parent).Area - QRegion(r, delta, Multiplicator.Child).Area) / QRegion(r, delta, Multiplicator.This).Area;
            return ((Double)QRegion(r, delta, Multiplicator.Parent).Area - (Double)QRegion(r, delta, Multiplicator.This).Area) / (Double)QRegion(r, delta, Multiplicator.This).Area;
        
        }

        private Boolean Stability(Region r)
        {
            if (Math.Min(QRegion(r, 1, Multiplicator.Parent).Variation, QRegion(r, 1, Multiplicator.Child).Variation) >= QRegion(r, 1, Multiplicator.This).Variation)
            {
                //Stable.Add(r);(В другом месте?)
                //StableRectangle.Add(r.Location);
                //return (r.Stable = true);
                return true;
            }
            return false;
        }


        #region Хрень

        private Int32 Area(Region r, Int32 multiplicator)
        {
            Region next = r;

            for (int i = 0; i < multiplicator*delta; i++)
            {
                if ((next = r.Parent) == null)
                {
                    throw new NullReferenceException();
                }
            }

            return next.Area;
        }

        private Int32 Area(Region r, Multiplicator m)
        {
            Region next = r;

            for (int i = 0; i < (Int32)m * delta; i++)
            {
                if ((next = r.Parent) == null)
                {
                    throw new NullReferenceException();
                }
            }

            return next.Area;
        }

        //Вычисление вариации для Q(i). Вариация для Q(i - delta) или r пока не известна.
        private Double CVariation(Region r, Boolean i)
        {
            return ((Area(r, 2) - Area(r, 0)) / Area(r, 1));
        }

        // multiplicator == 0: Q(i - delta) (или r)
        // multiplicator == 1: Q(i) 
        // multiplicator == 2: Q(i + delta) 
        private Double CVariation(Region r, Int32 multiplicator)
        {
            Region next = r;

            for (int i = 0; i < multiplicator*delta; i++)
            {
                if ((next = r.Parent) == null)
                {
                    throw new NullReferenceException();
                }
            }

            return next.Variation;
        }


        //public Bitmap DrawTree()
        //{
        //    return DrawTree(Root);
        //}
        //private Bitmap DrawTree(Region region)
        //{
        //    Bitmap result = new Bitmap(Root.GrayscaleMask);

        //    DrawRegion(region, result);

        //    if (region.Children == null)
        //    {
        //        return result;
        //    }

        //    for (int i = 0; i < region.Children.Count; i++)
        //    {
        //        DrawTree(region.Children[i]);
        //    }

        //    if (region.Parent == null)
        //    {
        //        return result;
        //    }

        //    return result;
        //}
        //private void DrawRegion(Region region, Bitmap result)
        //{
        //    for (int i = region.Location.X; i < region.Location.X + region.Location.Width - 1; i++)
        //    {
        //        result.SetPixel(i, region.Location.Y, Color.Green);
        //        result.SetPixel(i, region.Location.Y + region.Location.Height - 1, Color.Green);
        //    }
        //    for (int i = region.Location.Y; i < region.Location.Y + region.Location.Height - 1; i++)
        //    {
        //        result.SetPixel(region.Location.X, i, Color.Green);
        //        result.SetPixel(region.Location.X + region.Location.Width - 1, i, Color.Green);
        //    }
        //}
        //private void SaveRegion(Region region, int number)
        //{
        //    using (System.IO.FileStream f = File.Create("C:\\Users\\Rina\\Desktop\\Tree\\" + region.Level.ToString() + "_" + number.ToString() + region.Area.ToString() + ".jpeg"))
        //    {
        //        region.Mask.Save(f, System.Drawing.Imaging.ImageFormat.Jpeg);
        //    }
        //}

        #endregion

        #endregion
    }
}
