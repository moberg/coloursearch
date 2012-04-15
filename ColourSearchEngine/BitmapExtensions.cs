using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AForge.Imaging;

namespace ColourSearchEngine
{
    public static class BitmapExtensions
    {
        public static Histogram GetHistograms(this Bitmap source)
        {
            return new Histogram
                       {
                           RgbHistogram = GetRgbHistogram(source),
                           HsvHistogram = GetHsvHistogram(source)
                       };
        }

        public static int[][] GetRgbHistogram(this Bitmap SourceImage)
        {
            int[][] rgbColor = { new int[256], new int[256], new int[256], new int[256] };
            int width = SourceImage.Width, height = SourceImage.Height;
            byte Red, Green, Blue;
            Color pixelColor;

            for (int i = 0, j; i < width; ++i)
                for (j = 0; j < height; ++j)
                {
                    pixelColor = SourceImage.GetPixel(i, j);
                    Red = pixelColor.R;
                    Green = pixelColor.G;
                    Blue = pixelColor.B;
                    ++rgbColor[0][(int)(0.114 * Blue + 0.587 * Green + 0.299 * Red)];
                    ++rgbColor[1][Red];                             
                    ++rgbColor[2][Green];                         
                    ++rgbColor[3][Blue];                            
                }

            return rgbColor;
        }

        public static int[][] GetHsvHistogram(this Bitmap SourceImage)
        {
            int[][] hsvColor = { new int[360], new int[101], new int[101] };
            int width = SourceImage.Width, height = SourceImage.Height;
            Color pixelColor;
            double h, s, v;

            for (int i = 0, j; i < width; ++i)
                for (j = 0; j < height; ++j)
                {
                    pixelColor = SourceImage.GetPixel(i, j);
                    Util.ColorToHSV(pixelColor, out h, out s, out v);

                    ++hsvColor[0][(int)h];
                    ++hsvColor[1][(int)s * 100];
                    ++hsvColor[2][(int)v * 100];
                }

            return hsvColor;
        }


        public static Bitmap ConvertTo16bpp(this Bitmap img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format64bppArgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        public static Bitmap ResizeBitmap(this Bitmap source, int nWidth, int nHeight)
        {
            var result = new Bitmap(nWidth, nHeight);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(source, 0, 0, nWidth, nHeight);
            }

            source.Dispose();
            return result;
        }
    }
}
