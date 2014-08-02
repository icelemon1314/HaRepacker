namespace WzLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class WzListFile : IWzFile, IWzObject, IDisposable
    {
        internal List<string> listEntries;
        internal string name;
        internal byte[] wzFileBytes;
        public int crypting = 0;

        public WzListFile(byte[] fileBytes)
        {
            this.listEntries = new List<string>();
            this.name = "";
            this.wzFileBytes = fileBytes;
        }

        // 将指定文件读入wzFileBytes中
        public WzListFile(string filePath)
        {
            this.listEntries = new List<string>();
            this.name = "";
            this.name = Path.GetFileName(filePath);
            FileStream stream = File.Open(filePath, FileMode.Open);
            this.wzFileBytes = new byte[stream.Length];
            stream.Read(this.wzFileBytes, 0, (int) stream.Length);
            stream.Close();
        }

        public void Dispose()
        {
            this.wzFileBytes = null;
            this.name = null;
            this.listEntries.Clear();
            this.listEntries = null;
        }

        // 解析WZ文件
        public void ParseWzFile()
        {
            WzTools.CreateWzKey(WzMapleVersion.GMS);
            // 将内存中的数据初始化为一个二进制流准备读取
            BinaryReader reader = new BinaryReader(new MemoryStream(this.wzFileBytes));
            while (reader.PeekChar() != -1)
            {
                int num = reader.ReadInt32(); // 读取4个字节 05 00 00 00
                char[] stringToDecrypt = new char[num];
                for (int i = 0; i < num; i++)
                {
                    stringToDecrypt[i] = (char) ((ushort) reader.ReadInt16()); // 读取2个字节 05 00
                }
                reader.ReadUInt16();
                string item = WzTools.DecryptString(stringToDecrypt);
                if ((reader.PeekChar() == -1) && (item[item.Length - 1] == '/'))
                {
                    item = item.TrimEnd("/".ToCharArray()) + "g";
                }
                this.listEntries.Add(item);
            }
            reader.Close();
        }

        internal void SaveToDisk(string path)
        {
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
                return WzObjectType.File;
            }
        }

        public IWzObject Parent
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string[] WzListEntries
        {
            get
            {
                return this.listEntries.ToArray();
            }
        }
    }
}

