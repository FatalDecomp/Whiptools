using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Whiptools
{
    public static class Bitmapper
    {
        public static Color[] ConvertRGBToPalette(byte[] inputArray)
        {
            if (inputArray.Length % 3 != 0) throw new ArgumentException();
            Color[] output = new Color[inputArray.Length / 3];
            for (int i = 0; i < output.Length; i++)
                output[i] = Color.FromArgb(inputArray[i * 3],
                    inputArray[i * 3 + 1], inputArray[i * 3 + 2]);
            return output;
        }

        public static Bitmap ConvertPaletteToBitmap(Color[] palette)
        {
            var bitmap = new Bitmap(palette.Length, 1, PixelFormat.Format24bppRgb);
            for (int i = 0; i < palette.Length; i++)
                bitmap.SetPixel(i, 0, GetColorHigh(palette[i]));
            return bitmap;
        }

        public static byte[] CreateRGBArray(byte[] bitmapArray, Color[] palette)
        {
            if (bitmapArray == null || palette == null)
                throw new ArgumentNullException();
            byte[] output = new byte[bitmapArray.Length * 3];
            for (int i = 0; i < bitmapArray.Length; i++)
            {
                if (bitmapArray[i] >= palette.Length)
                    throw new ArgumentOutOfRangeException();
                output[i * 3] = palette[bitmapArray[i]].B;
                output[i * 3 + 1] = palette[bitmapArray[i]].G;
                output[i * 3 + 2] = palette[bitmapArray[i]].R;
            }
            return output;
        }

        public static Bitmap CreateBitmapFromRGB(int width, int height, byte[] rgbArray)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int stride = bitmapData.Stride;
            IntPtr ptr = bitmapData.Scan0;
            byte[] rgbArrayHigh = GetByteArrayHigh(rgbArray);
            for (int y = 0; y < height; y++)
            {
                int offset = y * width * 3;
                Marshal.Copy(rgbArrayHigh, offset, ptr + y * stride,
                    Math.Min(width * 3, rgbArrayHigh.Length - offset));
            }
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        // bitmap creator

        public static Color[] GetPaletteFromBitmap(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height; 
            
            var hashColors = new HashSet<Color>();

            var pixelData = CopyPixels(bitmap, out int stride);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * 3;
                    byte B = pixelData[offset];
                    byte G = pixelData[offset + 1];
                    byte R = pixelData[offset + 2];
                    hashColors.Add(GetColorLow(Color.FromArgb(R, G, B)));
                }
            }
            return hashColors.ToArray();
        }

        public static byte[] GetBitmapArray(Bitmap bitmap, Color[] palette)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] output = new byte[width * height];

            var paletteDict = new Dictionary<Color, byte>();
            for (int i = 0; i < palette.Length; i++)
                paletteDict[palette[i]] = (byte)i;

            var pixelData = CopyPixels(bitmap, out int stride);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * 3;
                    byte B = pixelData[offset];
                    byte G = pixelData[offset + 1];
                    byte R = pixelData[offset + 2];
                    if (paletteDict.TryGetValue(GetColorLow(Color.FromArgb(R, G, B)),
                        out byte paletteIndex))
                        output[y * width + x] = paletteIndex;
                    else throw new KeyNotFoundException();
                }
            }
            return output;
        }

        public static byte[] GetPaletteArray(Color[] palette)
        {
            byte[] output = new byte[palette.Length * 3];
            for (int i = 0; i < palette.Length; i++)
            {
                output[i * 3] = (byte)(palette[i].R);
                output[i * 3 + 1] = (byte)(palette[i].G);
                output[i * 3 + 2] = (byte)(palette[i].B);
            }
            return output;
        }

        // utils

        private static byte[] CopyPixels(Bitmap bitmap, out int stride)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            using (var clone = bitmap.Clone(new Rectangle(0, 0, width, height), PixelFormat.Format24bppRgb))
            {
                var rect = new Rectangle(0, 0, width, height);
                var bitmapData = clone.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    stride = bitmapData.Stride;
                    byte[] pixelData = new byte[stride * height];
                    Marshal.Copy(bitmapData.Scan0, pixelData, 0, pixelData.Length);
                    return pixelData;
                }
                finally
                {
                    clone.UnlockBits(bitmapData);
                }
            }
        }

        private static byte[] GetByteArrayHigh(byte[] input)
        {
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = (byte)GetIntHigh(input[i]);
            return output;
        }

        private static Color GetColorHigh(Color input) =>
            Color.FromArgb(
                GetIntHigh(input.R),
                GetIntHigh(input.G),
                GetIntHigh(input.B));

        private static Color GetColorLow(Color input) =>
            Color.FromArgb(
                input.R >> 2,
                input.G >> 2,
                input.B >> 2);

        private static int GetIntHigh(int input) =>
            (input << 2) + (input >> 4);
    }
}