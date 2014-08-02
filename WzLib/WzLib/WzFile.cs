namespace WzLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class WzFile : IWzFile, IWzObject, IDisposable
    {
        internal string copyright;
        internal byte fileVersion;
        internal ulong fsize;
        internal uint fstart;
        internal string ident;
        internal string name;
        internal string path;
        internal WzMapleVersion type;
        internal short version;
        internal int versionHash;
        internal WzLib.WzDirectory wzDir;

        public WzFile(WzMapleVersion version)
        {
            this.ident = "";
            this.copyright = "";
            this.name = "";
            this.wzDir = new WzLib.WzDirectory();
            this.ident = "PKG1";
            this.copyright = "Package file v1.0 Copyright 2002 Wizet, ZMS";
            this.type = version;
        }

        public WzFile(string filePath, WzMapleVersion version)
        {
            this.ident = "";
            this.copyright = "";
            this.name = "";
            this.name = System.IO.Path.GetFileName(filePath);
            this.path = filePath;
            //this.fileVersion = gameVersion;
            this.type = version;
        }

        
        internal void CreateVersionHash()
        {
            this.versionHash = 0;
            foreach (char ch in this.fileVersion.ToString())
            {
                this.versionHash = ((this.versionHash * 0x20) + ((byte) ch)) + 1;
            }
            int num = (this.versionHash >> 0x18) & 0xff;
            int num2 = (this.versionHash >> 0x10) & 0xff;
            int num3 = (this.versionHash >> 8) & 0xff;
            int num4 = this.versionHash & 0xff;
            this.version = (byte) ~(((num ^ num2) ^ num3) ^ num4);
        }

        public void Dispose()
        {
            try
            {
                this.wzDir.wzReader.Close();
                this.path = null;
                this.ident = null;
                this.copyright = null;
                this.name = null;
                this.WzDirectory.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {
            }
        }

        public IWzObject GetObjectFromPath(string path)
        {
            string[] strArray = path.Split("/".ToCharArray());
            if (strArray[0] != this.name)
            {
                return null;
            }
            if (strArray.Length == 1)
            {
                return this.WzDirectory;
            }
            IWzObject wzDirectory = this.WzDirectory;
            for (int i = 1; i < strArray.Length; i++)
            {
                switch (wzDirectory.ObjectType)
                {
                    case WzObjectType.Image:
                    {
                        wzDirectory = ((WzImage) wzDirectory)[strArray[i]];
                        continue;
                    }
                    case WzObjectType.Directory:
                    {
                        wzDirectory = ((WzLib.WzDirectory) wzDirectory)[strArray[i]];
                        continue;
                    }
                    case WzObjectType.Property:
                        switch (((IWzImageProperty) wzDirectory).PropertyType)
                        {
                            case WzPropertyType.Extended:
                                goto Label_00D4;

                            case WzPropertyType.SubProperty:
                                goto Label_00E6;

                            case WzPropertyType.Vector:
                                goto Label_00F7;

                            case WzPropertyType.Convex:
                                goto Label_00C3;
                        }
                        return null;

                    default:
                    {
                        continue;
                    }
                }
                wzDirectory = ((WzCanvasProperty) wzDirectory)[strArray[i]];
                continue;
            Label_00C3:
                wzDirectory = ((WzConvexProperty) wzDirectory)[strArray[i]];
                continue;
            Label_00D4:
                wzDirectory = ((WzExtendedProperty) wzDirectory).ExtendedProperty;
                i--;
                continue;
            Label_00E6:
                wzDirectory = ((WzSubProperty) wzDirectory)[strArray[i]];
                continue;
            Label_00F7:
                if (strArray[i] == "X")
                {
                    return ((WzVectorProperty) wzDirectory).X;
                }
                if (strArray[i] == "Y")
                {
                    return ((WzVectorProperty) wzDirectory).Y;
                }
                return null;
            }
            if ((wzDirectory.ObjectType == WzObjectType.Property) && (((IWzImageProperty) wzDirectory).PropertyType == WzPropertyType.Extended))
            {
                wzDirectory = ((WzExtendedProperty) wzDirectory).ExtendedProperty;
            }
            return wzDirectory;
        }

        public IWzObject[] GetObjectsFromDirectory(WzLib.WzDirectory dir)
        {
            List<IWzObject> list = new List<IWzObject>();
            foreach (WzImage image in dir.WzImages)
            {
                list.Add(image);
                list.AddRange(this.GetObjectsFromImage(image));
            }
            foreach (WzLib.WzDirectory directory in dir.WzDirectories)
            {
                list.Add(directory);
                list.AddRange(this.GetObjectsFromDirectory(directory));
            }
            return list.ToArray();
        }

        public IWzObject[] GetObjectsFromImage(WzImage img)
        {
            List<IWzObject> list = new List<IWzObject>();
            foreach (IWzImageProperty property in img.WzProperties)
            {
                list.Add(property);
                list.AddRange(this.GetObjectsFromProperty(property));
            }
            return list.ToArray();
        }

        public IWzObject[] GetObjectsFromProperty(IWzImageProperty prop)
        {
            List<IWzObject> list = new List<IWzObject>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Extended:
                    list.AddRange(this.GetObjectsFromProperty(((WzExtendedProperty) prop).ExtendedProperty));
                    break;

                case WzPropertyType.SubProperty:
                    foreach (IWzImageProperty property3 in ((WzSubProperty) prop).WzProperties)
                    {
                        list.AddRange(this.GetObjectsFromProperty(property3));
                    }
                    break;

                case WzPropertyType.Canvas:
                    foreach (IWzImageProperty property in ((WzCanvasProperty) prop).WzProperties)
                    {
                        list.AddRange(this.GetObjectsFromProperty(property));
                    }
                    list.Add(((WzCanvasProperty) prop).PngProperty);
                    break;

                case WzPropertyType.Vector:
                    list.Add(((WzVectorProperty) prop).X);
                    list.Add(((WzVectorProperty) prop).Y);
                    break;

                case WzPropertyType.Convex:
                    foreach (WzExtendedProperty property2 in ((WzConvexProperty) prop).WzProperties)
                    {
                        list.AddRange(this.GetObjectsFromProperty(property2));
                    }
                    break;
            }
            return list.ToArray();
        }

        public IWzObject[] GetObjectsFromRegexPath(string path)
        {
            if (path == this.name)
            {
                return new IWzObject[] { this.WzDirectory };
            }
            List<IWzObject> list = new List<IWzObject>();
            foreach (WzImage image in this.WzDirectory.WzImages)
            {
                foreach (string str in this.GetPathsFromImage(image, this.name + "/" + image.Name))
                {
                    if (Regex.Match(str, path).Success)
                    {
                        list.Add(this.GetObjectFromPath(str));
                    }
                }
            }
            foreach (WzLib.WzDirectory directory in this.wzDir.WzDirectories)
            {
                foreach (string str2 in this.GetPathsFromDirectory(directory, this.name + "/" + directory.Name))
                {
                    if (Regex.Match(str2, path).Success)
                    {
                        list.Add(this.GetObjectFromPath(str2));
                    }
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return list.ToArray();
        }

        public IWzObject[] GetObjectsFromWildcardPath(string path)
        {
            if (path == this.name)
            {
                return new IWzObject[] { this.WzDirectory };
            }
            if (path == "*")
            {
                List<IWzObject> list = new List<IWzObject> {
                    this.WzDirectory
                };
                list.AddRange(this.GetObjectsFromDirectory(this.WzDirectory));
                return list.ToArray();
            }
            if (!path.Contains("*"))
            {
                return new IWzObject[] { this.GetObjectFromPath(path) };
            }
            string[] strArray = path.Split("/".ToCharArray());
            if ((strArray.Length == 2) && (strArray[1] == "*"))
            {
                return this.GetObjectsFromDirectory(this.WzDirectory);
            }
            List<IWzObject> list2 = new List<IWzObject>();
            foreach (WzImage image in this.WzDirectory.WzImages)
            {
                foreach (string str in this.GetPathsFromImage(image, this.name + "/" + image.Name))
                {
                    if (this.strMatch(path, str))
                    {
                        list2.Add(this.GetObjectFromPath(str));
                    }
                }
            }
            foreach (WzLib.WzDirectory directory in this.wzDir.WzDirectories)
            {
                foreach (string str2 in this.GetPathsFromDirectory(directory, this.name + "/" + directory.Name))
                {
                    if (this.strMatch(path, str2))
                    {
                        list2.Add(this.GetObjectFromPath(str2));
                    }
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return list2.ToArray();
        }

        internal string[] GetPathsFromDirectory(WzLib.WzDirectory dir, string curPath)
        {
            List<string> list = new List<string>();
            foreach (WzImage image in dir.WzImages)
            {
                list.Add(curPath + "/" + image.Name);
                list.AddRange(this.GetPathsFromImage(image, curPath + "/" + image.Name));
            }
            foreach (WzLib.WzDirectory directory in dir.WzDirectories)
            {
                list.Add(curPath + "/" + directory.Name);
                list.AddRange(this.GetPathsFromDirectory(directory, curPath + "/" + directory.Name));
            }
            return list.ToArray();
        }

        internal string[] GetPathsFromImage(WzImage img, string curPath)
        {
            List<string> list = new List<string>();
            foreach (IWzImageProperty property in img.WzProperties)
            {
                list.Add(curPath + "/" + property.Name);
                list.AddRange(this.GetPathsFromProperty(property, curPath + "/" + property.Name));
            }
            return list.ToArray();
        }

        internal string[] GetPathsFromProperty(IWzImageProperty prop, string curPath)
        {
            List<string> list = new List<string>();
            switch (prop.PropertyType)
            {
                case WzPropertyType.Extended:
                    list.AddRange(this.GetPathsFromProperty(((WzExtendedProperty) prop).ExtendedProperty, curPath));
                    break;

                case WzPropertyType.SubProperty:
                    foreach (IWzImageProperty property3 in ((WzSubProperty) prop).WzProperties)
                    {
                        list.Add(curPath + "/" + property3.Name);
                        list.AddRange(this.GetPathsFromProperty(property3, curPath + "/" + property3.Name));
                    }
                    break;

                case WzPropertyType.Canvas:
                    foreach (IWzImageProperty property in ((WzCanvasProperty) prop).WzProperties)
                    {
                        list.Add(curPath + "/" + property.Name);
                        list.AddRange(this.GetPathsFromProperty(property, curPath + "/" + property.Name));
                    }
                    list.Add(curPath + "/PNG");
                    break;

                case WzPropertyType.Vector:
                    list.Add(curPath + "/X");
                    list.Add(curPath + "/Y");
                    break;

                case WzPropertyType.Convex:
                    foreach (WzExtendedProperty property2 in ((WzConvexProperty) prop).WzProperties)
                    {
                        list.Add(curPath + "/" + property2.Name);
                        list.AddRange(this.GetPathsFromProperty(property2, curPath + "/" + property2.Name));
                    }
                    break;
            }
            return list.ToArray();
        }

        // 获取版本的哈希列表 76 
        internal bool GetVersionHash()
        {
            this.versionHash = 0;
            foreach (char ch in this.fileVersion.ToString())
            {
                this.versionHash = ((this.versionHash * 0x20) + ((byte) ch)) + 1;
            }
            int num = (this.versionHash >> 0x18) & 0xff;
            int num2 = (this.versionHash >> 0x10) & 0xff;
            int num3 = (this.versionHash >> 8) & 0xff;
            int num4 = this.versionHash & 0xff;
            byte num5 = (byte) ~(((num ^ num2) ^ num3) ^ num4);
            return (num5 == this.version);
        }
        // 解析WZ主目录
        internal void ParseMainWzDirectory()
        {
            WzLib.WzDirectory directory;
            if (this.path == null)
            {
                return;
            }
            BinaryReader fileReader = new BinaryReader(File.Open(this.path, FileMode.Open));
            this.ident = "";
            // 前四个字节，文件标识：PKG1
            for (int i = 0; i < 4; i++)
            {
                this.ident = this.ident + ((char)fileReader.ReadByte());
            }
            this.fsize = fileReader.ReadUInt64();   // 8个字节，文件大小
            this.fstart = fileReader.ReadUInt32();  // 4个字节，开始位置
            this.copyright = WzTools.ReadNullTerminatedString(fileReader);  // 字符串，版本信息：Package file v1.0 Copyright 2002 Wizet, ZMS
            this.version = fileReader.ReadInt16();  // 2个字节，文件版本
            // 这个循环用于穷举versionHash值
            for (int j = 0; j < 0xff; j++)
            {
                this.fileVersion = (byte)j;
                if (this.GetVersionHash())
                {
                    long pos = fileReader.BaseStream.Position;
                    WzDirectory tdir = null;
                    try
                    {
                        tdir = new WzDirectory(fileReader, fstart, name, versionHash);
                        tdir.ParseDirectory();
                    }
                    catch
                    {
                        fileReader.BaseStream.Position = pos;
                        continue;
                    }
                    WzImage test = null;
                    if (tdir.WzImages.Length != 0)
                    {
                        test = tdir.WzImages[0];
                    }
                    else
                    {
                        test = GetTestImage(tdir.WzDirectories);
                    }
                    try
                    {
                        fileReader.BaseStream.Position = test.Offset;
                        byte check = fileReader.ReadByte();
                        fileReader.BaseStream.Position = pos;
                        tdir.Dispose();
                        if (check == 0x73 || check == 0x1B)
                            goto Label_00B1;
                        else
                            fileReader.BaseStream.Position = pos;
                    }
                    catch
                    {
                        fileReader.BaseStream.Position = pos;
                        continue;
                    }
                }
            }
            throw new Exception("Error with game version hash : The specified game version is incorrect and WzLib was unable to determine the version itself");
        Label_00B1:
            directory = new WzLib.WzDirectory(fileReader, this.fstart, this.name, this.versionHash);
            directory.ParseDirectory();
            this.wzDir = directory;
        }

        private WzImage GetTestImage(WzDirectory[] dir)
        {
            foreach (WzDirectory wzdir in dir)
            {
                if (wzdir.WzImages.Length != 0)
                {
                    return wzdir.WzImages[0];
                }
                else if (wzdir.WzDirectories.Length != 0)
                {
                    return GetTestImage(wzdir.WzDirectories);
                }
                else
                {
                    throw new Exception("Error in GetTestImage, could not find an img to brute force encryption on");
                }
            }
            throw new Exception("Error in GetTestImage, could not find an img to brute force encryption on");
        }
        // 解析WZ文件
        public void ParseWzFile()
        {
            WzTools.CreateWzKey(this.type);
            this.ParseMainWzDirectory();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void SaveToDisk(string path)
        {
            string temp = System.IO.Path.GetTempPath();
            if (Directory.Exists(temp + @"HaRepacker$temp.build"))
            {
                WzTools.DelDir(temp + @"HaRepacker$temp.build");
            }
            WzTools.CreateWzKey(this.type);
            this.CreateVersionHash();
            this.wzDir.SetHash(this.versionHash);
            Directory.CreateDirectory(temp + @"HaRepacker$temp.build");
            this.wzDir.GenerateDataFile(temp + @"HaRepacker$temp.build");
            uint imgOffsets = this.wzDir.GetImgOffsets(this.wzDir.GetOffsets(0x3e));
            BinaryWriter wzWriter = new BinaryWriter(File.Create(path));
            for (int i = 0; i < 4; i++)
            {
                wzWriter.Write((byte) this.ident[i]);
            }
            wzWriter.Write((long) (imgOffsets - 60));
            wzWriter.Write(60);
            WzTools.WriteNullTerminatedString(wzWriter, "Package file v1.0 Copyright 2002 Wizet, ZMS");
            wzWriter.Write(this.version);
            this.wzDir.SaveDirectory(wzWriter);
            this.wzDir.SaveImages(wzWriter, temp + @"HaRepacker$temp.build");
            wzWriter.Close();
            WzTools.DelDir(temp + @"\HaRepacker$temp.build");
        }

        internal bool strMatch(string strWildCard, string strCompare)
        {
            if (strWildCard.Length == 0)
            {
                return (strCompare.Length == 0);
            }
            if (strCompare.Length != 0)
            {
                if ((strWildCard[0] == '*') && (strWildCard.Length > 1))
                {
                    for (int i = 0; i < strCompare.Length; i++)
                    {
                        if (this.strMatch(strWildCard.Substring(1), strCompare.Substring(i)))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (strWildCard[0] == '*')
                    {
                        return true;
                    }
                    if (strWildCard[0] == strCompare[0])
                    {
                        return this.strMatch(strWildCard.Substring(1), strCompare.Substring(1));
                    }
                }
            }
            return false;
        }

        public string Copyright
        {
            get
            {
                return this.copyright;
            }
        }

        public ulong FileSize
        {
            get
            {
                return this.fsize;
            }
        }

        public uint FileStart
        {
            get
            {
                return this.fstart;
            }
        }

        public string Identification
        {
            get
            {
                return this.ident;
            }
        }

        public IWzObject this[string name]
        {
            get
            {
                return this.WzDirectory[name];
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

        public WzLib.WzDirectory WzDirectory
        {
            get
            {
                return this.wzDir;
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }
    }
}

