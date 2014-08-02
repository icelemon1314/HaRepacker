namespace WzLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class WzDirectory : IWzObject, IDisposable
    {
        internal uint blockStartOffset;
        internal int checksum;
        internal int hash;
        internal uint headerOffsetOffset;
        internal List<WzImage> images;
        internal string name;
        internal uint offset;
        internal int offsetSize;
        internal IWzObject parent;
        internal int size;
        internal List<WzDirectory> subDirs;
        internal BinaryReader wzReader;

        public WzDirectory()
        {
            this.images = new List<WzImage>();
            this.subDirs = new List<WzDirectory>();
            this.checksum = 0xc032f7a;
        }

        public WzDirectory(string name)
        {
            this.images = new List<WzImage>();
            this.subDirs = new List<WzDirectory>();
            this.checksum = 0xc032f7a;
            this.name = name;
        }

        internal WzDirectory(BinaryReader reader, uint blockStart, string dirName, int verHash)
        {
            this.images = new List<WzImage>();
            this.subDirs = new List<WzDirectory>();
            this.checksum = 0xc032f7a;
            this.wzReader = reader;
            this.blockStartOffset = blockStart;
            this.name = dirName;
            this.hash = verHash;
        }

        public void AddDirectory(WzDirectory dir)
        {
            dir.Parent = this;
            this.subDirs.Add(dir);
        }

        public void AddImage(WzImage img)
        {
            img.Parent = this;
            this.images.Add(img);
        }

        public void ClearDirectories()
        {
            this.subDirs.Clear();
        }

        public void ClearImages()
        {
            this.images.Clear();
        }

        public void Dispose()
        {
            this.name = null;
            this.wzReader = null;
            foreach (WzImage image in this.images)
            {
                image.Dispose();
            }
            foreach (WzDirectory directory in this.subDirs)
            {
                directory.Dispose();
            }
            this.images.Clear();
            this.subDirs.Clear();
            this.images = null;
            this.subDirs = null;
        }

        internal uint EncryptOffset(uint offset, long pos)
        {
            uint x = (uint) pos;
            x = (x - 60) ^ uint.MaxValue;
            x = (uint) (x * this.hash);
            x -= 0x581c3f6d;
            return (this.RotateLeft(x, (byte) (x & 0x1f)) ^ (offset - 120));
        }

        internal int GenerateDataFile(string path)
        {
            this.size = 0;
            int i = this.subDirs.Count + this.images.Count;
            if (i == 0)
            {
                this.offsetSize = 1;
                return (this.size = 0);
            }
            this.size = WzTools.GetCompressedIntLength(i);
            this.offsetSize = WzTools.GetCompressedIntLength(i);
            for (int j = 0; j < this.subDirs.Count; j++)
            {
                Directory.CreateDirectory(Path.Combine(path, this.subDirs[j].name));
                this.size += ((((1 + WzTools.GetEncodedStringLength(this.subDirs[j].name)) + this.subDirs[j].GenerateDataFile(Path.Combine(path, this.subDirs[j].name))) + WzTools.GetCompressedIntLength(this.subDirs[j].size)) + WzTools.GetCompressedIntLength(this.subDirs[j].checksum)) + 4;
                this.offsetSize += (((1 + WzTools.GetEncodedStringLength(this.subDirs[j].name)) + WzTools.GetCompressedIntLength(this.subDirs[j].size)) + WzTools.GetCompressedIntLength(this.subDirs[j].checksum)) + 4;
            }
            for (int k = 0; k < this.images.Count; k++)
            {
                BinaryWriter wzWriter = new BinaryWriter(File.Create(Path.Combine(path, this.images[k].Name + ".TEMP")));
                this.images[k].SaveImage(wzWriter);
                wzWriter.Close();
                FileInfo info = new FileInfo(Path.Combine(path, this.images[k].Name + ".TEMP"));
                this.size += ((((1 + WzTools.GetEncodedStringLength(this.images[k].Name)) + WzTools.GetCompressedIntLength((int) info.Length)) + ((int) info.Length)) + WzTools.GetCompressedIntLength(this.images[k].Checksum)) + 4;
                this.offsetSize += (((1 + WzTools.GetEncodedStringLength(this.images[k].Name)) + WzTools.GetCompressedIntLength((int) info.Length)) + WzTools.GetCompressedIntLength(this.images[k].Checksum)) + 4;
            }
            return this.size;
        }

        public WzDirectory GetDirectoryByName(string name)
        {
            foreach (WzDirectory directory in this.subDirs)
            {
                if (directory.Name == name)
                {
                    return directory;
                }
            }
            return null;
        }

        public WzImage GetImageByName(string name)
        {
            foreach (WzImage image in this.images)
            {
                if (image.Name == name)
                {
                    return image;
                }
            }
            return null;
        }

        internal uint GetImgOffsets(uint curOffset)
        {
            foreach (WzImage image in this.images)
            {
                image.Offset = curOffset;
                curOffset += (uint) image.BlockSize;
            }
            foreach (WzDirectory directory in this.subDirs)
            {
                curOffset = directory.GetImgOffsets(curOffset);
            }
            return curOffset;
        }

        internal uint GetOffsets(uint curOffset)
        {
            this.offset = curOffset;
            curOffset += (uint) this.offsetSize;
            foreach (WzDirectory directory in this.subDirs)
            {
                curOffset = directory.GetOffsets(curOffset);
            }
            return curOffset;
        }

        internal void ParseDirectory()
        {
            int num = WzTools.ReadCompressedInt(this.wzReader);
            for (int i = 0; i < num; i++)
            {
                byte num3 = this.wzReader.ReadByte();
                if ((num3 >= 2) && (num3 <= 4))
                {
                    string str;
                    if ((num3 == 3) || (num3 == 4))
                    {
                        str = WzTools.ReadDecodedString(this.wzReader);
                    }
                    else
                    {
                        str = WzTools.ReadDecodedStringAtOffsetAndReset(this.wzReader, this.blockStartOffset + this.wzReader.ReadInt32(), true);
                    }
                    int num4 = WzTools.ReadCompressedInt(this.wzReader);
                    int num5 = WzTools.ReadCompressedInt(this.wzReader);
                    uint num6 = this.ReadOffset();
                    if (num3 == 3)
                    {
                        WzDirectory item = new WzDirectory(this.wzReader, this.blockStartOffset, str, this.hash) {
                            BlockSize = num4,
                            Checksum = num5,
                            Offset = num6,
                            Parent = this
                        };
                        this.subDirs.Add(item);
                    }
                    else
                    {
                        WzImage image = new WzImage(str, this.wzReader) {
                            BlockSize = num4,
                            Checksum = num5,
                            Offset = num6,
                            Parent = this
                        };
                        this.images.Add(image);
                    }
                }
            }
            foreach (WzDirectory directory2 in this.subDirs)
            {
                this.wzReader.BaseStream.Position = directory2.offset;
                directory2.ParseDirectory();
            }
        }

        internal void ParseImages()
        {
            foreach (WzImage image in this.images)
            {
                if (this.wzReader.BaseStream.Position != image.Offset)
                {
                    this.wzReader.BaseStream.Position = image.Offset;
                }
                image.ParseImage();
            }
            foreach (WzDirectory directory in this.subDirs)
            {
                if (this.wzReader.BaseStream.Position != directory.Offset)
                {
                    this.wzReader.BaseStream.Position = directory.Offset;
                }
                directory.ParseImages();
            }
        }

        internal uint ReadOffset()
        {
            uint position = (uint) this.wzReader.BaseStream.Position;
            position = (position - 60) ^ uint.MaxValue;
            position = (uint) (position * this.hash);
            position -= 0x581c3f6d;
            position = this.RotateLeft(position, (byte) (position & 0x1f));
            uint num2 = this.wzReader.ReadUInt32();
            position ^= num2;
            return (position + 120);
        }

        public void RemoveDirectory(string name)
        {
            for (int i = 0; i < this.subDirs.Count; i++)
            {
                if (this.subDirs[i].Name == name)
                {
                    this.subDirs.RemoveAt(i);
                }
            }
        }

        public void RemoveImage(string name)
        {
            for (int i = 0; i < this.images.Count; i++)
            {
                if (this.images[i].Name == name)
                {
                    this.images.RemoveAt(i);
                }
            }
        }

        internal uint RotateLeft(uint x, byte n)
        {
            return ((x << n) | (x >> (0x20 - n)));
        }

        internal uint RotateRight(uint x, byte n)
        {
            return ((x >> n) | (x << (0x20 - n)));
        }

        internal void SaveDirectory(BinaryWriter wzWriter)
        {
            this.offset = (uint) wzWriter.BaseStream.Position;
            int i = this.subDirs.Count + this.images.Count;
            if (i == 0)
            {
                this.BlockSize = 0;
            }
            else
            {
                WzTools.WriteCompressedInt(wzWriter, i);
                foreach (WzDirectory directory in this.subDirs)
                {
                    byte num2 = 3;
                    wzWriter.Write(num2);
                    WzTools.WriteEncodedString(wzWriter, directory.Name);
                    WzTools.WriteCompressedInt(wzWriter, directory.BlockSize);
                    WzTools.WriteCompressedInt(wzWriter, directory.Checksum);
                    wzWriter.Write(this.EncryptOffset(directory.Offset, wzWriter.BaseStream.Position));
                }
                foreach (WzImage image in this.images)
                {
                    byte num3 = 4;
                    wzWriter.Write(num3);
                    WzTools.WriteEncodedString(wzWriter, image.Name);
                    WzTools.WriteCompressedInt(wzWriter, image.BlockSize);
                    WzTools.WriteCompressedInt(wzWriter, image.Checksum);
                    wzWriter.Write(this.EncryptOffset(image.Offset, wzWriter.BaseStream.Position));
                }
                foreach (WzDirectory directory2 in this.subDirs)
                {
                    if (directory2.BlockSize > 0)
                    {
                        directory2.SaveDirectory(wzWriter);
                    }
                    else
                    {
                        wzWriter.Write((byte) 0);
                    }
                }
            }
        }

        internal void SaveImages(BinaryWriter wzWriter, string path)
        {
            foreach (WzImage image in this.images)
            {
                BinaryReader reader = null;
            tryagain:
                try
                {
                    reader = new BinaryReader(File.Open(Path.Combine(path, image.name + ".TEMP"), FileMode.Open));
                }
                catch
                {
                    goto tryagain;
                }
                byte[] data = new byte[reader.BaseStream.Length];
                reader.Read(data,0,data.Length);
                wzWriter.Write(data, 0, data.Length);
                reader.Close();
            }
            foreach (WzDirectory directory in this.subDirs)
            {
                directory.SaveImages(wzWriter, Path.Combine(path, directory.Name));
            }
        }

        internal void SetHash(int newHash)
        {
            this.hash = newHash;
            foreach (WzDirectory directory in this.subDirs)
            {
                directory.SetHash(newHash);
            }
        }

        public int BlockSize
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }

        public int Checksum
        {
            get
            {
                return this.checksum;
            }
            set
            {
                this.checksum = value;
            }
        }

        public IWzObject this[string name]
        {
            get
            {
                foreach (WzImage image in this.images)
                {
                    if (image.Name == name)
                    {
                        return image;
                    }
                }
                foreach (WzDirectory directory in this.subDirs)
                {
                    if (directory.Name == name)
                    {
                        return directory;
                    }
                }
                throw new KeyNotFoundException("No wz image or directory was found with the specified name");
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public WzObjectType ObjectType
        {
            get
            {
                return WzObjectType.Directory;
            }
        }

        public uint Offset
        {
            get
            {
                return this.offset;
            }
            set
            {
                this.offset = value;
            }
        }

        public IWzObject Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public WzDirectory[] WzDirectories
        {
            get
            {
                return this.subDirs.ToArray();
            }
        }

        public WzImage[] WzImages
        {
            get
            {
                return this.images.ToArray();
            }
        }
    }
}

