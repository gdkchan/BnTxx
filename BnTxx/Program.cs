﻿using BnTxx.Formats;
using System;
using System.Drawing;
using System.IO;

namespace BnTxx
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("BnTxx - Switch Binary Texture extractor by gdkchan");
            Console.WriteLine("Version 0.3.0\n");

            Console.ResetColor();

            if (args.Length > 0 && File.Exists(args[0]))
            {
                using (FileStream FS = new FileStream(args[0], FileMode.Open))
                {
                    BinaryTexture BT = new BinaryTexture(FS);

                    if (args.Length > 1)
                    {
                        switch (args[1].ToLower())
                        {
                            case "-list":
                                const string InfoFmt = "{0,-40}{1,-7}{2,-7}{3,-7}{4,-9}{5,-9}";

                                Console.WriteLine(InfoFmt,
                                    "Name",
                                    "Width",
                                    "Height",
                                    "Mips",
                                    "Type",
                                    "Format");

                                Console.WriteLine(InfoFmt,
                                    "------",
                                    "------",
                                    "------",
                                    "------",
                                    "------",
                                    "------");

                                foreach (var Tex in BT.Textures)
                                {
                                    Console.WriteLine(InfoFmt,
                                        Tex.Name,
                                        Tex.Width,
                                        Tex.Height,
                                        Tex.MipmapCount,
                                        Tex.Type,
                                        Tex.FormatType);
                                }
                                break;

                            case "-x":
                                foreach (var Tex in BT.Textures)
                                {
                                    ExtractTex(Tex, Path.Combine(args[2], Tex.Name + ".png"));
                                }
                                break;

                            case "-xtex": ExtractTex(BT.Textures[int.Parse(args[2])], args[3]); break;

                            default: PrintUsage(); break;
                        }
                    }
                    else
                    {
                        foreach (var Tex in BT.Textures)
                        {
                            ExtractTex(Tex);
                        }
                    }
                }
            }
            else if (args.Length > 0 && Directory.Exists(args[0]) == true)
            {
                // Get a list of all texture files from the directory.
                string[] textureFiles = Directory.GetFiles(args[0], "*.bntx", SearchOption.AllDirectories);
                for (int i = 0; i < textureFiles.Length; i++)
                {
                    // Open the texture for reading.
                    using (FileStream FS = new FileStream(textureFiles[i], FileMode.Open))
                    {
                        // Decode the texture so we can save it.
                        BinaryTexture BT = new BinaryTexture(FS);

                        // Extract the texture.
                        if (BT.Textures.Count > 1)
                        {
                            // Create a new directory with the name of the texture file.
                            string extractionFolder = string.Format("{0}\\{1}", Path.GetDirectoryName(textureFiles[i]), Path.GetFileNameWithoutExtension(textureFiles[i]));
                            Directory.CreateDirectory(extractionFolder);

                            // Extract the textures to the new directory.
                            for (int x = 0; x < BT.Textures.Count; x++)
                                ExtractTex(BT.Textures[x], Path.Combine(extractionFolder, BT.Textures[x].Name + ".png"));
                        }
                        else
                        {
                            // Extract the texture and preserve the original file name.
                            ExtractTex(BT.Textures[0], string.Format("{0}\\{1}.png", Path.GetDirectoryName(textureFiles[i]), Path.GetFileNameWithoutExtension(textureFiles[i])));
                        }
                    }
                }
            }
            else
            {
                PrintUsage();
            }
        }

        static void ExtractTex(Texture Tex)
        {
            ExtractTex(Tex, Tex.Name + ".png");
        }

        static void ExtractTex(Texture Tex, string FileName)
        {
            if (Tex.FormatType >= TextureFormatType.ASTC4x4 &&
                Tex.FormatType <= TextureFormatType.ASTC12x12)
            {
                Console.WriteLine("Extracting " + Tex.Name + " (ASTC)...");

                FileName = FileName.Replace(Path.GetExtension(FileName), ".astc");

                ASTC.Save(Tex, FileName);
            }
            else
            {
                Console.WriteLine("Extracting " + Tex.Name + "...");

                void PrintWarning()
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("ERROR: Texture " + Tex.Name + " have a unsupported format!");

                    Console.ResetColor();
                }

                if (Tex.Type == TextureType.Cube)
                {
                    int Length = Tex.Data.Length / Tex.ArrayCount;

                    int Offset = 0;

                    string[] CubeFaces = new string[] { "x+", "x-", "y+", "y-", "z+", "z-" };

                    for (int Index = 0; Index < Tex.ArrayCount; Index++)
                    {
                        if (!PixelDecoder.TryDecode(Tex, out Bitmap Img, Offset))
                        {
                            PrintWarning();

                            break;
                        }

                        string Ext = Path.GetExtension(FileName);

                        Img.Save(FileName.Replace(Ext, "." + CubeFaces[Index] + Ext));

                        Offset += Length;
                    }
                }
                else
                {
                    if (Tex.MipmapCount == 1)
                    {
                        if (PixelDecoder.TryDecode(Tex, out Bitmap Img))
                        {
                            Img.Save(FileName);
                        }
                        else
                        {
                            PrintWarning();
                        }
                    }
                    else
                    {
                        for (int Index = 0; Index < Tex.MipmapCount; Index++)
                        {
                            if (!PixelDecoder.TryDecode(Tex, out Bitmap Img, (int)Tex.MipOffsets[Index]))
                            {
                                PrintWarning();

                                break;
                            }

                            string Ext = Path.GetExtension(FileName);

                            Img.Save(FileName.Replace(Ext, "." + Index + "." + Ext));

                            Tex.Width  = Math.Max(Tex.Width  >> 1, 1);
                            Tex.Height = Math.Max(Tex.Height >> 1, 1);

                            while (Tex.GetBlockHeight() * 8 > Tex.GetPow2HeightInTexels() && Tex.BlockHeightLog2 > 0)
                            {
                                Tex.BlockHeightLog2--;
                            }
                        }
                    }
                    
                }
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Please pass the *.bntx file name as an argument to this tool!");
        }
    }
}
