namespace BnTxx.Formats
{
    public struct Texture
    {
        public string Name;

        public int Width;
        public int Height;
        public int Faces;
        public int Mipmaps;

        public byte[] Data;

        public ChannelType Channel0Type;
        public ChannelType Channel1Type;
        public ChannelType Channel2Type;
        public ChannelType Channel3Type;

        public TextureType       Type;
        public TextureFormatType FormatType;
        public TextureFormatVar  FormatVariant;
    }
}
