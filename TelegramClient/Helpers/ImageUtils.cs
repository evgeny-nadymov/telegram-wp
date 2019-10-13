// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Phone;

namespace TelegramClient.Helpers
{
    public static class ImageUtils
    {
        public static BitmapImage CreateImage(Stream stream)
        {
            BitmapImage imageSource;

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var image = new BitmapImage();
                image.SetSource(stream);
                imageSource = image;
            }
            catch (Exception)
            {
                return null;
            }

            return imageSource;
        }
        public static BitmapImage CreateImage(Stream stream, BitmapCreateOptions options)
        {
            BitmapImage imageSource;

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var image = new BitmapImage();
                image.CreateOptions = options;
                image.SetSource(stream);
                imageSource = image;
            }
            catch (Exception)
            {
                return null;
            }

            return imageSource;
        }

        public static BitmapImage CreateImage(byte[] buffer)
        {
            BitmapImage imageSource;

            try
            {
                using (var stream = new MemoryStream(buffer))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var image = new BitmapImage();
                    image.SetSource(stream);
                    imageSource = image;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return imageSource;
        }

        public static BitmapImage CreateImage(byte[] buffer, BitmapCreateOptions options)
        {
            BitmapImage imageSource;

            try
            {
                using (var stream = new MemoryStream(buffer))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var image = new BitmapImage();
                    image.CreateOptions = options;
                    image.SetSource(stream);
                    imageSource = image;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return imageSource;
        }

        public static byte[] CreateThumb(byte[] image, int rectangleSize, int targetQuality, out int targetHeight, out int targetWidth)
        {
            try
            {
                var stream = new MemoryStream(image);
                var writeableBitmap = PictureDecoder.DecodeJpeg(stream);

                var maxDimension = Math.Max(writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
                var scale = (double)rectangleSize / maxDimension;
                targetHeight = (int)(writeableBitmap.PixelHeight * scale);
                targetWidth = (int)(writeableBitmap.PixelWidth * scale);

                var outStream = new MemoryStream();
                writeableBitmap.SaveJpeg(outStream, targetWidth, targetHeight, 0, targetQuality);

                return outStream.ToArray();
            }
            catch (Exception ex)
            {
                targetWidth = 0;
                targetHeight = 0;
            }

            return new byte[0];
        }

        private static UInt64 GetColors(uint value)
        {
            var p = BitConverter.GetBytes(value);

            return p[0] + ((UInt64)p[1] << 16) + ((UInt64)p[2] << 32);
        }

        public static void FastBlur(this WriteableBitmap wb)
        {
            var pixels = new byte[wb.Pixels.Length * 4];
            for (var j = 0; j < wb.Pixels.Length; j++)
            {
                pixels[j * 4] = (byte) wb.Pixels[j];                //r
                pixels[j * 4 + 1] = (byte) (wb.Pixels[j] >> 8);     //g
                pixels[j * 4 + 2] = (byte) (wb.Pixels[j] >> 16);    //b
                pixels[j * 4 + 3] = (byte) (wb.Pixels[j] >> 24);    //a
            }

            const int sizeFull = 8100;
            uint w = (uint) wb.PixelWidth;
            uint h = (uint) wb.PixelHeight;
            uint radius = 7;
            uint stride = w*4;
            uint r1 = radius + 1;
            uint div = radius*2 + 1;
            if (radius > 15 || div >= w || div >= h || w*h >= sizeFull)
            {
                return;
            }

            var rgb = new ulong[sizeFull];

            int x, y, i;

            Int64 yw = 0;
            Int64 we = w - r1;
            for (y = 0; y < h; y++) 
            {
                UInt64 cur = GetColors (pixels[yw]);
                UInt64 rgballsum = unchecked((ulong) - radius * cur);
                UInt64 rgbsum = cur * ((r1 * (r1 + 1)) >> 1);

                for (i = 1; i <= radius; i++) 
                {
                    cur = GetColors (pixels[yw + i * 4]);
                    rgbsum += cur * (UInt64) (r1 - i);
                    rgballsum += cur;
                }

                x = 0;

                while (x < r1) {
                    //update (0, x, x + r1);
                    rgb[y * w + x] = (rgbsum >> 6) & 0x00FF00FF00FF00FF;
                    rgballsum += (GetColors(pixels[yw + (0) * 4]) 
                        - 2 * GetColors(pixels[yw + (x) * 4]) 
                        + GetColors(pixels[yw + (x + r1) * 4]));
                    rgbsum += rgballsum;
                    x++;      
                }
                while (x < we) 
                {
                    //update (x - r1, x, x + r1);
                    rgb[y * w + x] = (rgbsum >> 6) & 0x00FF00FF00FF00FF;
                    rgballsum += (GetColors(pixels[yw + (x - r1) * 4])
                        - 2 * GetColors(pixels[yw + (x) * 4])
                        + GetColors(pixels[yw + (x + r1) * 4]));
                    rgbsum += rgballsum;
                    x++;  
                }
                while (x < w) 
                {
                    //update (x - r1, x, w - 1);
                    rgb[y * w + x] = (rgbsum >> 6) & 0x00FF00FF00FF00FF;
                    rgballsum += (GetColors(pixels[yw + (x - r1) * 4])
                        - 2 * GetColors(pixels[yw + (x) * 4])
                        + GetColors(pixels[yw + (w - 1) * 4]));
                    rgbsum += rgballsum;
                    x++;  
                }

                yw += stride;
            }

            uint he = h - r1;
            for (x = 0; x < w; x++)
            {
                UInt64 rgballsum = unchecked ((ulong) -radius*rgb[x]);
                UInt64 rgbsum = rgb[x]*((r1*(r1 + 1)) >> 1);
                for (i = 1; i <= radius; i++)
                {
                    rgbsum += rgb[i*w + x]*(UInt64) (r1 - i);
                    rgballsum += rgb[i*w + x];
                }

                y = 0;
                int yi = x*4;        
                                             
                yi += (int)stride;

                while (y < r1)
                {
                    //update(0, y, y + r1);
                    UInt64 res = rgbsum >> 6;
                    pixels[yi] = (byte)res;
                    pixels[yi + 1] = (byte)(res >> 16);
                    pixels[yi + 2] = (byte)(res >> 32);
                    rgballsum += rgb[x + (0) * w]
                        - 2 * rgb[x + (y) * w]
                        + rgb[x + (y + r1) * w];
                    rgbsum += rgballsum;
                    y++;
                }
                while (y < he)
                {
                    //update(y - r1, y, y + r1);
                    UInt64 res = rgbsum >> 6;
                    pixels[yi] = (byte)res;
                    pixels[yi + 1] = (byte)(res >> 16);
                    pixels[yi + 2] = (byte)(res >> 32);
                    rgballsum += rgb[x + (y - r1) * w]
                        - 2 * rgb[x + (y) * w]
                        + rgb[x + (y + r1) * w];
                    rgbsum += rgballsum;
                    y++;
                }
                while (y < h)
                {
                    //update(y - r1, y, h - 1);
                    UInt64 res = rgbsum >> 6;
                    pixels[yi] = (byte)res;
                    pixels[yi + 1] = (byte)(res >> 16);
                    pixels[yi + 2] = (byte)(res >> 32);
                    rgballsum += rgb[x + (y - r1) * w]
                        - 2 * rgb[x + (y) * w]
                        + rgb[x + (h - 1) * w];
                    rgbsum += rgballsum;
                    y++;
                }
            }

            for (var j = 0; j < wb.Pixels.Length; j++)
            {
                wb.Pixels[j] = pixels[j*4] | (pixels[j*4 + 1] << 8) | (pixels[j*4 + 2] << 16) | (pixels[j*4 + 3] << 24);
            }
        }

        public static void BoxBlur(this WriteableBitmap bmp, int range)
        {
            if ((range & 1) == 0)
            {
                throw new InvalidOperationException("Range must be odd!");
            }

            bmp.BoxBlurHorizontal(range);
            bmp.BoxBlurVertical(range);
        }

        public static void BoxBlurHorizontal(this WriteableBitmap bmp, int range)
        {
            int[] pixels = bmp.Pixels;
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
            int halfRange = range / 2;
            int index = 0;
            int[] newColors = new int[w];

            for (int y = 0; y < h; y++)
            {
                int hits = 0;
                int r = 0;
                int g = 0;
                int b = 0;
                for (int x = -halfRange; x < w; x++)
                {
                    int oldPixel = x - halfRange - 1;
                    if (oldPixel >= 0)
                    {
                        int col = pixels[index + oldPixel];
                        if (col != 0)
                        {
                            r -= ((byte)(col >> 16));
                            g -= ((byte)(col >> 8));
                            b -= ((byte)col);
                        }
                        hits--;
                    }

                    int newPixel = x + halfRange;
                    if (newPixel < w)
                    {
                        int col = pixels[index + newPixel];
                        if (col != 0)
                        {
                            r += ((byte)(col >> 16));
                            g += ((byte)(col >> 8));
                            b += ((byte)col);
                        }
                        hits++;
                    }

                    if (x >= 0)
                    {
                        int color =
                            (255 << 24)
                            | ((byte)(r / hits) << 16)
                            | ((byte)(g / hits) << 8)
                            | ((byte)(b / hits));

                        newColors[x] = color;
                    }
                }

                for (int x = 0; x < w; x++)
                {
                    pixels[index + x] = newColors[x];
                }

                index += w;
            }
        }

        public static void BoxBlurVertical(this WriteableBitmap bmp, int range)
        {
            int[] pixels = bmp.Pixels;
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
            int halfRange = range / 2;

            int[] newColors = new int[h];
            int oldPixelOffset = -(halfRange + 1) * w;
            int newPixelOffset = (halfRange) * w;

            for (int x = 0; x < w; x++)
            {
                int hits = 0;
                int r = 0;
                int g = 0;
                int b = 0;
                int index = -halfRange * w + x;
                for (int y = -halfRange; y < h; y++)
                {
                    int oldPixel = y - halfRange - 1;
                    if (oldPixel >= 0)
                    {
                        int col = pixels[index + oldPixelOffset];
                        if (col != 0)
                        {
                            r -= ((byte)(col >> 16));
                            g -= ((byte)(col >> 8));
                            b -= ((byte)col);
                        }
                        hits--;
                    }

                    int newPixel = y + halfRange;
                    if (newPixel < h)
                    {
                        int col = pixels[index + newPixelOffset];
                        if (col != 0)
                        {
                            r += ((byte)(col >> 16));
                            g += ((byte)(col >> 8));
                            b += ((byte)col);
                        }
                        hits++;
                    }

                    if (y >= 0)
                    {
                        int color =
                            (255 << 24)
                            | ((byte)(r / hits) << 16)
                            | ((byte)(g / hits) << 8)
                            | ((byte)(b / hits));

                        newColors[y] = color;
                    }

                    index += w;
                }

                for (int y = 0; y < h; y++)
                {
                    pixels[y * w + x] = newColors[y];
                }
            }
        }
    }
}
