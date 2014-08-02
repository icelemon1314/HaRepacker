namespace WzLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class WzImage : IWzObject, IDisposable
    {
        internal int checksum;
        internal uint headerOffsetOffset;
        internal string name;
        internal uint offset;
        internal IWzObject parent;
        internal bool parsed;
        internal List<IWzImageProperty> properties;
        internal int size;
        internal BinaryReader wzReader;
        public bool changed = false;

        public WzImage Clone()
        {
            WzImage result = new WzImage(name, wzReader);
            result.checksum = checksum;
            result.headerOffsetOffset = headerOffsetOffset;
            result.offset = offset;
            result.parent = parent;
            result.parsed = parsed;
            result.properties = new List<IWzImageProperty>();
            foreach (IWzImageProperty wzprop in this.properties)
            {
                result.properties.Add(wzprop);
            }
            result.size = size;
            return result;
        }

        public WzImage()
        {
            this.checksum = 0xc32c58c;
            this.properties = new List<IWzImageProperty>();
        }

        public WzImage(string name)
        {
            this.checksum = 0xc32c58c;
            this.properties = new List<IWzImageProperty>();
            this.name = name;
        }

        internal WzImage(string name, BinaryReader wzread)
        {
            this.checksum = 0xc32c58c;
            this.properties = new List<IWzImageProperty>();
            this.name = name;
            this.wzReader = wzread;
        }

        public void AddProperty(IWzImageProperty prop)
        {
            if ((this.wzReader != null) && !this.parsed)
            {
                this.ParseImage();
            }
            prop.Parent = this;
            prop.ParentImage = this;
            switch (prop.PropertyType)
            {
                case WzPropertyType.SubProperty:
                case WzPropertyType.Canvas:
                case WzPropertyType.Vector:
                case WzPropertyType.Convex:
                case WzPropertyType.Sound:
                case WzPropertyType.UOL:
                this.properties.Add(new WzExtendedProperty(prop.Name) { ExtendedProperty = prop });
                    return;
            }
            this.properties.Add(prop);
        }

        public void Dispose()
        {
            this.name = null;
            this.wzReader = null;
            if (this.properties != null)
            {
                foreach (IWzImageProperty property in this.properties)
                {
                    property.Dispose();
                }
                this.properties.Clear();
                this.properties = null;
            }
        }

        internal string DumpBlock()
        {
            switch (this.wzReader.ReadByte())
            {
                case 0:
                    return WzTools.ReadDecodedString(this.wzReader);

                case 1:
                    return WzTools.ReadDecodedStringAtOffsetAndReset(this.wzReader, this.offset + this.wzReader.ReadInt32());
            }
            return "";
        }

        public void ParseImage()
        {
            long position = this.wzReader.BaseStream.Position;
            this.wzReader.BaseStream.Position = this.offset;
            if (((this.wzReader.ReadByte() == 0x73) && (WzTools.ReadDecodedString(this.wzReader) == "Property")) && (this.wzReader.ReadUInt16() == 0))
            {
                int num2 = WzTools.ReadCompressedInt(this.wzReader);
                for (int i = 0; i < num2; i++)
                {
                    byte num4;
                    string propName = this.DumpBlock();
                    switch (this.wzReader.ReadByte())
                    {
                        case 0:
                        {
                            this.properties.Add(new WzNullProperty(propName) { Parent = this, ParentImage = this });
                            continue;
                        }
                        case 1:
                        case 6:
                        case 7:
                        case 10:
                        {
                            continue;
                        }
                        case 2:
                        case 11:
                        {
                            this.properties.Add(new WzUnsignedShortProperty(propName, this.wzReader.ReadUInt16()) { Parent = this, ParentImage = this });
                            continue;
                        }
                        case 3:
                        {
                            this.properties.Add(new WzCompressedIntProperty(propName, WzTools.ReadCompressedInt(this.wzReader)) { Parent = this, ParentImage = this });
                            continue;
                        }
                        case 4:
                        {
                            num4 = this.wzReader.ReadByte();
                            if (num4 != 0x80)
                            {
                                break;
                            }
                            this.properties.Add(new WzByteFloatProperty(propName, this.wzReader.ReadSingle()) { Parent = this, ParentImage = this });
                            continue;
                        }
                        case 5:
                        {
                            this.properties.Add(new WzDoubleProperty(propName, this.wzReader.ReadDouble()) { Parent = this, ParentImage = this });
                            continue;
                        }
                        case 8:
                        {
                            this.properties.Add(new WzStringProperty(propName, this.DumpBlock()) { Parent = this });
                            continue;
                        }
                        case 9:
                        {
                            WzExtendedProperty property;
                            int eob = (int) (this.wzReader.ReadUInt32() + this.wzReader.BaseStream.Position);
                            property = new WzExtendedProperty((int)this.offset, eob, propName); /*{
                                Parent = property.ParentImage = this
                            };*/
                            property.Parent = this;
                            property.ParentImage = this;
                            property.ParseExtendedProperty(this.wzReader);
                            this.properties.Add(property);
                            if (this.wzReader.BaseStream.Position != eob)
                            {
                                this.wzReader.BaseStream.Position = eob;
                            }
                            continue;
                        }
                        default:
                        {
                            continue;
                        }
                    }
                    if (num4 == 0)
                    {
                        this.properties.Add(new WzByteFloatProperty(propName, 0f) { Parent = this, ParentImage = this });
                    }
                }
                this.parsed = true;
            }
        }

        public void RemoveProperty(string name)
        {
            if ((this.wzReader != null) && !this.parsed)
            {
                this.ParseImage();
            }
            for (int i = 0; i < this.properties.Count; i++)
            {
                if (this.properties[i].Name == name)
                {
                    this.properties.RemoveAt(i);
                }
            }
        }

        internal void SaveImage(BinaryWriter wzWriter)
        {
            if (this.changed == false)
            {
                this.wzReader.BaseStream.Position = this.offset;
                byte[] imgdata = new byte[this.size];
                this.wzReader.Read(imgdata, 0, this.size);
                wzWriter.Write(imgdata, 0, this.size);
                return;
            }
            if ((this.wzReader != null) && !this.parsed)
            {
                this.ParseImage();
            }
            wzWriter.Write((byte) 0x73);
            WzTools.WriteEncodedString(wzWriter, "Property");
            wzWriter.Write((ushort) 0);
            WzTools.WriteCompressedInt(wzWriter, this.properties.Count);
            for (int i = 0; i < this.properties.Count; i++)
            {
                if (this.properties[i] is WzExtendedProperty)
                    this.properties[i].Name = ((WzExtendedProperty)this.properties[i]).ExtendedProperty.Name;                
		        wzWriter.Write((byte) 0);
                WzTools.WriteEncodedString(wzWriter, this.properties[i].Name);
                switch (this.properties[i].PropertyType)
                {
                    case WzPropertyType.Null:
                    {
                        wzWriter.Write((byte) 0);
                        continue;
                    }
                    case WzPropertyType.UnsignedShort:
                    {
                        wzWriter.Write((byte) 2);
                        wzWriter.Write(((WzUnsignedShortProperty) this.properties[i]).Value);
                        continue;
                    }
                    case WzPropertyType.CompressedInt:
                    {
                        wzWriter.Write((byte) 3);
                        WzTools.WriteCompressedInt(wzWriter, ((WzCompressedIntProperty) this.properties[i]).Value);
                        continue;
                    }
                    case WzPropertyType.ByteFloat:
                    {
                        wzWriter.Write((byte) 4);
                        if (((WzByteFloatProperty) this.properties[i]).Value != 0f)
                        {
                            break;
                        }
                        wzWriter.Write((byte) 0);
                        continue;
                    }
                    case WzPropertyType.Double:
                    {
                        wzWriter.Write((byte) 5);
                        wzWriter.Write(((WzDoubleProperty) this.properties[i]).Value);
                        continue;
                    }
                    case WzPropertyType.String:
                    {
                        wzWriter.Write((byte) 8);
                        wzWriter.Write((byte) 0);
                        WzTools.WriteEncodedString(wzWriter, ((WzStringProperty) this.properties[i]).Value);
                        continue;
                    }
                    case WzPropertyType.Extended:
                    {
                        wzWriter.Write((byte) 9);
                        long position = wzWriter.BaseStream.Position;
                        wzWriter.Write(0);
                        ((WzExtendedProperty) this.properties[i]).SaveExtendedProperty(wzWriter);
                        int num3 = (int) (wzWriter.BaseStream.Position - position);
                        long num4 = wzWriter.BaseStream.Position;
                        wzWriter.BaseStream.Position = position;
                        wzWriter.Write((int) (num3 - 4));
                        wzWriter.BaseStream.Position = num4;
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
                wzWriter.Write((byte) 0x80);
                wzWriter.Write(((WzByteFloatProperty) this.properties[i]).Value);
            }
            this.size = (int) wzWriter.BaseStream.Position;
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

        public IWzImageProperty this[string name]
        {
            get
            {
                if ((this.wzReader != null) && !this.parsed)
                {
                    this.ParseImage();
                }
                foreach (IWzImageProperty property in this.properties)
                {
                    if (property.Name == name)
                    {
                        if (property.PropertyType == WzPropertyType.Extended)
                        {
                            return ((WzExtendedProperty) property).ExtendedProperty;
                        }
                        return property;
                    }
                }
                throw new KeyNotFoundException("A wz property with the specified name was not found");
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
                if ((this.wzReader != null) && !this.parsed)
                {
                    this.ParseImage();
                }
                return WzObjectType.Image;
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

        public IWzImageProperty[] WzProperties
        {
            get
            {
                if ((this.wzReader != null) && !this.parsed)
                {
                    this.ParseImage();
                }
                return this.properties.ToArray();
            }
        }

        public bool Parsed
        {
            get
            {
                return this.parsed;
            }
        }
    }
}

