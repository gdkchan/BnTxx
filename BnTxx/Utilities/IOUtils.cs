using System.IO;
using System.Text;

namespace BnTxx.Utilities
{
    static class IOUtils
    {
        public static string ReadString(this BinaryReader Reader, int Length)
        {
            if (Length > 0)
            {
                long Position = Reader.BaseStream.Position + Length;

                using (MemoryStream MS = new MemoryStream())
                {
                    for (byte Value; Length-- > 0 && (Value = Reader.ReadByte()) != 0;)
                    {
                        MS.WriteByte(Value);
                    }

                    Reader.BaseStream.Seek(Position, SeekOrigin.Begin);

                    return Encoding.UTF8.GetString(MS.ToArray());
                }
            }

            return null;
        }

        public static string ReadShortString(this BinaryReader Reader)
        {
            return Reader.ReadString(Reader.ReadUInt16());
        }
    }
}
