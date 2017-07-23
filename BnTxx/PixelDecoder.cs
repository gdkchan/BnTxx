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
            Dictionary<TextureFormat, DecodeFunc> DecodeFuncs = new
            Dictionary<TextureFormat, DecodeFunc>()
        {
            { TextureFormat.RGBA8888, DecodeRGBA8888 },
            { TextureFormat.BC1,      BCn.DecodeBC1  },
            { TextureFormat.BC3,      BCn.DecodeBC3  }
        };

        public static bool TryDecode(Texture Tex, out Bitmap Img)
        {
            if (DecodeFuncs.ContainsKey(Tex.Format))
            {
                if (BitUtils.IsPow2(Tex.Width))
                {
                    Img = DecodeFuncs[Tex.Format](
                        Tex.Data,
                        Tex.Width,
                        Tex.Height);
                }
                else
                {
                    Img = new Bitmap(Tex.Width, Tex.Height);

                    using (Graphics g = Graphics.FromImage(Img))
                    {
                        int BlockSize = Tex.BlockSize;

                        int Chuncks = Tex.Height / BlockSize + 1;

                        Bitmap Plain = DecodeFuncs[Tex.Format](
                            Tex.Data,
                            Tex.Width * Chuncks,
                            BlockSize);

                        for (int i = 0; i < Chuncks; i++)
                        {
                            Rectangle Src = new Rectangle(i * Tex.Width, 0, Tex.Width, BlockSize);
                            Rectangle Dst = new Rectangle(0, i * BlockSize, Tex.Width, BlockSize);

                            g.DrawImage(Plain, Dst, Src, GraphicsUnit.Pixel);
                        }
                    }
                }

                return true;
            }

            Img = null;

            return false;
        }

        public static int GetDataLength(int Width, int Height, TextureFormat Fmt)
        {
            switch (Fmt)
            {
                case TextureFormat.RGBA8888: return (Width * Height) * 4;
                case TextureFormat.BC1:      return (Width * Height) / 2;
                case TextureFormat.BC3:      return (Width * Height);
                default:                     return -1;
            }
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
                    int IOffs = GetSwizzledAddressRGBA32(X, Y, XB, YB) * 4;

                    Output[OOffset + 0] = Buffer[IOffs + 2];
                    Output[OOffset + 1] = Buffer[IOffs + 1];
                    Output[OOffset + 2] = Buffer[IOffs + 0];
                    Output[OOffset + 3] = Buffer[IOffs + 3];

                    OOffset += 4;
                }
            }

            return GetBitmap(Output, Width, Height);
        }

        public static int GetSwizzledAddressRGBA32(int X, int Y, int XB, int YB)
        {
            return GetSwizzledAddress(X, Y, XB, YB, 2);
        }

        public static int GetSwizzledAddressBC1(int X, int Y, int XB, int YB)
        {
            return GetSwizzledAddress(X, Y, XB, YB, 1);
        }

        public static int GetSwizzledAddressBC2_3(int X, int Y, int XB, int YB)
        {
            return GetSwizzledAddress(X, Y, XB, YB, 0);
        }

        private static int GetSwizzledAddress(int X, int Y, int XB, int YB, int XBase)
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
            int Bit     = 0;
            int Address = 0;

            while (Bit < Math.Min(Size, XBase + 9))
            {
                int XMask = (1 << XCnt) - 1;
                int YMask = (1 << YCnt) - 1;

                Address |= (X & XMask) << Bit;
                Address |= (Y & YMask) << Bit + XCnt;

                X >>= XCnt;
                Y >>= YCnt;

                XB -= XCnt;
                YB -= YCnt;

                Bit += XCnt + YCnt;

                XCnt = Math.Min(XB, 1);
                YCnt = Math.Min(YB, YCnt << 1);
            }

            Address |= X << Bit;
            Address |= Y << Bit + XB;

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
