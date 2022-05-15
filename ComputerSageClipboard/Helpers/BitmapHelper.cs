using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ComputerSageClipboard.Helpers
{
    public class BitmapHelper
    {
        public static readonly string Bas64Header = "data:image/jpeg;base64,";
        private static string fileIconB64 = null,
            textIconB64 = null,
            audioIconB64 = null,
            defaultIconB64 = null;

        public static string GetBase64(BitmapSource source, bool header = false)
        {
            var bitmap = GetBitmap(source);
            return GetBase64(bitmap, header);
        }

        public static string GetBase64(Bitmap bitmap, bool header = false)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);

            byte[] byteImage = ms.ToArray();
            var result = Convert.ToBase64String(byteImage);
            if (header)
                result = Bas64Header + result;

            return result;
        }

        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppArgb
            );

            BitmapData data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb
            );

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride
            );

            bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapSource FromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream ms = new MemoryStream();
            ms.Write(bytes, 0, bytes.Length);
            
            Bitmap bitmap = new Bitmap(ms);
            return GetBitmapSource(bitmap);
        }

        public static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat
            );

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride
            );

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static string GetFileIcon()
        {
            if (fileIconB64 != null) return fileIconB64;
            fileIconB64 = GetBase64(LoadIcon("fileIcon"), true);
            return fileIconB64;
        }

        public static string GetTextIcon()
        {
            if (textIconB64 != null) return textIconB64;
            textIconB64 = GetBase64(LoadIcon("textIcon"), true);
            return textIconB64;
        }

        public static string GetAudioIcon()
        {
            if (audioIconB64 != null) return audioIconB64;
            audioIconB64 = GetBase64(LoadIcon("audioIcon"), true);
            return audioIconB64;
        }

        public static string GetDefaultIcon()
        {
            if (defaultIconB64 != null) return defaultIconB64;
            defaultIconB64 = GetBase64(LoadIcon("clipboard"), true);
            return defaultIconB64;
        }

        private static Bitmap LoadIcon(string icon)
        {
            var path = Path.Combine(FileManager.GetPluginPath(), "Images");
            var iconPath = Path.Combine(path, icon) + ".png";
            return new Bitmap(iconPath);
        }
    }
}
