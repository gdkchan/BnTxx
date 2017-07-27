using BnTxx.Formats;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BnTxx
{
    static class PixelDecoder
    {
        private delegate Bitmap DecodeFunc(Texture Tex);

        private static
            Dictionary<TextureFormatType, DecodeFunc> DecodeFuncs = new
            Dictionary<TextureFormatType, DecodeFunc>()
        {
            { TextureFormatType.RGB565,   DecodeRGB565   },
            { TextureFormatType.L8A8,     DecodeL8A8     },
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
                Img = DecodeFuncs[Tex.FormatType](Tex);

                if (Img.Width  != Tex.Width ||
                    Img.Height != Tex.Height)
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

        public static Bitmap DecodeRGB565(Texture Tex)
        {
            byte[] Output = new byte[Tex.Width * Tex.Height * 4];

            int OOffset = 0;

            SwizzleAddr Swizzle = new SwizzleAddr(Tex.Width, Tex.Height, 0x20);

            for (int Y = 0; Y < Tex.Height; Y++)
            {
                for (int X = 0; X < Tex.Width; X++)
                {
                    int IOffs = Swizzle.GetSwizzledAddress16(X, Y) * 2;

                    int Value =
                        Tex.Data[IOffs + 0] << 0 |
                        Tex.Data[IOffs + 1] << 8;

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

            return GetBitmap(Output, Tex.Width, Tex.Height);
        }

        public static Bitmap DecodeL8A8(Texture Tex)
        {
            byte[] Output = new byte[Tex.Width * Tex.Height * 4];

            int OOffset = 0;

            SwizzleAddr Swizzle = new SwizzleAddr(Tex.Width, Tex.Height, 0x20);

            for (int Y = 0; Y < Tex.Height; Y++)
            {
                for (int X = 0; X < Tex.Width; X++)
                {
                    int IOffs = Swizzle.GetSwizzledAddress16(X, Y) * 2;

                    Output[OOffset + 0] = Tex.Data[IOffs + 0];
                    Output[OOffset + 1] = Tex.Data[IOffs + 0];
                    Output[OOffset + 2] = Tex.Data[IOffs + 0];
                    Output[OOffset + 3] = Tex.Data[IOffs + 1];

                    OOffset += 4;
                }
            }

            return GetBitmap(Output, Tex.Width, Tex.Height);
        }

        public static Bitmap DecodeRGBA8888(Texture Tex)
        {
            byte[] Output = new byte[Tex.Width * Tex.Height * 4];

            int OOffset = 0;

            SwizzleAddr Swizzle = new SwizzleAddr(Tex.Width, Tex.Height, 0x10);

            for (int Y = 0; Y < Tex.Height; Y++)
            {
                for (int X = 0; X < Tex.Width; X++)
                {
                    int IOffs = Swizzle.GetSwizzledAddress32(X, Y) * 4;

                    Output[OOffset + 0] = Tex.Data[IOffs + 2];
                    Output[OOffset + 1] = Tex.Data[IOffs + 1];
                    Output[OOffset + 2] = Tex.Data[IOffs + 0];
                    Output[OOffset + 3] = Tex.Data[IOffs + 3];

                    OOffset += 4;
                }
            }

            return GetBitmap(Output, Tex.Width, Tex.Height);
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
