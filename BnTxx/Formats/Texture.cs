namespace BnTxx.Formats
{
    public struct Texture
    {
        public string Name;

        public int Width;
        public int Height;
        public int BlockSize;
        public int Mipmaps;

        public byte[] Data;

        public TextureFormat Format;
    }
}
