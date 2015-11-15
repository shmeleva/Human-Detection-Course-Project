using MetroFramework;
using MetroFramework.Forms;
using MetroFramework.Components;
using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HumanDetection
{
    public partial class Form1 : MetroForm
    {
        #region Инициализация

        public Form1()
        {
            InitializeComponent();
        }

        #endregion

        #region Обработка события нажатия плитки

        public void metroTile1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Открытие";
            openFileDialog1.Filter = "Файлы изображений (*.BMP;*.JPEG;*.GIF;*.PNG;*.TIFF)|*.BMP;*.JPEG;*.JPG;*.JPE;*.JIF;*.JFIF;*.JFI;*.GIF;*.PNG;*.TIFF";
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (var tmp = new Bitmap(openFileDialog1.FileName))
                    {
                        this.pictureBox1.Image = new Bitmap(tmp);
                    }

                    if (pictureBox2.Image != null && pictureBox1.Image.Size == pictureBox2.Image.Size)
                    {
                        metroTile2_Enable();
                        metroTile3_Enable();
                    }
                    else
                    {
                        metroTile2_Disable();
                        metroTile3_Disable();
                    }
                }
                catch
                {
                    MetroMessageBox.Show(this, "Неподдерживаемый формат.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void metroTile2_Click(object sender, EventArgs e)
        {
            if (this.pictureBox1.Image == null || this.pictureBox2.Image == null)
            {
                MetroMessageBox.Show(this, "Изображения еще не загружены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Stopwatch s = new Stopwatch();
            //s.Start();

            this.pictureBox1.Image = (new HumanDetector(this.pictureBox1.Image, this.pictureBox2.Image)).Draw();

            //var gb = new GrayscaleBitmap(new Bitmap(this.pictureBox1.Image));
            //this.pictureBox1.Image = gb.Gaussian().Canny().ToBitmap();

            //s.Stop();
            //MetroMessageBox.Show(this, s.Elapsed.ToString(), "Time!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /* Сохранение результата в формате JPEG с минимальным уровнем сжатия. 
         * Чтобы указать уровень сжатия при сохранении изображения в формате JPEG,
         * создается объект EncoderParameters. Он передается методу Save класса Image.
         * Объект EncoderParameters инициализируется так, чтобы он включал массив из
         * одного объекта EncoderParameter. При создании объекта EncoderParameter
         * указывается кодировщик Quality и выбирается уровень сжатия 100L. */
        private void metroTile3_Click(object sender, EventArgs e)
        {
            if (this.pictureBox1.Image == null)
            {
                MetroMessageBox.Show(this, "Image is not found.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Jpeg Image (*.JPEG, *.JPG)|*.jpeg;*.jpg";
            saveFileDialog1.Title = "Сохранение";

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    myEncoderParameters.Param[0] = new EncoderParameter(myEncoder, 100L);

                    this.pictureBox1.Image.Save(saveFileDialog1.FileName, jgpEncoder, myEncoderParameters);
                }
                catch
                {
                    MetroMessageBox.Show(this, "Не удалось сохранить изображение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public void metroTile4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Открытие";
            openFileDialog1.Filter = "Файлы изображений (*.BMP;*.JPEG;*.GIF;*.PNG;*.TIFF)|*.BMP;*.JPEG;*.JPG;*.JPE;*.JIF;*.JFIF;*.JFI;*.GIF;*.PNG;*.TIFF";
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (var tmp = new Bitmap(openFileDialog1.FileName))
                    {
                        this.pictureBox2.Image = new Bitmap(tmp);
                    }

                    if (pictureBox1.Image != null && pictureBox1.Image.Size == pictureBox2.Image.Size)
                    {
                        metroTile2_Enable();
                        metroTile3_Enable();
                    }
                    else
                    {
                        metroTile2_Disable();
                        metroTile3_Disable();
                    }
                }
                catch
                {
                    MetroMessageBox.Show(this, "Неподдерживаемый формат.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion

        #region Изменение цвета плитки при наведении

        private void metroTile1_MouseMove(object sender, EventArgs e)
        {
            this.metroTile1.Style = MetroFramework.MetroColorStyle.Orange;
        }
        private void metroTile1_MouseLeave(object sender, EventArgs e)
        {
            this.metroTile1.Style = MetroFramework.MetroColorStyle.Yellow;
        }

        private void metroTile2_MouseMove(object sender, EventArgs e)
        {
            this.metroTile2.Style = MetroFramework.MetroColorStyle.Orange;
        }
        private void metroTile2_MouseLeave(object sender, EventArgs e)
        {
            this.metroTile2.Style = MetroFramework.MetroColorStyle.Yellow;
        }

        private void metroTile3_MouseMove(object sender, EventArgs e)
        {
            this.metroTile3.Style = MetroFramework.MetroColorStyle.Orange;
        }
        private void metroTile3_MouseLeave(object sender, EventArgs e)
        {
            this.metroTile3.Style = MetroFramework.MetroColorStyle.Yellow;
        }

        private void metroTile4_MouseMove(object sender, EventArgs e)
        {
            this.metroTile4.Style = MetroFramework.MetroColorStyle.Orange;
        }
        private void metroTile4_MouseLeave(object sender, EventArgs e)
        {
            this.metroTile4.Style = MetroFramework.MetroColorStyle.Yellow;
        }

        private void metroTile2_Enable()
        {
            metroTile2.Enabled = true;
            this.metroTile2.UseCustomBackColor = false;
            metroTile2.Style = MetroFramework.MetroColorStyle.Yellow;
        }
        private void metroTile2_Disable()
        {
            metroTile2.Enabled = false;
            this.metroTile2.UseCustomBackColor = true;
            this.metroTile2.BackColor = System.Drawing.Color.LightGray;
        }

        private void metroTile3_Enable()
        {
            metroTile3.Enabled = true;
            this.metroTile3.UseCustomBackColor = false;
            metroTile3.Style = MetroFramework.MetroColorStyle.Yellow;
        }
        private void metroTile3_Disable()
        {
            metroTile3.Enabled = false;
            this.metroTile3.UseCustomBackColor = true;
            this.metroTile3.BackColor = System.Drawing.Color.LightGray;
        }

        #endregion
    }
}
