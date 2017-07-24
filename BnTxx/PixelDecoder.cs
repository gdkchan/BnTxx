using BnTxx.Formats;
using BnTxx.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BnTxx
{
    static class PixelDecoder
    {
        private delegate Bitmap DecodeFunc(byte[] Buffer, int Width, int Height);

        private static
            Dictionary<TextureFormatType, DecodeFunc> DecodeFuncs = new
            Dictionary<TextureFormatType, DecodeFunc>()
        {
            { TextureFormatType.RGB565,   DecodeRGB565   },
            { TextureFormatType.RGBA8888, DecodeRGBA8888 },
            { TextureFormatType.BC1,      BCn.DecodeBC1  },
            { TextureFormatType.BC2,      BCn.DecodeBC2  },
            { TextureFormatType.BC3,      BCn.DecodeBC3  },
            { TextureFormatType.BC4,      BCn.DecodeBC4  },
            { TextureFormatType.BC5,      BCn.DecodeBC5  }
        };

        public static bool TryDecode(Texture Tex, out Bitmap Img)
        {
            if (DecodeFuncs.ContainsKey(Tex.FormatType))
            {
                int TexWidth  = RoundSize(Tex.Width);
                int TexHeight = RoundSize(Tex.Height);

                Img = DecodeFuncs[Tex.FormatType](
                    Tex.Data,
                    TexWidth,
                    TexHeight);

                if (TexWidth  != Tex.Width ||
                    TexHeight != Tex.Height)
                {
                    Bitmap Output = new Bitmap(Tex.Width, Tex.Height);

                    using (Graphics g = Graphics.FromImage(Output))
                    {
                        Rectangle Rect = new Rectangle(0, 0, Tex.Width, Tex.Height);

                        g.DrawImage(Img, Rect, Rect, GraphicsUnit.Pixel);
                    }

                    Img.Dispose();

                    Img = Output;
                }

                return true;
            }

            Img = null;

            return false;
        }

        private static int RoundSize(int Size)
        {
            if ((Size & 0x0f) != 0)
            {
                Size &= ~0x0f;
                Size +=  0x10;
            }

            return Size;
        }

        public static Bitmap DecodeRGB565(byte[] Buffer, int Width, int Height)
        {
            byte[] Output = new byte[Width * Height * 4];

            int OOffset = 0;

            int XB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Width));
            int YB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Height));

            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    int IOffs = GetSwizzledAddressRGBA16(X, Y, XB, YB, Width) * 2;

                    IOffs %= Buffer.Length;

                    int Value =
                        Buffer[IOffs + 0] << 0 |
                        Buffer[IOffs + 1] << 8;

                    int R = ((Value >>  0) & 0x1f) << 3;
                    int G = ((Value >>  5) & 0x3f) << 2;
                    int B = ((Value >> 11) & 0x1f) << 3;

                    Output[OOffset + 0] = (byte)(B | (B >> 5));
                    Output[OOffset + 1] = (byte)(G | (G >> 6));
                    Output[OOffset + 2] = (byte)(R | (R >> 5));
                    Output[OOffset + 3] = 0xff;

                    OOffset += 4;
                }
            }

            return GetBitmap(Output, Width, Height);
        }

        public static Bitmap DecodeRGBA8888(byte[] Buffer, int Width, int Height)
        {
            byte[] Output = new byte[Width * Height * 4];

            int OOffset = 0;

            int XB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Width));
            int YB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Height));

            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    int IOffs = GetSwizzledAddressRGBA32(X, Y, XB, YB, Width) * 4;

                    IOffs %= Buffer.Length;

                    Output[OOffset + 0] = Buffer[IOffs + 2];
                    Output[OOffset + 1] = Buffer[IOffs + 1];
                    Output[OOffset + 2] = Buffer[IOffs + 0];
                    Output[OOffset + 3] = Buffer[IOffs + 3];

                    OOffset += 4;
                }
            }

            return GetBitmap(Output, Width, Height);
        }

        public static int GetSwizzledAddressRGBA8(int X, int Y, int XB, int YB, int Width)
        {
            return GetSwizzledAddress(X, Y, XB, YB, Width, 4);
        }

        public static int GetSwizzledAddressRGBA16(int X, int Y, int XB, int YB, int Width)
        {
            return GetSwizzledAddress(X, Y, XB, YB, Width, 3);
        }

        public static int GetSwizzledAddressRGBA32(int X, int Y, int XB, int YB, int Width)
        {
            return GetSwizzledAddress(X, Y, XB, YB, Width, 2);
        }

        public static int GetSwizzledAddressBC1(int X, int Y, int XB, int YB, int Width)
        {
            return GetSwizzledAddress(X, Y, XB, YB, Width, 1);
        }

        public static int GetSwizzledAddressBC2_3(int X, int Y, int XB, int YB, int Width)
        {
            return GetSwizzledAddress(X, Y, XB, YB, Width, 0);
        }

        private static int GetSwizzledAddress(int X, int Y, int XB, int YB,int Width, int XBase)
        {
            /*
             * Examples of patterns:
             *                     x x y x y y x y 0 0 0 0 64   x 64   dxt5
             *         x x x x x y y y y x y y x y 0 0 0 0 512  x 512  dxt5
             *     y x x x x x x y y y y x y y x y 0 0 0 0 1024 x 1024 dxt5
             *   y y x x x x x x y y y y x y y x y x 0 0 0 2048 x 2048 dxt1
             * y y y x x x x x x y y y y x y y x y x x 0 0 1024 x 1024 rgba8888
             * 
             * Read from right to left, LSB first.
             */
            int Size    = XB + YB;
            int XCnt    = XBase;
            int YCnt    = 1;
            int XUsed   = 0;
            int YUsed   = 0;
            int Bit     = 0;
            int Address = 0;

            while (Bit < XBase + 9 && XUsed + XCnt < XB)
            {
                int XMask = (1 << XCnt) - 1;
                int YMask = (1 << YCnt) - 1;

                Address |= (X & XMask) << Bit;
                Address |= (Y & YMask) << Bit + XCnt;

                X >>= XCnt;
                Y >>= YCnt;

                XUsed += XCnt;
                YUsed += YCnt;

                Bit += XCnt + YCnt;

                XCnt = Math.Min(XB - XUsed, 1);
                YCnt = Math.Min(YB - YUsed, YCnt << 1);
            }

            Width >>= XUsed;

            Address |= (X + Y * Width) << Bit;

            return Address;
        }

        public static Bitmap GetBitmap(byte[] Buffer, int Width, int Height)
        {
            Rectangle Rect = new Rectangle(0, 0, Width, Height);

            Bitmap Img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            BitmapData ImgData = Img.LockBits(Rect, ImageLockMode.WriteOnly, Img.PixelFormat);

            Marshal.Copy(Buffer, 0, ImgData.Scan0, Buffer.Length);

            Img.UnlockBits(ImgData);

            return Img;
        }
    }
}
