namespace WzLib
{
    using System;
    using System.IO;

    public class WzExtendedProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal int endOfBlock;
        internal IWzImageProperty extendedProperty;
        internal WzImage imgParent;
        internal string name;
        internal int offset;
        internal IWzObject parent;
        internal BinaryReader wzReader;

        public WzExtendedProperty()
        {
        }

        public WzExtendedProperty(string name)
        {
            this.name = name;
        }

        internal WzExtendedProperty(int offset, string name)
        {
            this.name = name;
            this.offset = offset;
        }

        internal WzExtendedProperty(int offset, int eob, string name)
        {
            this.endOfBlock = eob;
            this.name = name;
            this.offset = offset;
        }

        public void Dispose()
        {
            this.name = null;
            this.extendedProperty.Dispose();
            this.wzReader = null;
        }

        internal string DumpBlock()
        {
            byte blockType=this.wzReader.ReadByte();
            string decodeStr = "";
            switch (blockType)
            {
                case 0:
                    decodeStr = WzTools.ReadDecodedString(this.wzReader);
                    break;
                case 1:
                    decodeStr = WzTools.ReadDecodedStringAtOffsetAndReset(this.wzReader, (long)(this.offset + this.wzReader.ReadInt32()));
                    break;
                default:
                    this.WriteFile("DumpBlock"+blockType);
                    break;
            }
            return decodeStr;
        }

        public void WriteFile(string text)
        {
            FileStream fs = new FileStream("C:\\wzlib.txt",FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(text+"\n");
            sw.Close();
            fs.Close();
        }

        internal void DumpBlock(int endOfBlock, string name)
        {
            byte extractType = this.wzReader.ReadByte();
            switch (extractType)
            {
                case 0x1b:
                    this.ExtractMore(endOfBlock, name, WzTools.ReadDecodedStringAtOffsetAndReset(this.wzReader, (long) (this.offset + this.wzReader.ReadInt32())));
                    break;
                case 0x73:
                    this.ExtractMore(endOfBlock, name, "");
                    break;
                default:
                    this.WriteFile("未知的extractType：" + extractType);
                    break;

            }
        }

        internal void ExtractMore(int eob, string name, string iname)
        {
            if (iname == "")
            {
                iname = WzTools.ReadDecodedString(this.wzReader);
            }
            string str3 = iname;
            if (str3 != null)
            {
                if (str3 != "Property")
                {
                    if (str3 != "Canvas")
                    {
                        WzVectorProperty property19;
                        if (str3 != "Shape2D#Vector2D")
                        {
                            if (str3 != "Shape2D#Convex2D")
                            {
                                if (str3 != "Sound_DX8")
                                {
                                    if (str3 == "UOL")
                                    {
                                        Stream stream4 = this.wzReader.BaseStream;
                                        stream4.Position += 1L;
                                        switch (this.wzReader.ReadByte())
                                        {
                                            case 0:
                                                this.extendedProperty = new WzUOLProperty(name, WzTools.ReadDecodedString(this.wzReader)) { Parent = this.parent, ParentImage = this.imgParent };
                                                return;

                                            case 1:
                                                this.extendedProperty = new WzUOLProperty(name, WzTools.ReadDecodedStringAtOffsetAndReset(this.wzReader, (long) (this.offset + this.wzReader.ReadInt32()))) { Parent = this.parent, ParentImage = this.imgParent };
                                                return;
                                        }
                                    } else {
                                        this.WriteFile("未知的extendedProperty：" + str3);
                                    }

                                    return;
                                }
                                WzSoundProperty property23 = new WzSoundProperty(name) {
                                    Parent = this.parent,
                                    ParentImage = this.imgParent
                                };
                                property23.ParseSound(this.wzReader);
                                this.extendedProperty = property23;
                                return;
                            }
                            WzConvexProperty property20 = new WzConvexProperty(name) {
                                Parent = this.parent,
                                ParentImage = this.imgParent
                            };
                            int num9 = WzTools.ReadCompressedInt(this.wzReader);
                            for (int i = 0; i < num9; i++)
                            {
                                WzExtendedProperty prop = new WzExtendedProperty(this.offset, name) {
                                    Parent = property20,
                                    ParentImage = this.imgParent
                                };
                                prop.ParseExtendedProperty(this.wzReader);
                                property20.AddProperty(prop);
                            }
                            this.extendedProperty = property20;
                            return;
                        }
                        property19 = new WzVectorProperty(name) {
                            Parent = this.parent,
                            ParentImage = this.imgParent,
                            X = new WzCompressedIntProperty("X", WzTools.ReadCompressedInt(this.wzReader)) /*{ Parent = property19, ParentImage = this.imgParent }*/,
                            Y = new WzCompressedIntProperty("Y", WzTools.ReadCompressedInt(this.wzReader)) /*{ Parent = property19, ParentImage = this.imgParent }*/
                        };
                        this.extendedProperty = property19;
                        return;
                    }
                }
                else
                {
                    WzSubProperty property = new WzSubProperty(name) {
                        Parent = this.parent,
                        ParentImage = this.imgParent
                    };
                    Stream stream1 = this.wzReader.BaseStream;
                    stream1.Position += 2L;
                    int num = WzTools.ReadCompressedInt(this.wzReader);
                    for (int j = 0; j < num; j++)
                    {
                        byte num3;
                        string propName = this.DumpBlock();
                        byte propValue = this.wzReader.ReadByte();
                        switch (propValue)
                        {
                            case 0:
                            {
                                property.AddProperty(new WzNullProperty(propName) { Parent = property, ParentImage = this.imgParent });
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
                                property.AddProperty(new WzUnsignedShortProperty(propName, this.wzReader.ReadUInt16()) { Parent = property, ParentImage = this.imgParent });
                                continue;
                            }
                            case 3:
                            case 19: // Map.wz 992001100 add
                            case 20: // Character.wz  add
                            {
                                property.AddProperty(new WzCompressedIntProperty(propName, WzTools.ReadCompressedInt(this.wzReader)) { Parent = property, ParentImage = this.imgParent });
                                continue;
                            }
                            case 4: // 4字节浮点数
                            {
                                num3 = this.wzReader.ReadByte();
                                if (num3 != 0x80)
                                {
                                    break;
                                }
                                property.AddProperty(new WzByteFloatProperty(propName, this.wzReader.ReadSingle()) { Parent = property, ParentImage = this.imgParent });
                                continue;
                            }
                            case 5:
                            {
                                property.AddProperty(new WzDoubleProperty(propName, this.wzReader.ReadDouble()) { Parent = property, ParentImage = this.imgParent });
                                continue;
                            }
                            case 8: // string
                            {
                                property.AddProperty(new WzStringProperty(propName, this.DumpBlock()) { Parent = property, ParentImage = this.imgParent });
                                continue;
                            }
                            case 9:
                            {
                                int num4 = (int) (this.wzReader.ReadUInt32() + this.wzReader.BaseStream.Position);
                                WzExtendedProperty property2 = new WzExtendedProperty(this.offset, num4, propName) {
                                    Parent = property,
                                    ParentImage = this.imgParent
                                };
                                property2.ParseExtendedProperty(this.wzReader);
                                property.AddProperty(property2);
                                if (this.wzReader.BaseStream.Position != num4)
                                {
                                    this.wzReader.BaseStream.Position = num4;
                                }
                                continue;
                            }
                            default:
                            {
                                this.WriteFile("未知的PropertyValue：" + propValue);
                                continue;
                            }
                        }
                        if (num3 == 0)
                        {
                            property.AddProperty(new WzByteFloatProperty(propName, 0f) { Parent = property, ParentImage = this.imgParent });
                        }
                    }
                    this.extendedProperty = property;
                    return;
                }
                WzCanvasProperty property10 = new WzCanvasProperty(name) {
                    Parent = this.parent,
                    ParentImage = this.imgParent
                };
                Stream baseStream = this.wzReader.BaseStream;
                baseStream.Position += 1L;
                if (this.wzReader.ReadByte() == 1)
                {
                    Stream stream3 = this.wzReader.BaseStream;
                    stream3.Position += 2L;
                    int num5 = WzTools.ReadCompressedInt(this.wzReader);
                    for (int k = 0; k < num5; k++)
                    {
                        byte num7;
                        string str2 = this.DumpBlock();
                        switch (this.wzReader.ReadByte())
                        {
                            case 0:
                                property10.AddProperty(new WzNullProperty(str2) { Parent = property10, ParentImage = this.imgParent });
                                goto Label_061A;

                            case 2:
                            case 11:
                                property10.AddProperty(new WzUnsignedShortProperty(str2, this.wzReader.ReadUInt16()) { Parent = property10, ParentImage = this.imgParent });
                                goto Label_061A;

                            case 3:
                                property10.AddProperty(new WzCompressedIntProperty(str2, WzTools.ReadCompressedInt(this.wzReader)) { Parent = property10, ParentImage = this.imgParent });
                                goto Label_061A;

                            case 4:
                                num7 = this.wzReader.ReadByte();
                                if (num7 != 0x80)
                                {
                                    break;
                                }
                                property10.AddProperty(new WzByteFloatProperty(str2, this.wzReader.ReadSingle()) { Parent = property10, ParentImage = this.imgParent });
                                goto Label_061A;

                            case 5:
                                property10.AddProperty(new WzDoubleProperty(str2, this.wzReader.ReadDouble()) { Parent = property10, ParentImage = this.imgParent });
                                goto Label_061A;

                            case 8:
                                property10.AddProperty(new WzStringProperty(str2, this.DumpBlock()) { Parent = property10, ParentImage = this.imgParent });
                                goto Label_061A;

                            case 9:
                            {
                                int num8 = (int) (this.wzReader.ReadUInt32() + this.wzReader.BaseStream.Position);
                                WzExtendedProperty property11 = new WzExtendedProperty(this.offset, num8, str2) {
                                    Parent = property10,
                                    ParentImage = this.imgParent
                                };
                                property11.ParseExtendedProperty(this.wzReader);
                                property10.AddProperty(property11);
                                if (this.wzReader.BaseStream.Position != num8)
                                {
                                    this.wzReader.BaseStream.Position = num8;
                                }
                                goto Label_061A;
                            }
                            default:
                                goto Label_061A;
                        }
                        if (num7 == 0)
                        {
                            property10.AddProperty(new WzByteFloatProperty(str2, 0f) { Parent = property10, ParentImage = this.imgParent });
                        }
                    Label_061A:;
                    }
                }
                property10.PngProperty = new WzPngProperty(this.wzReader) { Parent = property10, ParentImage = this.imgParent };
                this.extendedProperty = property10;
            }
        }

        internal int GetExtendedPropertyLength()
        {
            int num2;
            int num3;
            int num = 0;
            switch (this.extendedProperty.PropertyType)
            {
                case WzPropertyType.SubProperty:
                    num += 3;
                    num += WzTools.GetEncodedStringLength("Property");
                    num += WzTools.GetCompressedIntLength(((WzSubProperty) this.extendedProperty).WzProperties.Length);
                    num2 = 0;
                    goto Label_0195;

                case WzPropertyType.Canvas:
                    num++;
                    num += WzTools.GetEncodedStringLength("Canvas");
                    num++;
                    if (((WzCanvasProperty) this.extendedProperty).WzProperties.Length <= 0)
                    {
                        num++;
                        goto Label_0350;
                    }
                    num += 3;
                    num += WzTools.GetCompressedIntLength(((WzCanvasProperty) this.extendedProperty).WzProperties.Length);
                    num3 = 0;
                    goto Label_0332;

                case WzPropertyType.Vector:
                    num++;
                    num += WzTools.GetEncodedStringLength("Shape2D#Vector2D");
                    num += WzTools.GetCompressedIntLength(((WzVectorProperty) this.extendedProperty).X.Value);
                    return (num + WzTools.GetCompressedIntLength(((WzVectorProperty) this.extendedProperty).Y.Value));

                case WzPropertyType.Convex:
                    num++;
                    num += WzTools.GetEncodedStringLength("Shape2D#Convex2D");
                    num += WzTools.GetCompressedIntLength(((WzConvexProperty) this.extendedProperty).WzProperties.Length);
                    for (int i = 0; i < ((WzConvexProperty) this.extendedProperty).WzProperties.Length; i++)
                    {
                        num += ((WzConvexProperty) this.extendedProperty).WzProperties[i].GetExtendedPropertyLength();
                    }
                    return num;

                case WzPropertyType.Sound:
                    num++;
                    num += WzTools.GetEncodedStringLength("Sound_DX8");
                    num++;
                    num += WzTools.GetCompressedIntLength(((WzSoundProperty) this.extendedProperty).SoundData.Length);
                    num++;
                    return (num + ((WzSoundProperty) this.extendedProperty).SoundData.Length);

                case WzPropertyType.UOL:
                    num++;
                    num += WzTools.GetEncodedStringLength("UOL");
                    num += 2;
                    return (num + WzTools.GetEncodedStringLength(((WzUOLProperty) this.extendedProperty).Value));

                default:
                    return num;
            }
        Label_0191:
            num2++;
        Label_0195:
            if (num2 < ((WzSubProperty) this.extendedProperty).WzProperties.Length)
            {
                num++;
                num += WzTools.GetEncodedStringLength(((WzSubProperty) this.extendedProperty).WzProperties[num2].Name);
                switch (((WzSubProperty) this.extendedProperty).WzProperties[num2].PropertyType)
                {
                    case WzPropertyType.Null:
                        num++;
                        goto Label_0191;

                    case WzPropertyType.UnsignedShort:
                        num += 3;
                        goto Label_0191;

                    case WzPropertyType.CompressedInt:
                        num++;
                        num += WzTools.GetCompressedIntLength(((WzCompressedIntProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num2]).Value);
                        goto Label_0191;

                    case WzPropertyType.ByteFloat:
                        num++;
                        if (((WzByteFloatProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num2]).Value != 0f)
                        {
                            num += 5;
                        }
                        else
                        {
                            num++;
                        }
                        goto Label_0191;

                    case WzPropertyType.Double:
                        num++;
                        num += 8;
                        goto Label_0191;

                    case WzPropertyType.String:
                        num += 2;
                        num += WzTools.GetEncodedStringLength(((WzStringProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num2]).Value);
                        goto Label_0191;

                    case WzPropertyType.Extended:
                        num += 5;
                        num += ((WzExtendedProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num2]).GetExtendedPropertyLength();
                        goto Label_0191;
                }
                goto Label_0191;
            }
            return num;
        Label_032E:
            num3++;
        Label_0332:
            if (num3 < ((WzCanvasProperty) this.extendedProperty).WzProperties.Length)
            {
                num++;
                num += WzTools.GetEncodedStringLength(((WzCanvasProperty) this.extendedProperty).WzProperties[num3].Name);
                switch (((WzCanvasProperty) this.extendedProperty).WzProperties[num3].PropertyType)
                {
                    case WzPropertyType.Null:
                        num++;
                        goto Label_032E;

                    case WzPropertyType.UnsignedShort:
                        num += 3;
                        goto Label_032E;

                    case WzPropertyType.CompressedInt:
                        num++;
                        num += WzTools.GetCompressedIntLength(((WzCompressedIntProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num3]).Value);
                        goto Label_032E;

                    case WzPropertyType.ByteFloat:
                        num++;
                        if (((WzByteFloatProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num3]).Value != 0f)
                        {
                            num += 5;
                        }
                        else
                        {
                            num++;
                        }
                        goto Label_032E;

                    case WzPropertyType.Double:
                        num++;
                        num += 8;
                        goto Label_032E;

                    case WzPropertyType.String:
                        num += 2;
                        num += WzTools.GetEncodedStringLength(((WzStringProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num3]).Value);
                        goto Label_032E;

                    case WzPropertyType.Extended:
                        num += 5;
                        num += ((WzExtendedProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num3]).GetExtendedPropertyLength();
                        goto Label_032E;
                }
                goto Label_032E;
            }
        Label_0350:
            num += WzTools.GetCompressedIntLength(((WzCanvasProperty) this.extendedProperty).PngProperty.Width);
            num += WzTools.GetCompressedIntLength(((WzCanvasProperty) this.extendedProperty).PngProperty.Height);
            num += WzTools.GetCompressedIntLength(((WzCanvasProperty) this.extendedProperty).PngProperty.Format);
            num += 10;
            return (num + ((WzCanvasProperty) this.extendedProperty).PngProperty.CompressedBytes.Length);
        }

        internal void ParseExtendedProperty(BinaryReader wzReader)
        {
            this.wzReader = wzReader;
            this.DumpBlock(this.endOfBlock, this.name);
        }

        internal void SaveExtendedProperty(BinaryWriter wzWriter)
        {
            int num;
            int num5;
            switch (this.extendedProperty.PropertyType)
            {
                case WzPropertyType.SubProperty:
                    wzWriter.Write((byte) 0x73);
                    WzTools.WriteEncodedString(wzWriter, "Property");
                    wzWriter.Write((ushort) 0);
                    WzTools.WriteCompressedInt(wzWriter, ((WzSubProperty) this.extendedProperty).WzProperties.Length);
                    num = 0;
                    goto Label_0275;

                case WzPropertyType.Canvas:
                    wzWriter.Write((byte) 0x73);
                    WzTools.WriteEncodedString(wzWriter, "Canvas");
                    wzWriter.Write((byte) 0);
                    if (((WzCanvasProperty) this.extendedProperty).WzProperties.Length <= 0)
                    {
                        wzWriter.Write((byte) 0);
                        goto Label_052F;
                    }
                    wzWriter.Write((byte) 1);
                    wzWriter.Write((ushort) 0);
                    WzTools.WriteCompressedInt(wzWriter, ((WzCanvasProperty) this.extendedProperty).WzProperties.Length);
                    num5 = 0;
                    goto Label_050D;

                case WzPropertyType.Vector:
                    wzWriter.Write((byte) 0x73);
                    WzTools.WriteEncodedString(wzWriter, "Shape2D#Vector2D");
                    WzTools.WriteCompressedInt(wzWriter, ((WzVectorProperty) this.extendedProperty).X.Value);
                    WzTools.WriteCompressedInt(wzWriter, ((WzVectorProperty) this.extendedProperty).Y.Value);
                    return;

                case WzPropertyType.Convex:
                    wzWriter.Write((byte) 0x73);
                    WzTools.WriteEncodedString(wzWriter, "Shape2D#Convex2D");
                    WzTools.WriteCompressedInt(wzWriter, ((WzConvexProperty) this.extendedProperty).WzProperties.Length);
                    for (int i = 0; i < ((WzConvexProperty) this.extendedProperty).WzProperties.Length; i++)
                    {
                        ((WzConvexProperty) this.extendedProperty).WzProperties[i].SaveExtendedProperty(wzWriter);
                    }
                    return;

                case WzPropertyType.Sound:
                    wzWriter.Write((byte) 0x73);
                    WzTools.WriteEncodedString(wzWriter, "Sound_DX8");
                    wzWriter.Write((byte) 0);
                    WzTools.WriteCompressedInt(wzWriter, ((WzSoundProperty) this.extendedProperty).SoundData.Length);
                    WzTools.WriteCompressedInt(wzWriter, 0);
                    wzWriter.Write(((WzSoundProperty) this.extendedProperty).SoundData);
                    return;

                case WzPropertyType.UOL:
                    wzWriter.Write((byte) 0x73);
                    WzTools.WriteEncodedString(wzWriter, "UOL");
                    wzWriter.Write((byte) 0);
                    wzWriter.Write((byte) 0);
                    WzTools.WriteEncodedString(wzWriter, ((WzUOLProperty) this.extendedProperty).Value);
                    return;

                default:
                    return;
            }
        Label_0271:
            num++;
        Label_0275:
            if (num < ((WzSubProperty) this.extendedProperty).WzProperties.Length)
            {
                wzWriter.Write((byte) 0);
                WzTools.WriteEncodedString(wzWriter, ((WzSubProperty) this.extendedProperty).WzProperties[num].Name);
                switch (((WzSubProperty) this.extendedProperty).WzProperties[num].PropertyType)
                {
                    case WzPropertyType.Null:
                        wzWriter.Write((byte) 0);
                        goto Label_0271;

                    case WzPropertyType.UnsignedShort:
                        wzWriter.Write((byte) 2);
                        wzWriter.Write(((WzUnsignedShortProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num]).Value);
                        goto Label_0271;

                    case WzPropertyType.CompressedInt:
                        wzWriter.Write((byte) 3);
                        WzTools.WriteCompressedInt(wzWriter, ((WzCompressedIntProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num]).Value);
                        goto Label_0271;

                    case WzPropertyType.ByteFloat:
                        wzWriter.Write((byte) 4);
                        if (((WzByteFloatProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num]).Value != 0f)
                        {
                            wzWriter.Write((byte) 0x80);
                            wzWriter.Write(((WzByteFloatProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num]).Value);
                        }
                        else
                        {
                            wzWriter.Write((byte) 0);
                        }
                        goto Label_0271;

                    case WzPropertyType.Double:
                        wzWriter.Write((byte) 5);
                        wzWriter.Write(((WzDoubleProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num]).Value);
                        goto Label_0271;

                    case WzPropertyType.String:
                        wzWriter.Write((byte) 8);
                        wzWriter.Write((byte) 0);
                        WzTools.WriteEncodedString(wzWriter, ((WzStringProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num]).Value);
                        goto Label_0271;

                    case WzPropertyType.Extended:
                    {
                        wzWriter.Write((byte) 9);
                        long position = wzWriter.BaseStream.Position;
                        wzWriter.Write(0);
                        ((WzExtendedProperty) ((WzSubProperty) this.extendedProperty).WzProperties[num]).SaveExtendedProperty(wzWriter);
                        int num3 = (int) (wzWriter.BaseStream.Position - position);
                        long num4 = wzWriter.BaseStream.Position;
                        wzWriter.BaseStream.Position = position;
                        wzWriter.Write((int) (num3 - 4));
                        wzWriter.BaseStream.Position = num4;
                        goto Label_0271;
                    }
                }
                goto Label_0271;
            }
            return;
        Label_0507:
            num5++;
        Label_050D:
            if (num5 < ((WzCanvasProperty) this.extendedProperty).WzProperties.Length)
            {
                wzWriter.Write((byte) 0);
                WzTools.WriteEncodedString(wzWriter, ((WzCanvasProperty) this.extendedProperty).WzProperties[num5].Name);
                switch (((WzCanvasProperty) this.extendedProperty).WzProperties[num5].PropertyType)
                {
                    case WzPropertyType.Null:
                        wzWriter.Write((byte) 0);
                        goto Label_0507;

                    case WzPropertyType.UnsignedShort:
                        wzWriter.Write((byte) 2);
                        wzWriter.Write(((WzUnsignedShortProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num5]).Value);
                        goto Label_0507;

                    case WzPropertyType.CompressedInt:
                        wzWriter.Write((byte) 3);
                        WzTools.WriteCompressedInt(wzWriter, ((WzCompressedIntProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num5]).Value);
                        goto Label_0507;

                    case WzPropertyType.ByteFloat:
                        wzWriter.Write((byte) 4);
                        if (((WzByteFloatProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num5]).Value != 0f)
                        {
                            wzWriter.Write((byte) 0x80);
                            wzWriter.Write(((WzByteFloatProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num5]).Value);
                        }
                        else
                        {
                            wzWriter.Write((byte) 0);
                        }
                        goto Label_0507;

                    case WzPropertyType.Double:
                        wzWriter.Write((byte) 5);
                        wzWriter.Write(((WzDoubleProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num5]).Value);
                        goto Label_0507;

                    case WzPropertyType.String:
                        wzWriter.Write((byte) 8);
                        wzWriter.Write((byte) 0);
                        WzTools.WriteEncodedString(wzWriter, ((WzStringProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num5]).Value);
                        goto Label_0507;

                    case WzPropertyType.Extended:
                    {
                        wzWriter.Write((byte) 9);
                        long num6 = wzWriter.BaseStream.Position;
                        wzWriter.Write(0);
                        ((WzExtendedProperty) ((WzCanvasProperty) this.extendedProperty).WzProperties[num5]).SaveExtendedProperty(wzWriter);
                        int num7 = (int) (wzWriter.BaseStream.Position - num6);
                        long num8 = wzWriter.BaseStream.Position;
                        wzWriter.BaseStream.Position = num6;
                        wzWriter.Write((int) (num7 - 4));
                        wzWriter.BaseStream.Position = num8;
                        goto Label_0507;
                    }
                }
                goto Label_0507;
            }
        Label_052F:
            WzTools.WriteCompressedInt(wzWriter, ((WzCanvasProperty) this.extendedProperty).PngProperty.Width);
            WzTools.WriteCompressedInt(wzWriter, ((WzCanvasProperty) this.extendedProperty).PngProperty.Height);
            WzTools.WriteCompressedInt(wzWriter, ((WzCanvasProperty) this.extendedProperty).PngProperty.Format);
            wzWriter.Write((byte) 0);
            wzWriter.Write(0);
            wzWriter.Write((int) (((WzCanvasProperty) this.extendedProperty).PngProperty.CompressedBytes.Length + 1));
            wzWriter.Write((byte) 0);
            wzWriter.Write(((WzCanvasProperty) this.extendedProperty).PngProperty.CompressedBytes);
        }

        public IWzImageProperty ExtendedProperty
        {
            get
            {
                return this.extendedProperty;
            }
            set
            {
                this.extendedProperty = value;
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
                return WzObjectType.Property;
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

        public WzImage ParentImage
        {
            get
            {
                return this.imgParent;
            }
            set
            {
                this.imgParent = value;
            }
        }

        public WzPropertyType PropertyType
        {
            get
            {
                return WzPropertyType.Extended;
            }
        }
    }
}

