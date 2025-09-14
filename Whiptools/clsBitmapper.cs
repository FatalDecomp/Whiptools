using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Whiptools
{
    class Bitmapper
    {
        public static Color[] ConvertRGBToPalette(byte[] inputArray)
        {
            if (inputArray.Length %3 != 0)
                throw new Exception();
            Color[] output = new Color[inputArray.Length / 3];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Color.FromArgb(
                    inputArray[i * 3],
                    inputArray[i * 3 + 1],
                    inputArray[i * 3 + 2]);
            }
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
            byte[] output = new byte[bitmapArray.Length * 3];
            for (int i = 0; i < bitmapArray.Length; i++)
            {
                if (bitmapArray[i] >= palette.Length)
                    throw new Exception();
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
                Marshal.Copy(rgbArrayHigh, y * width * 3, ptr + y * stride, width * 3);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        // bitmap creator

        public static Color[] GetPaletteFromBitmap(Bitmap bitmap)
        {
            var hashColors = new HashSet<Color>();
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    hashColors.Add(GetColorLow(bitmap.GetPixel(x, y)));
            Color[] output = new Color[hashColors.Count];
            hashColors.CopyTo(output);
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

        public static byte[] GetBitmapArray(Bitmap bitmap, Color[] palette)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] output = new byte[width * height];

            var paletteDict = new Dictionary<Color, byte>();
            for (int i = 0; i < palette.Length; i++)
                paletteDict[palette[i]] = (byte)i;

            using (var clone = bitmap.Clone(new Rectangle(0, 0, width, height), bitmap.PixelFormat))
            {
                var rect = new Rectangle(0, 0, width, height);
                var bitmapData = clone.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    int stride = bitmapData.Stride;
                    byte[] pixelData = new byte[stride * height];
                    Marshal.Copy(bitmapData.Scan0, pixelData, 0, pixelData.Length);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int offset = y * stride + x * 3;
                            byte B = pixelData[offset];
                            byte G = pixelData[offset + 1];
                            byte R = pixelData[offset + 2];
                            Color colorLow = GetColorLow(Color.FromArgb(R, G, B));

                            if (paletteDict.TryGetValue(colorLow, out byte paletteIndex))
                                output[y * width + x] = paletteIndex;
                            else
                                throw new Exception();
                        }
                    }
                }
                finally
                {
                    clone.UnlockBits(bitmapData);
                }
            }
            return output;
        }

        // utils

        private static byte[] GetByteArrayHigh(byte[] input)
        {
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = (byte)GetIntHigh(input[i]);
            return output;
        }

        private static Color GetColorHigh(Color input)
        {
            return Color.FromArgb(
                GetIntHigh(input.R),
                GetIntHigh(input.G),
                GetIntHigh(input.B));
        }
        private static Color GetColorLow(Color input)
        {
            return Color.FromArgb(
                input.R >> 2,
                input.G >> 2,
                input.B >> 2);
        }

        private static int GetIntHigh(int input) =>
            (input << 2) + (input >> 4);
    }
}