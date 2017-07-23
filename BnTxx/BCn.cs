using BnTxx.Utilities;
using System.Drawing;

namespace BnTxx
{
    static class BCn
    {
        public static Bitmap DecodeBC1(byte[] Data, int Width, int Height)
        {
            byte[] Output = new byte[Width * Height * 4];

            int XB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Width)  / 4);
            int YB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Height) / 4);

            for (int Y = 0; Y < Height / 4; Y++)
            {
                for (int X = 0; X < Width / 4; X++)
                {
                    int Offset = PixelDecoder.GetSwizzledAddressBC1(X, Y, XB, YB) * 8;

                    byte[] Tile = BCnDecodeTile(Data, Offset, true);

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * Width) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = Tile[TOffset + 3];

                            TOffset += 4;
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, Width, Height);
        }

        public static Bitmap DecodeBC2(byte[] Data, int Width, int Height)
        {
            byte[] Output = new byte[Width * Height * 4];

            int XB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Width)  / 4);
            int YB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Height) / 4);

            for (int Y = 0; Y < Height / 4; Y++)
            {
                for (int X = 0; X < Width / 4; X++)
                {
                    int Offset = PixelDecoder.GetSwizzledAddressBC2_3(X, Y, XB, YB) * 16;

                    byte[] Tile = BCnDecodeTile(Data, Offset + 8, false);

                    int AlphaLow  = Get32(Data, Offset + 0);
                    int AlphaHigh = Get32(Data, Offset + 4);

                    ulong AlphaCh = (uint)AlphaLow | (ulong)AlphaHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            ulong Alpha = (AlphaCh >> (TY * 16 + TX * 4)) & 0xf;

                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * Width) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = (byte)(Alpha | (Alpha << 4));

                            TOffset += 4;
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, Width, Height);
        }

        public static Bitmap DecodeBC3(byte[] Data, int Width, int Height)
        {
            byte[] Output = new byte[Width * Height * 4];

            int XB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Width)  / 4);
            int YB = BitUtils.CountZeros(BitUtils.Pow2RoundUp(Height) / 4);

            for (int Y = 0; Y < Height / 4; Y++)
            {
                for (int X = 0; X < Width / 4; X++)
                {
                    int Offset = PixelDecoder.GetSwizzledAddressBC2_3(X, Y, XB, YB) * 16;

                    byte[] Tile = BCnDecodeTile(Data, Offset + 8, false);

                    byte[] Alpha = new byte[8];

                    Alpha[0] = Data[Offset + 0];
                    Alpha[1] = Data[Offset + 1];

                    CalculateBC3Alpha(Alpha);

                    int AlphaLow  = Get32(Data, Offset + 2);
                    int AlphaHigh = Get16(Data, Offset + 6);

                    ulong AlphaCh = (uint)AlphaLow | (ulong)AlphaHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * Width) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = Alpha[(AlphaCh >> (TY * 12 + TX * 3)) & 7];

                            TOffset += 4;
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, Width, Height);
        }

        private static void CalculateBC3Alpha(byte[] Alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if (Alpha[0] > Alpha[1])
                {
                    Alpha[i] = (byte)(((8 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    Alpha[i] = (byte)(((6 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    Alpha[i] = 0;
                }
                else /* i == 7 */
                {
                    Alpha[i] = 0xff;
                }
            }
        }

        private static byte[] BCnDecodeTile(
            byte[] Input,
            int    Offset,
            bool   IsBC1)
        {
            Color[] CLUT = new Color[4];

            int c0 = Get16(Input, Offset + 0);
            int c1 = Get16(Input, Offset + 2);

            CLUT[0] = DecodeRGB565(c0);
            CLUT[1] = DecodeRGB565(c1);
            CLUT[2] = CalculateCLUT2(CLUT[0], CLUT[1], c0, c1, IsBC1);
            CLUT[3] = CalculateCLUT3(CLUT[0], CLUT[1], c0, c1, IsBC1);

            int Indices = Get32(Input, Offset + 4);

            int IdxShift = 0;

            byte[] Output = new byte[4 * 4 * 4];

            int OOffset = 0;

            for (int TY = 0; TY < 4; TY++)
            {
                for (int TX = 0; TX < 4; TX++)
                {
                    int Idx = (Indices >> IdxShift) & 3;

                    IdxShift += 2;

                    Color Pixel = CLUT[Idx];

                    Output[OOffset + 0] = Pixel.B;
                    Output[OOffset + 1] = Pixel.G;
                    Output[OOffset + 2] = Pixel.R;
                    Output[OOffset + 3] = Pixel.A;

                    OOffset += 4;
                }
            }

            return Output;
        }

        private static Color CalculateCLUT2(Color C0, Color C1, int c0, int c1, bool IsBC1)
        {
            if (c0 > c1 || !IsBC1)
            {
                return Color.FromArgb(
                    (2 * C0.R + C1.R) / 3,
                    (2 * C0.G + C1.G) / 3,
                    (2 * C0.B + C1.B) / 3);
            }
            else
            {
                return Color.FromArgb(
                    (C0.R + C1.R) / 2,
                    (C0.G + C1.G) / 2,
                    (C0.B + C1.B) / 2);
            }
        }

        private static Color CalculateCLUT3(Color C0, Color C1, int c0, int c1, bool IsBC1)
        {
            if (c0 > c1 || !IsBC1)
            {
                return
                    Color.FromArgb(
                        (2 * C1.R + C0.R) / 3,
                        (2 * C1.G + C0.G) / 3,
                        (2 * C1.B + C0.B) / 3);
            }

            return Color.Transparent;
        }

        private static Color DecodeRGB565(int Value)
        {
            int B = ((Value >>  0) & 0x1f) << 3;
            int G = ((Value >>  5) & 0x3f) << 2;
            int R = ((Value >> 11) & 0x1f) << 3;

            return Color.FromArgb(
                R | (R >> 5),
                G | (G >> 6),
                B | (B >> 5));
        }

        private static int Get16(byte[] Data, int Address)
        {
            return
                Data[Address + 0] << 0 |
                Data[Address + 1] << 8;
        }

        private static int Get32(byte[] Data, int Address)
        {
            return
                Data[Address + 0] <<  0 |
                Data[Address + 1] <<  8 |
                Data[Address + 2] << 16 |
                Data[Address + 3] << 24;
        }
    }
}