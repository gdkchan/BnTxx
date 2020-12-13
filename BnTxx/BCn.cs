using BnTxx.Formats;
using BnTxx.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;

namespace BnTxx
{
    static class BCn
    {
        public static Bitmap DecodeBC1(Texture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            ISwizzle Swizzle = Tex.GetSwizzle();

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzleOffset(X, Y);

                    byte[] Tile = BCnDecodeTile(Tex.Data, IOffs, true);

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = Tile[TOffset + 3];

                            TOffset += 4;
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, W * 4, H * 4);
        }

        public static Bitmap DecodeBC2(Texture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            ISwizzle Swizzle = Tex.GetSwizzle();

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzleOffset(X, Y);

                    byte[] Tile = BCnDecodeTile(Tex.Data, IOffs + 8, false);

                    int AlphaLow  = IOUtils.Get32(Tex.Data, IOffs + 0);
                    int AlphaHigh = IOUtils.Get32(Tex.Data, IOffs + 4);

                    ulong AlphaCh = (uint)AlphaLow | (ulong)AlphaHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            ulong Alpha = (AlphaCh >> (TY * 16 + TX * 4)) & 0xf;

                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = (byte)(Alpha | (Alpha << 4));

                            TOffset += 4;
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, W * 4, H * 4);
        }

        public static Bitmap DecodeBC3(Texture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            ISwizzle Swizzle = Tex.GetSwizzle();

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzleOffset(X, Y);

                    byte[] Tile = BCnDecodeTile(Tex.Data, IOffs + 8, false);

                    byte[] Alpha = new byte[8];

                    Alpha[0] = Tex.Data[IOffs + 0];
                    Alpha[1] = Tex.Data[IOffs + 1];

                    CalculateBC3Alpha(Alpha);

                    int AlphaLow  = IOUtils.Get32(Tex.Data, IOffs + 2);
                    int AlphaHigh = IOUtils.Get16(Tex.Data, IOffs + 6);

                    ulong AlphaCh = (uint)AlphaLow | (ulong)AlphaHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            byte AlphaPx = Alpha[(AlphaCh >> (TY * 12 + TX * 3)) & 7];

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = AlphaPx;

                            TOffset += 4;
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, W * 4, H * 4);
        }

        public static Bitmap DecodeBC4(Texture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            ISwizzle Swizzle = Tex.GetSwizzle();

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzleOffset(X, Y);

                    byte[] Red = new byte[8];

                    Red[0] = Tex.Data[IOffs + 0];
                    Red[1] = Tex.Data[IOffs + 1];

                    CalculateBC3Alpha(Red);

                    int RedLow  = IOUtils.Get32(Tex.Data, IOffs + 2);
                    int RedHigh = IOUtils.Get16(Tex.Data, IOffs + 6);

                    ulong RedCh = (uint)RedLow | (ulong)RedHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            byte RedPx = Red[(RedCh >> (TY * 12 + TX * 3)) & 7];

                            Output[OOffset + 0] = RedPx;
                            Output[OOffset + 1] = RedPx;
                            Output[OOffset + 2] = RedPx;
                            Output[OOffset + 3] = 0xff;

                            TOffset += 4;
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, W * 4, H * 4);
        }

        public static Bitmap DecodeBC5(Texture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            ISwizzle Swizzle = Tex.GetSwizzle();

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzleOffset(X, Y);

                    byte[] Red   = new byte[8];
                    byte[] Green = new byte[8];

                    Red[0]   = Tex.Data[IOffs + 0];
                    Red[1]   = Tex.Data[IOffs + 1];

                    Green[0] = Tex.Data[IOffs + 8];
                    Green[1] = Tex.Data[IOffs + 9];

                    if (Tex.FormatVariant == TextureFormatVar.SNorm)
                    {
                        CalculateBC3AlphaS(Red);
                        CalculateBC3AlphaS(Green);
                    }
                    else
                    {
                        CalculateBC3Alpha(Red);
                        CalculateBC3Alpha(Green);
                    }

                    int RedLow    = IOUtils.Get32(Tex.Data, IOffs + 2);
                    int RedHigh   = IOUtils.Get16(Tex.Data, IOffs + 6);

                    int GreenLow  = IOUtils.Get32(Tex.Data, IOffs + 10);
                    int GreenHigh = IOUtils.Get16(Tex.Data, IOffs + 14);

                    ulong RedCh   = (uint)RedLow   | (ulong)RedHigh   << 32;
                    ulong GreenCh = (uint)GreenLow | (ulong)GreenHigh << 32;

                    int TOffset = 0;

                    if (Tex.FormatVariant == TextureFormatVar.SNorm)
                    {
                        for (int TY = 0; TY < 4; TY++)
                        {
                            for (int TX = 0; TX < 4; TX++)
                            {
                                int Shift = TY * 12 + TX * 3;

                                int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                                byte RedPx   = Red  [(RedCh   >> Shift) & 7];
                                byte GreenPx = Green[(GreenCh >> Shift) & 7];

                                if (Tex.FormatVariant == TextureFormatVar.SNorm)
                                {
                                    RedPx   += 0x80;
                                    GreenPx += 0x80;
                                }

                                float NX = (RedPx   / 255f) * 2 - 1;
                                float NY = (GreenPx / 255f) * 2 - 1;

                                float NZ = (float)Math.Sqrt(1 - (NX * NX + NY * NY));

                                Output[OOffset + 0] = Clamp((NZ + 1) * 0.5f);
                                Output[OOffset + 1] = Clamp((NY + 1) * 0.5f);
                                Output[OOffset + 2] = Clamp((NX + 1) * 0.5f);
                                Output[OOffset + 3] = 0xff;

                                TOffset += 4;
                            }
                        }
                    }
                    else
                    {
                        for (int TY = 0; TY < 4; TY++)
                        {
                            for (int TX = 0; TX < 4; TX++)
                            {
                                int Shift = TY * 12 + TX * 3;

                                int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                                byte RedPx   = Red  [(RedCh   >> Shift) & 7];
                                byte GreenPx = Green[(GreenCh >> Shift) & 7];

                                Output[OOffset + 0] = RedPx;
                                Output[OOffset + 1] = RedPx;
                                Output[OOffset + 2] = RedPx;
                                Output[OOffset + 3] = GreenPx;

                                TOffset += 4;
                            }
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, W * 4, H * 4);
        }

        public static Bitmap DecodeBC7(Texture Tex, int Offset)
        {
            int W = (Tex.Width + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            ISwizzle Swizzle = Tex.GetSwizzle();

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzleOffset(X, Y);

                    // Get the format mode for this block.
                    BC7BlockMode mode = (BC7BlockMode)BitUtils.CountZeros(Tex.Data[IOffs]);
                    BC7FormatMode format = BC7FormatMode.FormatModes[mode];

                    // Check the block mode and handle accordingly.
                    if (mode != BC7BlockMode.Mode8)
                    {
                        // Decode additional block parameters.
                        int startPos = (int)mode + 1;
                        int partition = BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, format.PartitionBitCount);
                        int rotation = BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, format.RotationBitCount);
                        int indexMode = BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, format.IndexModeBitCount);

                        // Safety checks until there are more test cases.
                        Debug.Assert(partition < 64);
                        Debug.Assert(rotation < 4);
                        Debug.Assert(indexMode < 2);

                        // Calculate the number of endpoints based on the format mode.
                        int endpointCount = format.SubsetCount * 2;

                        // Initialize the pixel Tex.Data.
                        BC7Color[] pixelBuffer = new BC7Color[endpointCount];
                        for (int i = 0; i < pixelBuffer.Length; i++)
                            pixelBuffer[i] = new BC7Color();

                        byte[] pValues = new byte[6];

                        // Read the red color bits.
                        for (int i = 0; i < endpointCount; i++)
                        {
                            Debug.Assert(startPos + format.BitsPerColor.BitsR <= 128);

                            pixelBuffer[i].r = (byte)BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, format.BitsPerColor.BitsR);
                        }

                        // Read the green color bits.
                        for (int i = 0; i < endpointCount; i++)
                        {
                            Debug.Assert(startPos + format.BitsPerColor.BitsG <= 128);

                            pixelBuffer[i].g = (byte)BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, format.BitsPerColor.BitsG);
                        }

                        // Read the blue color bits.
                        for (int i = 0; i < endpointCount; i++)
                        {
                            Debug.Assert(startPos + format.BitsPerColor.BitsB <= 128);

                            pixelBuffer[i].b = (byte)BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, format.BitsPerColor.BitsB);
                        }

                        // Read the alpha color bits.
                        for (int i = 0; i < endpointCount; i++)
                        {
                            Debug.Assert(startPos + format.BitsPerColor.BitsA <= 128);

                            if (format.BitsPerColor.BitsA != 0)
                                pixelBuffer[i].a = (byte)BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, format.BitsPerColor.BitsA);
                            else
                                pixelBuffer[i].a = 255;
                        }

                        // Read the P bits.
                        for (int i = 0; i < format.PBits; i++)
                        {
                            Debug.Assert(startPos < 128);

                            pValues[i] = (byte)BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, 1);
                        }

                        // If there are P bits then adjust the pixel colors accordingly.
                        if (format.PBits > 0)
                        {
                            for (int i = 0; i < endpointCount; i++)
                            {
                                // Calculate the P value index.
                                int pIndex = (i * format.PBits) / endpointCount;

                                // Mask in the P bit as the LSB in the color value.
                                pixelBuffer[i].r = (byte)((pixelBuffer[i].r << 1) | pValues[pIndex]);
                                pixelBuffer[i].g = (byte)((pixelBuffer[i].g << 1) | pValues[pIndex]);
                                pixelBuffer[i].b = (byte)((pixelBuffer[i].b << 1) | pValues[pIndex]);
                                pixelBuffer[i].a = (byte)((pixelBuffer[i].a << 1) | pValues[pIndex]);
                            }
                        }

                        // Adjust the colors for precision based on the P bits.
                        for (int i = 0; i < endpointCount; i++)
                        {
                            Unquantize(ref pixelBuffer[i], format.BitsPerColor);
                        }

                        // Read the color indices.
                        int[] colorWeights = new int[16];
                        for (int i = 0; i < 16; i++)
                        {
                            int bitCount = (IsFixUpOffset(format.SubsetCount, partition, i) ? format.IndexBitCount1 - 1 : format.IndexBitCount1);

                            Debug.Assert(startPos + bitCount <= 128);

                            colorWeights[i] = BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, bitCount);
                            Debug.Assert(colorWeights[i] < 64);
                        }

                        // Read the alpha indices.
                        int[] alphaWeights = new int[16];
                        if (format.IndexBitCount2 > 0)
                        {
                            for (int i = 0; i < 16; i++)
                            {
                                int bitCount = (i > 0 ? format.IndexBitCount2 : format.IndexBitCount2 - 1);

                                Debug.Assert(startPos + bitCount <= 128);

                                alphaWeights[i] = BitUtils.GetBits32(Tex.Data, 16, IOffs, ref startPos, bitCount);
                            }
                        }

                        // Interpolate the colors.
                        int OOffset = (X * 4 + (Y * 4) * W * 4) * 4;
                        for (int i = 0; i < 16; i++)
                        {
                            BC7Color newColor;

                            // Get the region for this pixel.
                            byte region = g_aPartitionTable[format.SubsetCount - 1][partition][i];
                            if (format.IndexBitCount2 > 0)
                            {
                                if (indexMode == 0)
                                    newColor = Interpolate(pixelBuffer[region * 2], pixelBuffer[(region * 2) + 1], colorWeights[i], alphaWeights[i], format.IndexBitCount1, format.IndexBitCount2);
                                else
                                    newColor = Interpolate(pixelBuffer[region * 2], pixelBuffer[(region * 2) + 1], alphaWeights[i], colorWeights[i], format.IndexBitCount2, format.IndexBitCount1);
                            }
                            else
                            {
                                newColor = Interpolate(pixelBuffer[region * 2], pixelBuffer[(region * 2) + 1], colorWeights[i], colorWeights[i], format.IndexBitCount1, format.IndexBitCount1);
                            }

                            // Handle the rotation.
                            switch (rotation)
                            {
                                case 1: Swap(ref newColor.r, ref newColor.a); break;
                                case 2: Swap(ref newColor.g, ref newColor.a); break;
                                case 3: Swap(ref newColor.b, ref newColor.a); break;
                            }

                            Output[OOffset + 0] = newColor.b;
                            Output[OOffset + 1] = newColor.g;
                            Output[OOffset + 2] = newColor.r;
                            Output[OOffset + 3] = newColor.a;

                            OOffset += 4;

                            if (((i + 1) & 3) == 0)
                                OOffset += 4 * ((W * 4) - 4);
                        }
                    }
                    else
                    {
                        // Fill the block with empty bytes.
                        int OOffset = (X * 4 + (Y * 4) * W * 4) * 4;
                        for (int i = 0; i < 16; i++)
                        {
                            Output[OOffset + 0] = 0;
                            Output[OOffset + 1] = 0;
                            Output[OOffset + 2] = 0;
                            Output[OOffset + 3] = 0;
                            OOffset += 4;

                            if (((i + 1) & 3) == 0)
                                OOffset += 4 * ((W * 4) - 4);
                        }
                    }
                }
            }

            return PixelDecoder.GetBitmap(Output, W * 4, H * 4);
        }

        private static void Swap(ref byte b1, ref byte b2)
        {
            byte temp = b1;
            b1 = b2;
            b2 = temp;
        }

        private static byte Clamp(float Value)
        {
            if (Value > 1)
            {
                return 0xff;
            }
            else if (Value < 0)
            {
                return 0;
            }
            else
            {
                return (byte)(Value * 0xff);
            }
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

        private static void CalculateBC3AlphaS(byte[] Alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if ((sbyte)Alpha[0] > (sbyte)Alpha[1])
                {
                    Alpha[i] = (byte)(((8 - i) * (sbyte)Alpha[0] + (i - 1) * (sbyte)Alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    Alpha[i] = (byte)(((6 - i) * (sbyte)Alpha[0] + (i - 1) * (sbyte)Alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    Alpha[i] = 0x80;
                }
                else /* i == 7 */
                {
                    Alpha[i] = 0x7f;
                }
            }
        }

        private static byte[] BCnDecodeTile(
            byte[] Input,
            int    Offset,
            bool   IsBC1)
        {
            Color[] CLUT = new Color[4];

            int c0 = IOUtils.Get16(Input, Offset + 0);
            int c1 = IOUtils.Get16(Input, Offset + 2);

            CLUT[0] = DecodeRGB565(c0);
            CLUT[1] = DecodeRGB565(c1);
            CLUT[2] = CalculateCLUT2(CLUT[0], CLUT[1], c0, c1, IsBC1);
            CLUT[3] = CalculateCLUT3(CLUT[0], CLUT[1], c0, c1, IsBC1);

            int Indices = IOUtils.Get32(Input, Offset + 4);

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

        private static byte Unquantize(byte comp, byte prec)
        {
            comp = (byte)(comp << (8 - prec));
            return (byte)(comp | (comp >> prec));
        }

        private static void Unquantize(ref BC7Color color, ColorRGBAP prec)
        {
            color.r = Unquantize(color.r, (byte)(prec.BitsR + prec.BitsP));
            color.g = Unquantize(color.g, (byte)(prec.BitsG + prec.BitsP));
            color.b = Unquantize(color.b, (byte)(prec.BitsB + prec.BitsP));
            color.a = (prec.BitsA > 0 ? Unquantize(color.a, (byte)(prec.BitsA + prec.BitsP)) : (byte)255);
        }

        public static bool IsFixUpOffset(int subsetCount, int partition, int offset)
        {
            for (int i = 0; i < subsetCount; i++)
            {
                if (g_aFixUp[subsetCount - 1][partition][i] == offset)
                    return true;
            }

            return false;
        }

        private static BC7Color Interpolate(BC7Color color1, BC7Color color2, int colorWeightIdx, int alphaWeightIdx, int colorPrec, int alphaPrec)
        {
            int[][] ColorWeights = new int[][] { g_aWeights2, g_aWeights3, g_aWeights4 };

            Debug.Assert(colorPrec >= 2 && colorPrec <= 4);
            Debug.Assert(alphaPrec >= 2 && alphaPrec <= 4);

            // Get the weight values.
            int colorWeight = ColorWeights[colorPrec - 2][colorWeightIdx];
            int alphaWeight = ColorWeights[alphaPrec - 2][alphaWeightIdx];

            // Perform the interpolation.
            byte r = (byte)(((int)color1.r * (64 - colorWeight) + (int)color2.r * colorWeight + 32) >> 6);
            byte g = (byte)(((int)color1.g * (64 - colorWeight) + (int)color2.g * colorWeight + 32) >> 6);
            byte b = (byte)(((int)color1.b * (64 - colorWeight) + (int)color2.b * colorWeight + 32) >> 6);
            byte a = (byte)(((int)color1.a * (64 - alphaWeight) + (int)color2.a * alphaWeight + 32) >> 6);

            return new BC7Color(a, r, g, b);
        }

        // Partition, Shape, Pixel (index into 4x4 block)
        public static readonly byte[][][] g_aPartitionTable = new byte[3][][]
        {
            new byte[64][] {   // 1 Region case has no subsets (all 0)
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },

            new byte[64][] {   // BC6H/BC7 Partition Set for 2 Subsets
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 }, // Shape 0
                new byte[16] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 }, // Shape 1
                new byte[16] { 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 }, // Shape 2
                new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 3
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1 }, // Shape 4
                new byte[16] { 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 5
                new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 6
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 7
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1 }, // Shape 8
                new byte[16] { 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 9
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 10
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1 }, // Shape 11
                new byte[16] { 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 12
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 13
                new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 14
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 }, // Shape 15
                new byte[16] { 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1 }, // Shape 16
                new byte[16] { 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, // Shape 17
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0 }, // Shape 18
                new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0 }, // Shape 19
                new byte[16] { 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, // Shape 20
                new byte[16] { 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0 }, // Shape 21
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0 }, // Shape 22
                new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1 }, // Shape 23
                new byte[16] { 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0 }, // Shape 24
                new byte[16] { 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0 }, // Shape 25
                new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0 }, // Shape 26
                new byte[16] { 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0 }, // Shape 27
                new byte[16] { 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0 }, // Shape 28
                new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 }, // Shape 29
                new byte[16] { 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0 }, // Shape 30
                new byte[16] { 0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0 }, // Shape 31

                // BC7 Partition Set for 2 Subsets (second-half)
                new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }, // Shape 32
                new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1 }, // Shape 33
                new byte[16] { 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0 }, // Shape 34
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0 }, // Shape 35
                new byte[16] { 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0 }, // Shape 36
                new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0 }, // Shape 37
                new byte[16] { 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1 }, // Shape 38
                new byte[16] { 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1 }, // Shape 39
                new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0 }, // Shape 40
                new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0 }, // Shape 41
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0 }, // Shape 42
                new byte[16] { 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0 }, // Shape 43
                new byte[16] { 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0 }, // Shape 44
                new byte[16] { 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1 }, // Shape 45
                new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1 }, // Shape 46
                new byte[16] { 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0 }, // Shape 47
                new byte[16] { 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, // Shape 48
                new byte[16] { 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0 }, // Shape 49
                new byte[16] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0 }, // Shape 50
                new byte[16] { 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0 }, // Shape 51
                new byte[16] { 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1 }, // Shape 52
                new byte[16] { 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1 }, // Shape 53
                new byte[16] { 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0 }, // Shape 54
                new byte[16] { 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0 }, // Shape 55
                new byte[16] { 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 1 }, // Shape 56
                new byte[16] { 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1 }, // Shape 57
                new byte[16] { 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1 }, // Shape 58
                new byte[16] { 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1 }, // Shape 59
                new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 }, // Shape 60
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 }, // Shape 61
                new byte[16] { 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0 }, // Shape 62
                new byte[16] { 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1 }  // Shape 63
            },

            new byte[64][] {   // BC7 Partition Set for 3 Subsets
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 1, 2, 2, 2, 2 }, // Shape 0
                new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1 }, // Shape 1
                new byte[16] { 0, 0, 0, 0, 2, 0, 0, 1, 2, 2, 1, 1, 2, 2, 1, 1 }, // Shape 2
                new byte[16] { 0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 3
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2 }, // Shape 4
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 2, 2 }, // Shape 5
                new byte[16] { 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 6
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1 }, // Shape 7
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2 }, // Shape 8
                new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2 }, // Shape 9
                new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 10
                new byte[16] { 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2 }, // Shape 11
                new byte[16] { 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2 }, // Shape 12
                new byte[16] { 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2 }, // Shape 13
                new byte[16] { 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2, 1, 2, 2, 2 }, // Shape 14
                new byte[16] { 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0, 2, 2, 2, 0 }, // Shape 15
                new byte[16] { 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2 }, // Shape 16
                new byte[16] { 0, 1, 1, 1, 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0 }, // Shape 17
                new byte[16] { 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2 }, // Shape 18
                new byte[16] { 0, 0, 2, 2, 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1 }, // Shape 19
                new byte[16] { 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2 }, // Shape 20
                new byte[16] { 0, 0, 0, 1, 0, 0, 0, 1, 2, 2, 2, 1, 2, 2, 2, 1 }, // Shape 21
                new byte[16] { 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2 }, // Shape 22
                new byte[16] { 0, 0, 0, 0, 1, 1, 0, 0, 2, 2, 1, 0, 2, 2, 1, 0 }, // Shape 23
                new byte[16] { 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1, 0, 0, 0, 0 }, // Shape 24
                new byte[16] { 0, 0, 1, 2, 0, 0, 1, 2, 1, 1, 2, 2, 2, 2, 2, 2 }, // Shape 25
                new byte[16] { 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1, 0, 1, 1, 0 }, // Shape 26
                new byte[16] { 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1 }, // Shape 27
                new byte[16] { 0, 0, 2, 2, 1, 1, 0, 2, 1, 1, 0, 2, 0, 0, 2, 2 }, // Shape 28
                new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 2, 0, 0, 2, 2, 2, 2, 2 }, // Shape 29
                new byte[16] { 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1 }, // Shape 30
                new byte[16] { 0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 1, 1, 2, 2, 2, 1 }, // Shape 31
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 2, 2, 2 }, // Shape 32
                new byte[16] { 0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 2, 0, 0, 1, 1 }, // Shape 33
                new byte[16] { 0, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 2, 2, 2 }, // Shape 34
                new byte[16] { 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0 }, // Shape 35
                new byte[16] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0 }, // Shape 36
                new byte[16] { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0 }, // Shape 37
                new byte[16] { 0, 1, 2, 0, 2, 0, 1, 2, 1, 2, 0, 1, 0, 1, 2, 0 }, // Shape 38
                new byte[16] { 0, 0, 1, 1, 2, 2, 0, 0, 1, 1, 2, 2, 0, 0, 1, 1 }, // Shape 39
                new byte[16] { 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0, 1, 1 }, // Shape 40
                new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 41
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1 }, // Shape 42
                new byte[16] { 0, 0, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2, 1, 1, 2, 2 }, // Shape 43
                new byte[16] { 0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1 }, // Shape 44
                new byte[16] { 0, 2, 2, 0, 1, 2, 2, 1, 0, 2, 2, 0, 1, 2, 2, 1 }, // Shape 45
                new byte[16] { 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 0, 1 }, // Shape 46
                new byte[16] { 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1 }, // Shape 47
                new byte[16] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2 }, // Shape 48
                new byte[16] { 0, 2, 2, 2, 0, 1, 1, 1, 0, 2, 2, 2, 0, 1, 1, 1 }, // Shape 49
                new byte[16] { 0, 0, 0, 2, 1, 1, 1, 2, 0, 0, 0, 2, 1, 1, 1, 2 }, // Shape 50
                new byte[16] { 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2 }, // Shape 51
                new byte[16] { 0, 2, 2, 2, 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2 }, // Shape 52
                new byte[16] { 0, 0, 0, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2 }, // Shape 53
                new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2 }, // Shape 54
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2 }, // Shape 55
                new byte[16] { 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 56
                new byte[16] { 0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2 }, // Shape 57
                new byte[16] { 0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2 }, // Shape 58
                new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2 }, // Shape 59
                new byte[16] { 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1 }, // Shape 60
                new byte[16] { 0, 2, 2, 2, 1, 2, 2, 2, 0, 2, 2, 2, 1, 2, 2, 2 }, // Shape 61
                new byte[16] { 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 62
                new byte[16] { 0, 1, 1, 1, 2, 0, 1, 1, 2, 2, 0, 1, 2, 2, 2, 0 }  // Shape 63
            }
        };

        // Partition, Shape, Fixup
        public static readonly byte[][][] g_aFixUp = new byte[3][][]
        {
            new byte[64][] {   // No fix-ups for 1st subset for BC6H or BC7
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },
                new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 },new byte[] { 0, 0, 0 }
            },

            new byte[64][] {   // BC6H/BC7 Partition Set Fixups for 2 Subsets
                new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
                new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
                new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
                new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
                new byte[] { 0,15, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },
                new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 8, 0 },new byte[] { 0,15, 0 },
                new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },
                new byte[] { 0, 8, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },

                // BC7 Partition Set Fixups for 2 Subsets (second-half)
                new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0, 6, 0 },new byte[] { 0, 8, 0 },
                new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
                new byte[] { 0, 2, 0 },new byte[] { 0, 8, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },
                new byte[] { 0, 2, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0, 6, 0 },
                new byte[] { 0, 6, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 6, 0 },new byte[] { 0, 8, 0 },
                new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },
                new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },new byte[] { 0,15, 0 },
                new byte[] { 0,15, 0 },new byte[] { 0, 2, 0 },new byte[] { 0, 2, 0 },new byte[] { 0,15, 0 }
            },

            new byte[64][] {   // BC7 Partition Set Fixups for 3 Subsets
                new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },new byte[] { 0,15, 8 },new byte[] { 0,15, 3 },
                new byte[] { 0, 8,15 },new byte[] { 0, 3,15 },new byte[] { 0,15, 3 },new byte[] { 0,15, 8 },
                new byte[] { 0, 8,15 },new byte[] { 0, 8,15 },new byte[] { 0, 6,15 },new byte[] { 0, 6,15 },
                new byte[] { 0, 6,15 },new byte[] { 0, 5,15 },new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },
                new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },new byte[] { 0, 8,15 },new byte[] { 0,15, 3 },
                new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 },new byte[] { 0, 6,15 },new byte[] { 0,10, 8 },
                new byte[] { 0, 5, 3 },new byte[] { 0, 8,15 },new byte[] { 0, 8, 6 },new byte[] { 0, 6,10 },
                new byte[] { 0, 8,15 },new byte[] { 0, 5,15 },new byte[] { 0,15,10 },new byte[] { 0,15, 8 },
                new byte[] { 0, 8,15 },new byte[] { 0,15, 3 },new byte[] { 0, 3,15 },new byte[] { 0, 5,10 },
                new byte[] { 0, 6,10 },new byte[] { 0,10, 8 },new byte[] { 0, 8, 9 },new byte[] { 0,15,10 },
                new byte[] { 0,15, 6 },new byte[] { 0, 3,15 },new byte[] { 0,15, 8 },new byte[] { 0, 5,15 },
                new byte[] { 0,15, 3 },new byte[] { 0,15, 6 },new byte[] { 0,15, 6 },new byte[] { 0,15, 8 },
                new byte[] { 0, 3,15 },new byte[] { 0,15, 3 },new byte[] { 0, 5,15 },new byte[] { 0, 5,15 },
                new byte[] { 0, 5,15 },new byte[] { 0, 8,15 },new byte[] { 0, 5,15 },new byte[] { 0,10,15 },
                new byte[] { 0, 5,15 },new byte[] { 0,10,15 },new byte[] { 0, 8,15 },new byte[] { 0,13,15 },
                new byte[] { 0,15, 3 },new byte[] { 0,12,15 },new byte[] { 0, 3,15 },new byte[] { 0, 3, 8 }
            }
        };

        public static readonly int[] g_aWeights2 = { 0, 21, 43, 64 };
        public static readonly int[] g_aWeights3 = { 0, 9, 18, 27, 37, 46, 55, 64 };
        public static readonly int[] g_aWeights4 = { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };
    }
}