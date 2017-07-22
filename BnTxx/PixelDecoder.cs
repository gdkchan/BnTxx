using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BnTxx
{
    static class PixelDecoder
    {
        public static Bitmap DecodeRGBA8888(byte[] Buffer, int Width, int Height)
        {
            byte[] Output = new byte[Width * Height * 4];

            const int TS = 8;

            int tiledw = Width / TS;
            int tiledh = Height / TS;

            for (int Y = 0; Y < Height; Y += TS)
            {
                for (int X = 0; X < Width; X += TS)
                {
                    for (int TY = 0; TY < TS; TY++)
                    {
                        for (int TX = 0; TX < TS; TX++)
                        {
                            int toffs = (X / TS) + (Y / TS) * tiledw; 

                            toffs *= TS * TS;

                            int OOffset = (X + TX + ((Y + TY) * Width)) * 4;

                            int IOffs = (toffs + (TX + TY * TS)) * 4; //(X + TX + ((Y + TY) * Width)) * 4;

                            Output[OOffset + 0] = Buffer[IOffs + 0];
                            Output[OOffset + 1] = Buffer[IOffs + 1];
                            Output[OOffset + 2] = Buffer[IOffs + 2];
                            Output[OOffset + 3] = Buffer[IOffs + 3];
                        }
                    }
                }
            }

            return GetBitmap(Output, Width, Height);
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
