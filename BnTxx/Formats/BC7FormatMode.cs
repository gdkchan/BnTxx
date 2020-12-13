using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BnTxx.Formats
{
    public enum BC7BlockMode : int
    {
        Mode0 = 0,
        Mode1,
        Mode2,
        Mode3,
        Mode4,
        Mode5,
        Mode6,
        Mode7,
        Mode8
    }

    public class ColorRGBAP
    {
        public int BitsR;
        public int BitsG;
        public int BitsB;
        public int BitsA;
        public int BitsP;

        public ColorRGBAP(int bitsR, int bitsG, int bitsB, int bitsA, int bitsP)
        {
            this.BitsR = bitsR;
            this.BitsG = bitsG;
            this.BitsB = bitsB;
            this.BitsA = bitsA;
            this.BitsP = bitsP;
        }
    }

    public class BC7FormatMode
    {
        public BC7BlockMode Mode;
        public int SubsetCount;
        public int PartitionBitCount;
        public int PBits;
        public int RotationBitCount;
        public int IndexModeBitCount;
        public int IndexBitCount1;
        public int IndexBitCount2;
        public ColorRGBAP BitsPerColor;

        public BC7FormatMode(BC7BlockMode mode, int subsetCount, int partitionBitCount, int pBits, 
            int rotationBitCount, int indexModeBitCount, int indexBitCount1, int indexBitCount2, ColorRGBAP bitsPerColor)
        {
            this.Mode = mode;
            this.SubsetCount = subsetCount;
            this.PartitionBitCount = partitionBitCount;
            this.PBits = pBits;
            this.RotationBitCount = rotationBitCount;
            this.IndexModeBitCount = indexModeBitCount;
            this.IndexBitCount1 = indexBitCount1;
            this.IndexBitCount2 = indexBitCount2;
            this.BitsPerColor = bitsPerColor;
        }

        public static readonly Dictionary<BC7BlockMode, BC7FormatMode> FormatModes = new Dictionary<BC7BlockMode, BC7FormatMode>
        {
            { BC7BlockMode.Mode0, new BC7FormatMode(BC7BlockMode.Mode0, 3, 4, 6, 0, 0, 3, 0, new ColorRGBAP(4, 4, 4, 0, 1)) },
            { BC7BlockMode.Mode1, new BC7FormatMode(BC7BlockMode.Mode1, 2, 6, 2, 0, 0, 3, 0, new ColorRGBAP(6, 6, 6, 0, 1)) },
            { BC7BlockMode.Mode2, new BC7FormatMode(BC7BlockMode.Mode2, 3, 6, 0, 0, 0, 2, 0, new ColorRGBAP(5, 5, 5, 0, 0)) },
            { BC7BlockMode.Mode3, new BC7FormatMode(BC7BlockMode.Mode3, 2, 6, 4, 0, 0, 2, 0, new ColorRGBAP(7, 7, 7, 0, 1)) },
            { BC7BlockMode.Mode4, new BC7FormatMode(BC7BlockMode.Mode4, 1, 0, 0, 2, 1, 2, 3, new ColorRGBAP(5, 5, 5, 6, 0)) },
            { BC7BlockMode.Mode5, new BC7FormatMode(BC7BlockMode.Mode5, 1, 0, 0, 2, 0, 2, 2, new ColorRGBAP(7, 7, 7, 8, 0)) },
            { BC7BlockMode.Mode6, new BC7FormatMode(BC7BlockMode.Mode6, 1, 0, 2, 0, 0, 4, 0, new ColorRGBAP(7, 7, 7, 7, 1)) },
            { BC7BlockMode.Mode7, new BC7FormatMode(BC7BlockMode.Mode7, 2, 6, 4, 0, 0, 2, 0, new ColorRGBAP(5, 5, 5, 5, 1)) },
            { BC7BlockMode.Mode8, new BC7FormatMode(BC7BlockMode.Mode8, 0, 0, 0, 0, 0, 0, 0, new ColorRGBAP(0, 0, 0, 0, 0)) }
        };
    }
}
