using BnTxx.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System;

namespace BnTxx.Formats
{
    public class BinaryTexture : IList<Texture>
    {
        public List<Texture> Textures;

        private PatriciaTree NameTree;

        public Texture this[int Index]
        {
            get => Textures[Index];
            set => Textures[Index] = value;
        }

        public int Count => Textures.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /* Initialization and loading */

        public BinaryTexture()
        {
            Textures = new List<Texture>();

            NameTree = new PatriciaTree();
        }

        public BinaryTexture(Stream Data) : this()
        {
            BinaryReader Reader = new BinaryReader(Data);

            string BnTxSignature = Reader.ReadString(8);

            if (BnTxSignature != "BNTX")
            {
                throw new InvalidSignatureException("BNTX", BnTxSignature);
            }

            uint   DataLength     = Reader.ReadUInt32();
            ushort ByteOrderMark  = Reader.ReadUInt16();
            ushort Unknown0E      = Reader.ReadUInt16();
            uint   NameAddress    = Reader.ReadUInt32();
            ushort Unknown14      = Reader.ReadUInt16();
            ushort StringsAddress = Reader.ReadUInt16();
            uint   RelocAddress   = Reader.ReadUInt32();
            uint   FileLength     = Reader.ReadUInt32();
            uint   Unknown20      = Reader.ReadUInt32();

            ReadBinaryTextureInfo(Reader);
        }

        private void ReadBinaryTextureInfo(BinaryReader Reader)
        {
            uint TexturesCount   = Reader.ReadUInt32();
            long InfoPtrsAddress = Reader.ReadInt64();
            long DataBlkAddress  = Reader.ReadInt64();
            long DictAddress     = Reader.ReadInt64();
            uint StrDictLength   = Reader.ReadUInt32();

            Reader.BaseStream.Seek(DictAddress, SeekOrigin.Begin);

            NameTree = new PatriciaTree(Reader);

            for (int Index = 0; Index < TexturesCount; Index++)
            {
                long Position = InfoPtrsAddress + Index * 8;

                Reader.BaseStream.Seek(Position,           SeekOrigin.Begin);
                Reader.BaseStream.Seek(Reader.ReadInt64(), SeekOrigin.Begin);

                string BrTISignature = Reader.ReadString(4);

                if (BrTISignature != "BRTI")
                {
                    throw new InvalidSignatureException("BRTI", BrTISignature);
                }

                uint   BrTILength0    = Reader.ReadUInt32();
                uint   BrTILength1    = Reader.ReadUInt32();
                uint   Unknown0C      = Reader.ReadUInt32();
                uint   Unknown10      = Reader.ReadUInt32();
                uint   Unknown14      = Reader.ReadUInt32();
                uint   Unknown18      = Reader.ReadUInt32();
                uint   Format         = Reader.ReadUInt32();
                uint   Unknown20      = Reader.ReadUInt32();
                int    Width          = Reader.ReadInt32();
                int    Height         = Reader.ReadInt32();
                uint   Unknown2C      = Reader.ReadUInt32();
                uint   Unknown30      = Reader.ReadUInt32();
                uint   Unknown34      = Reader.ReadUInt32();
                uint   Unknown38      = Reader.ReadUInt32();
                uint   Unknown3C      = Reader.ReadUInt32();
                uint   Unknown40      = Reader.ReadUInt32();
                uint   Unknown44      = Reader.ReadUInt32();
                uint   Unknown48      = Reader.ReadUInt32();
                uint   Unknown4C      = Reader.ReadUInt32();
                int    DataLength     = Reader.ReadInt32();
                uint   Unknown54      = Reader.ReadUInt32();
                uint   Unknown58      = Reader.ReadUInt32();
                uint   Unknown5C      = Reader.ReadUInt32();
                int    NameAddress    = Reader.ReadInt32();
                uint   Unknown64      = Reader.ReadUInt32();
                uint   Unknown68      = Reader.ReadUInt32();
                uint   Unknown6C      = Reader.ReadUInt32();
                uint   DataPtrAddress = Reader.ReadUInt32();

                Reader.BaseStream.Seek(NameAddress, SeekOrigin.Begin);

                string Name = Reader.ReadShortString();

                Reader.BaseStream.Seek(DataPtrAddress,     SeekOrigin.Begin);
                Reader.BaseStream.Seek(Reader.ReadInt64(), SeekOrigin.Begin);

                byte[] Data = Reader.ReadBytes(DataLength);

                Textures.Add(new Texture()
                {
                    Name   = Name,
                    Width  = Width,
                    Height = Height,
                    Data   = Data,
                    Format = (TextureFormat)Format
                });
            }
        }

        /* Public facing methods */

        public IEnumerator<Texture> GetEnumerator()
        {
            return Textures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Texture item)
        {
            Textures.Add(item);
        }

        public void Clear()
        {
            Textures.Clear();
        }

        public bool Contains(Texture item)
        {
            return Textures.Contains(item);
        }

        public void CopyTo(Texture[] array, int arrayIndex)
        {
            Textures.CopyTo(array, arrayIndex);
        }

        public int IndexOf(Texture item)
        {
            return Textures.IndexOf(item);
        }

        public void Insert(int index, Texture item)
        {
            Textures.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Textures.RemoveAt(index);
        }

        bool ICollection<Texture>.Remove(Texture item)
        {
            return Textures.Remove(item);
        }
    }
}
