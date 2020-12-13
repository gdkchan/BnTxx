using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BnTxx.Utilities
{
    public class BC7Color
    {
        public byte a, r, g, b;

        public BC7Color()
        {

        }

        public BC7Color(byte a, byte r, byte g, byte b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}
