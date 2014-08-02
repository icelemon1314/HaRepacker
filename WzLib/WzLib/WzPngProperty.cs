namespace WzLib
{
    //using ManagedZLib;
    using System;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;

    public class WzPngProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal byte[] compressedBytes;
        internal int format;
        internal int format2;
        internal int height;
        internal WzImage imgParent;
        internal bool isNew;
        internal IWzObject parent;
        internal Bitmap png;
        internal int width;
        public WzFile ParentFile = null;

        public WzPngProperty()
        {
        }

        internal WzPngProperty(BinaryReader wzReader)
        {
            this.width = WzTools.ReadCompressedInt(wzReader);
            this.height = WzTools.ReadCompressedInt(wzReader);
            this.format = WzTools.ReadCompressedInt(wzReader);
            this.format2 = wzReader.ReadByte();
            Stream baseStream = wzReader.BaseStream;
            baseStream.Position += 4L;
            int count = wzReader.ReadInt32() - 1;
            Stream stream2 = wzReader.BaseStream;
            stream2.Position += 1L;
            if (count > 0)
            {
                this.compressedBytes = wzReader.ReadBytes(count);
            }
        }

        internal byte[] Compress(byte[] decompressedBuffer)
        {
            MemoryStream writeStream = new MemoryStream();
            ManagedZLib.Compress compress = new ManagedZLib.Compress(writeStream, ManagedZLib.CompressionOptions.CompressBest);
            compress.Write(decompressedBuffer);
            compress.Flush();
            byte[] buffer = new byte[compress.BytesOut];
            writeStream.Position = 0L;
            writeStream.Read(buffer, 0, buffer.Length);
            /*DeflateStream compress = new DeflateStream(writeStream, CompressionMode.Compress);
            compress.Write(decompressedBuffer,0,decompressedBuffer.Length);
            compress.Flush();
            byte[] buffer = new byte[writeStream.Length + 2];
            buffer[0] = 120;
            buffer[1] = 156;
            writeStream.Position = 0L;
            writeStream.Read(buffer, 2, buffer.Length - 2);*/
            return buffer;
        }

        internal void CompressPng(Bitmap bmp)
        {
            byte[] decompressedBuffer = new byte[(bmp.Width * bmp.Height) * 8];
            this.format = 2;
            this.format2 = 0;
            this.width = bmp.Width;
            this.height = bmp.Height;
            int index = 0;
            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    decompressedBuffer[index] = pixel.B;
                    decompressedBuffer[index + 1] = pixel.G;
                    decompressedBuffer[index + 2] = pixel.R;
                    decompressedBuffer[index + 3] = pixel.A;
                    index += 4;
                }
            }
            this.compressedBytes = this.Compress(decompressedBuffer);
            if (this.isNew)
            {
                BinaryWriter writer = new BinaryWriter(new MemoryStream());
                writer.Write(2);
                for (int k = 0; k < 2; k++)
                {
                    writer.Write((byte) (this.compressedBytes[k] ^ WzTools.wzKey[k]));
                }
                writer.Write((int) (this.compressedBytes.Length - 2));
                for (int m = 2; m < this.compressedBytes.Length; m++)
                {
                    writer.Write((byte) (this.compressedBytes[m] ^ WzTools.wzKey[m - 2]));
                }
                this.compressedBytes = ((MemoryStream) writer.BaseStream).GetBuffer();
            }
        }

        internal byte[] Decompress(byte[] compressedBuffer, int decompressedSize)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(compressedBuffer, 2, compressedBuffer.Length - 2);
            byte[] buffer = new byte[decompressedSize];
            stream.Position = 0L;
            DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress);
            stream2.Read(buffer, 0, buffer.Length);
            stream2.Close();
            stream2.Dispose();
            stream.Close();
            stream.Dispose();
            return buffer;
        }

        public void Dispose()
        {
            this.compressedBytes = null;
            if (this.png != null)
            {
                this.png.Dispose();
                this.png = null;
            }
        }

        private int ConvertToDword(byte[] from)
        {
            byte[] tempb = new byte[4];
            tempb[0] = from[3];
            tempb[1] = from[2];
            tempb[2] = from[1];
            tempb[3] = from[0];
            int temp0;
            int temp1;
            int temp2;
            int temp3;
            int temp;
            temp0 = Convert.ToInt32(tempb[0]) * 0x1000000;
            temp1 = Convert.ToInt32(tempb[1]) * 0x10000;
            temp2 = Convert.ToInt32(tempb[2]) * 0x100;
            temp3 = Convert.ToInt32(tempb[3]);
            temp = temp0 + temp1 + temp2 + temp3;
            return temp;
        }

        private byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
        
        private byte[] GetPngSub(BinaryReader stream)
        {
            byte[] LenByte = new byte[4];
            stream.Read(LenByte, 0, 4);
            int SubLen = ConvertToDword(LenByte);
            byte[] data = new byte[SubLen];
            stream.Read(data, 0, SubLen);
            return data;
        }

        internal void ParsePng()
        {
            byte[] buffer;
            int num6;
            int num7;
            int num8;
            int num9;
            int decompressedSize = 0;
            switch ((this.format + this.format2))
            {
                case 1:
                case 0x201:
                    decompressedSize = (this.height * this.width) * 4;
                    break;

                case 2:
                    decompressedSize = (this.height * this.width) * 8;
                    break;

                case 0x205:
                    decompressedSize = (this.height * this.width) / 0x80;
                    break;
            }
            try
            {
                buffer = this.Decompress(this.compressedBytes, decompressedSize);
            }
            catch
            {
                try
                {
                    BinaryReader reader = new BinaryReader(new MemoryStream(this.compressedBytes));
                    int csize = 0;
                    byte[] final = new byte[0];
                    while (true)
                    {
                        byte[] encrypted = GetPngSub(reader);
                        csize += encrypted.Length;
                        byte[] decrypted = new byte[encrypted.Length];
                        for (int i = 0; i < encrypted.Length; i++)
                        {
                            decrypted[i] = (byte)(encrypted[i] ^ WzTools.wzKey[i]);
                        }
                        final = Combine(final, decrypted);
                        if (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            continue;
                        }
                        break;
                    }
                    buffer = this.Decompress(final, decompressedSize);
                    this.isNew = true;
                }
                catch
                {
                    buffer = new byte[decompressedSize];
                }
            }
            Bitmap bitmap = new Bitmap(this.width, this.height);
            int x = 0;
            int y = 0;
            switch ((this.format + this.format2))
            {
                case 1:
                    for (int k = 0; k < decompressedSize; k += 2)
                    {
                        if (x == this.width)
                        {
                            x = 0;
                            y++;
                            if (y == this.height)
                            {
                                break;
                            }
                        }
                        num9 = buffer[k] & 15;
                        num9 |= num9 << 4;
                        num8 = buffer[k] & 240;
                        num8 |= num8 >> 4;
                        num7 = buffer[k + 1] & 15;
                        num7 |= num7 << 4;
                        num6 = buffer[k + 1] & 240;
                        num6 |= num6 >> 4;
                        bitmap.SetPixel(x, y, Color.FromArgb(num6, num7, num8, num9));
                        x++;
                    }
                    break;

                case 2:
                    for (int m = 0; m < decompressedSize; m += 4)
                    {
                        if (x == this.width)
                        {
                            x = 0;
                            y++;
                            if (y == this.height)
                            {
                                break;
                            }
                        }
                        bitmap.SetPixel(x, y, Color.FromArgb(buffer[m + 3], buffer[m + 2], buffer[m + 1], buffer[m]));
                        x++;
                    }
                    break;

                case 0x201:
                    for (int n = 0; n < decompressedSize; n += 2)
                    {
                        if (x == this.width)
                        {
                            x = 0;
                            y++;
                            if (y == this.height)
                            {
                                break;
                            }
                        }
                        num9 = (buffer[n] & 0x1f) << 3;
                        num9 |= num9 >> 5;
                        num8 = ((buffer[n + 1] & 7) << 5) | ((buffer[n] & 0xe0) >> 3);
                        num8 |= num8 >> 6;
                        num7 = buffer[n + 1] & 0xf8;
                        num7 |= num7 >> 5;
                        num6 = 0xff;
                        bitmap.SetPixel(x, y, Color.FromArgb(num6, num7, num8, num9));
                        x++;
                    }
                    break;

                case 0x205:
                {
                    byte red = 0;
                    for (int num16 = 0; num16 < decompressedSize; num16++)
                    {
                        for (byte num17 = 0; num17 < 8; num17 = (byte) (num17 + 1))
                        {
                            red = (byte) (((buffer[num16] & (1 << ((7 - num17) & 0x1f))) >> (7 - num17)) * 0xff);
                            for (int num18 = 0; num18 < 0x10; num18++)
                            {
                                if (x == this.width)
                                {
                                    x = 0;
                                    y++;
                                    if (y == this.height)
                                    {
                                        break;
                                    }
                                }
                                bitmap.SetPixel(x, y, Color.FromArgb(0xff, red, red, red));
                                x++;
                            }
                        }
                    }
                    break;
                }
            }
            this.png = bitmap;
        }

        internal byte[] CompressedBytes
        {
            get
            {
                return this.compressedBytes;
            }
        }

        public int Format
        {
            get
            {
                return (this.format + this.format2);
            }
            set
            {
                this.format = value;
                this.format2 = 0;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        public string Name
        {
            get
            {
                return "PNG";
            }
            set
            {
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

        public Bitmap PNG
        {
            get
            {
                if (this.png == null)
                {
                    this.ParsePng();
                }
                return this.png;
            }
            set
            {
                this.png = value;
                this.CompressPng(value);
            }
        }

        public WzPropertyType PropertyType
        {
            get
            {
                return WzPropertyType.PNG;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
    }
}

