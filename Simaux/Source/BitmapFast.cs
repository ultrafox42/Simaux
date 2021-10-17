using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using WindowsApi;

namespace FastBitmap
{
    internal struct InternalColor
    {
        public Color Color { get => Color.FromArgb(A, R, G, B); }

        public InternalColor(byte r, byte g, byte b, byte a = 255)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
        public bool EqualsColor(Color color)
        {
            return color.R == R && color.G == G && color.B == B;
        }
        internal byte A;
        internal byte R;
        internal byte G;
        internal byte B;
    }
    public class BitmapFast : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public int Width => Bitmap.Width;
        public int Height => Bitmap.Height;
        public string Filename { get => m_Filename; set => m_Filename = value; }
        public BitmapFast(string bitmapFile)
        {
            if (File.Exists(bitmapFile))
            {
                Bitmap = new Bitmap(bitmapFile, false);
                ToDataStream();
                m_Filename = bitmapFile;
            }
        }
        public BitmapFast(string bitmapFile, PixelFormat format)
        {
            if (File.Exists(bitmapFile))
            { 
                Bitmap = new Bitmap(bitmapFile, false);
                if (format != Bitmap.PixelFormat)
                {
                    ToDataStream();
                    var newFast = GetCrop(new Rectangle(0, 0, Width, Height), format);
                    Bitmap = newFast.Bitmap;
                    newFast.Dispose();
                }
                ToDataStream();
                m_Filename = bitmapFile;
            }
        }
        public BitmapFast(int width, int height, PixelFormat format = PixelFormat.Format24bppRgb)
        {
            Bitmap = new Bitmap(width, height, format);
            ToDataStream();
        }
        internal BitmapFast(Bitmap bmp)
        {
            Bitmap = bmp;
            ToDataStream();
        }
        internal BitmapFast ConvertTo(PixelFormat format)
        {
            if (format != Bitmap.PixelFormat)
                return GetCrop(new Rectangle(0, 0, Width, Height), format);
            return null;
        }
        internal void Save(string filename = null)
        {
            if (filename != null)
                m_Filename = filename;
            Bitmap.Save(m_Filename);
        }
        public BitmapFast GetCrop(Rectangle rect, PixelFormat format = PixelFormat.Format24bppRgb)
        {
            if (m_RgbStream == null)
                ToDataStream();

            var resultBitmap = new BitmapFast(rect.Width, rect.Height, format);
            for (int y = 0; y < rect.Height; y++)
                for (int x = 0; x < rect.Width; x++)
                    resultBitmap.SetPixel(x, y, GetPixel(rect.X + x, rect.Y + y));
            resultBitmap.FromDataStream();
            return resultBitmap;
        }
        public BitmapFast GetCropRoundCorners(Rectangle rect, int CornerRadius, Color BackgroundColor)
        {
            CornerRadius *= 2;
            var roundedImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(roundedImage))
            {
                g.Clear(BackgroundColor);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (Brush brush = new TextureBrush(Bitmap))
                {
                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        gp.AddArc(-1, -1, CornerRadius, CornerRadius, 180, 90);
                        gp.AddArc(0 + roundedImage.Width - CornerRadius, -1, CornerRadius, CornerRadius, 270, 90);
                        gp.AddArc(0 + roundedImage.Width - CornerRadius, 0 + roundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                        gp.AddArc(-1, 0 + roundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);

                        g.FillPath(brush, gp);
                    }
                }
                return new BitmapFast(roundedImage);
            }
        }
        unsafe internal void SetPixel(int x, int y, InternalColor pixel)
        {
            byte* startPtr = (byte*)(m_RgbStream.DataPointer + (y * m_Stride + x * m_BytesPerPixel));
            *startPtr++ = pixel.B;
            *startPtr++ = pixel.G;
            *startPtr++ = pixel.R;
            if (m_BytesPerPixel == 4)
                *startPtr = pixel.A;

        }
        unsafe internal void SetPixel(int x, int y, byte grayscale8Bit)
        {
            *((byte*)(m_RgbStream.DataPointer + (y * m_Stride + x * m_BytesPerPixel))) = grayscale8Bit;
        }
        unsafe internal byte GetPixelGrayscale(int x, int y)
        {
            return *((byte*)(m_RgbStream.DataPointer + (y * m_Stride + x * m_BytesPerPixel)));
        }
        unsafe internal InternalColor GetPixel(int x, int y)
        {
            byte* startPtr = (byte*)(m_RgbStream.DataPointer + (y * m_Stride + x * m_BytesPerPixel));
            InternalColor color = new InternalColor()
            {
                B = *startPtr++,
                G = *startPtr++,
                R = *startPtr++,
            };
            if (m_BytesPerPixel == 4)
                color.A = *startPtr;
            else
                color.A = 255;

            return color;
        }
        public byte GetBrightness(int x, int y)
        {
            var col = GetPixel(x, y);
            return (byte)((col.R + col.G + col.B) / 3 + 0.5);
        }
        public byte GetBrightness(Rectangle rect)
        {
            uint sum = 0;
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    sum += GetBrightness(x, y);
                }
            }
            return (byte)(sum / (rect.Width * rect.Height));
        }
        internal bool ChangeBrightness(int x, int y, double value)
        {
            var col = GetPixel(x, y);
            if (col.A > 0)
            {
                col.R = (byte)Math.Min(255, (col.R + (col.R * value + 0.5)));
                col.G = (byte)Math.Min(255, (col.G + (col.G * value + 0.5)));
                col.B = (byte)Math.Min(255, (col.B + (col.B * value + 0.5)));
                SetPixel(x, y, col);
                return true;
            }
            return false;
        }
        public double[] GetBrightnessHistogram(bool normalize = true)
        {
            double[] res = new double[256];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    res[GetBrightness(x, y)]++;
                }
            }
            if (normalize)
            {
                double numPixel = Width * Height;
                for (int i = 0; i < res.Length; i++)
                    res[i] /= numPixel;
            }
            return res;
        }
        public BitmapFast ToGrayscale()
        {
            var grayscaleBitmap = new BitmapFast(Bitmap.Width, Bitmap.Height, PixelFormat.Format8bppIndexed);
            for (int y = 0; y < Bitmap.Height; y++)
            {
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    var pixel = GetPixel(x, y);
                    byte grayValue = (byte)(pixel.R * 0.2126 +
                                            pixel.G * 0.7152 +
                                            pixel.B * 0.0722 + 0.5);
                    grayscaleBitmap.SetPixel(x, y, grayValue);
                }

            }
            var pal = grayscaleBitmap.Bitmap.Palette;
            for (int i = 0; i < pal.Entries.Length; i++)
            {
                pal.Entries[i] = Color.FromArgb(i, i, i);
            }
            grayscaleBitmap.Bitmap.Palette = pal;
            grayscaleBitmap.FromDataStream();
            grayscaleBitmap.Filename = Path.GetDirectoryName(m_Filename) + '\\' + Path.GetFileNameWithoutExtension(m_Filename) + "_gray" + Path.GetExtension(m_Filename);
            return grayscaleBitmap;
        }
        private bool ToDataStream()
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
            BitmapData bmpData = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Bitmap.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            m_BytesPerPixel = Image.GetPixelFormatSize(Bitmap.PixelFormat) >> 3;
            m_Stride = Math.Abs(bmpData.Stride);
            int bytes = m_Stride * Bitmap.Height;
            m_RgbStream = new SharpDX.DataStream(bytes, true, true);
            unsafe
            {
                Buffer.MemoryCopy(ptr.ToPointer(), m_RgbStream.DataPointer.ToPointer(), bytes, bytes);
            }
            Bitmap.UnlockBits(bmpData);
            return true;
        }
        internal bool FromDataStream()
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
            BitmapData bmpData = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Bitmap.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = m_Stride * Bitmap.Height;
            unsafe
            {
                Buffer.MemoryCopy(m_RgbStream.DataPointer.ToPointer(), ptr.ToPointer(), bytes, bytes);
            }
            Bitmap.UnlockBits(bmpData);
            return true;
        }
        public static BitmapFast FromWindow(IntPtr hwnd)
        {
            WinApi.GetWindowRect(hwnd, out WinApi.RECT windowRect);

            int width = windowRect.Right - windowRect.Left+1;
            int height = windowRect.Bottom - windowRect.Top+1;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            gfxBmp.CopyFromScreen(new Point(windowRect.Left, windowRect.Top), Point.Empty, new Size(width, height));
            return new BitmapFast(bmp);
        }
        public static BitmapFast FromClientWindow(IntPtr hwnd)
        {
            WinApi.GetClientRect(hwnd, out WinApi.RECT rect);
            var pt = new Point(rect.Left, rect.Top);
            WinApi.ClientToScreen(hwnd, ref pt);
            Rectangle scRect = new Rectangle(pt.X, pt.Y, rect.Right + 1, rect.Bottom + 1);
            Bitmap bmp = new Bitmap(scRect.Width, scRect.Height, PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            gfxBmp.CopyFromScreen(pt, Point.Empty, new Size(scRect.Width, scRect.Height));
            return new BitmapFast(bmp);
        }
        public void Dispose()
        {
            ((IDisposable)m_RgbStream).Dispose();
        }

        private SharpDX.DataStream m_RgbStream = null;
        private int m_BytesPerPixel = 0;
        private int m_Stride = 0;
        private string m_Filename;
    }
}
