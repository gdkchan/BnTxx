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
            Console.WriteLine("Version 0.1.0\n");

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
                                const string InfoFmt = "{0,-48}{1,-8}{2,-8}{3,-8}{4,-8}";

                                Console.WriteLine(InfoFmt,
                                    "Name",
                                    "Width",
                                    "Height",
                                    "Format",
                                    "Length");

                                Console.WriteLine(InfoFmt,
                                    "----",
                                    "-----",
                                    "------",
                                    "------",
                                    "------");

                                foreach (var Tex in BT.Textures)
                                {
                                    Console.WriteLine(InfoFmt,
                                        Tex.Name,
                                        Tex.Width,
                                        Tex.Height,
                                        Tex.Format,
                                        Tex.Data.Length);
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
            Bitmap Img = null;

            switch (Tex.Format)
            {
                /*case TextureFormat.RGBA8888:
                    Img = PixelDecoder.DecodeRGBA8888(Tex.Data, Tex.Width, Tex.Height);
                    break;*/

                case TextureFormat.BC1:
                    Img = BCn.DecodeBC1(Tex.Data, Tex.Width, Tex.Height);
                    break;

                case TextureFormat.BC3:
                    Img = BCn.DecodeBC3(Tex.Data, Tex.Width, Tex.Height);
                    break;
            }

            if (Img != null)
            {
                Console.WriteLine("Extracting " + Tex.Name + "...");

                Img.Save(FileName);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("ERROR: Texture " + Tex.Name + " have a unsupported format!");

                Console.ResetColor();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Please pass the *.istorage file name as an argument to this tool!");
        }
    }
}
