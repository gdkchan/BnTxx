using BnTxx.Formats;
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
            Console.WriteLine("Version 0.1.6\n");

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
                                        Tex.Mipmaps,
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

                Bitmap Img = null;

                if (Tex.Type == TextureType.Cube)
                {
                    int Length = Tex.Data.Length / Tex.Faces;

                    int Offset = 0;

                    string[] CubeFaces = new string[] { "x+", "x-", "y+", "y-", "z+", "z-" };

                    for (int Index = 0; Index < Tex.Faces; Index++)
                    {
                        if (!PixelDecoder.TryDecode(Tex, out Img, Offset))
                        {
                            break;
                        }

                        string Ext = Path.GetExtension(FileName);

                        Img.Save(FileName.Replace(Ext, "." + CubeFaces[Index] + Ext));

                        Offset += Length;
                    }
                }
                else
                {
                    PixelDecoder.TryDecode(Tex, out Img);
                }

                if (Img != null)
                {
                    Img.Save(FileName);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("ERROR: Texture " + Tex.Name + " have a unsupported format!");

                    Console.ResetColor();
                }
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Please pass the *.bntx file name as an argument to this tool!");
        }
    }
}
