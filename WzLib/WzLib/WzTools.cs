namespace WzLib
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    public class WzTools
    {
        public static uint[] checksumXorTable = new uint[] { 
            0, 0x4c11db7, 0x9823b6e, 0xd4326d9, 0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61, 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 
            0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75, 0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd, 
            0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039, 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d, 
            0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1, 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 
            0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072, 0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x18aeb13, 0x54bf6a4, 0x808d07d, 0xcc9cdca, 
            0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde, 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba, 
            0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6, 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 
            0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2, 0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a, 
            0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637, 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53, 
            0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b, 0x315d626, 0x7d4cb91, 0xa97ed48, 0xe56f0ff, 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 
            0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b, 0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3, 
            0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7, 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3, 
            0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8, 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 
            0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30, 0x29f3d35, 0x65e2082, 0xb1d065b, 0xfdc1bec, 0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654, 
            0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0, 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4, 
            0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668, 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4
         };
        public static byte[] wzKey;
        public static char[] wzUniKey;

        // 生成解密wz文件KEY
        public static void CreateWzKey(WzMapleVersion gameVersion)
        {
            string path = "";
            switch (gameVersion)
            {
                case WzMapleVersion.BMS:
                    wzKey = new byte[0xffff];
                    break;
                case WzMapleVersion.EMS:
                    path = Path.Combine(Application.StartupPath,"ems.key");
                    if (File.Exists(path))
                        wzKey = File.ReadAllBytes(path);
                    else
                        wzKey = new WzAes().getKeys(new byte[] { 0xb9, 0x7d, 0x63, 0xe9 });
                    break;
                case WzMapleVersion.GMS:
                    path = Path.Combine(Application.StartupPath, "gms.key");
                    if (File.Exists(path))
                        wzKey = File.ReadAllBytes(path);
                    else
                        wzKey = new WzAes().getKeys(new byte[] { 0x4d, 0x23, 0xc7, 0x2b });
                    break;
            }
            wzUniKey = CreateUniKey(wzKey);
        }

        // 创建UniKey
        public static char[] CreateUniKey(byte[] key)
        {
            char[] wzUniKey = new char[key.Length / 2];
            for (int i = 0; i < wzUniKey.Length; i++)
            {
                wzUniKey[i] = (char)((key[(i * 2) + 1] << 8) + key[i * 2]);
            }
            return wzUniKey;
        }

        public static string DecryptNonUnicodeString(char[] stringToDecrypt)
        {
            /*string str = "";
            for (int i = 0; i < stringToDecrypt.Length; i++)
            {
                str = str + ((char)(stringToDecrypt[i] ^ wzKey[i]));
            }
            return str;*/
            StringBuilder str = new StringBuilder();
            int length=stringToDecrypt.Length;
            for (int i=0; i < length; i++) {
                str.Append(((char)(stringToDecrypt[i] ^ wzKey[i])));
            }

            return str.ToString();

        }
        // 解密字符串
        public static string DecryptString(char[] stringToDecrypt)
        {
            string str = "";
            for (int i = 0; i < stringToDecrypt.Length; i++)
            {
                str = str + ((char)(stringToDecrypt[i] ^ wzUniKey[i]));
            }
            return str;
        }

        public static void DelDir(string path)
        {
            foreach (string str in Directory.GetFiles(path))
            {
                File.Delete(str);
            }
            foreach (string str2 in Directory.GetDirectories(path))
            {
                DelDir(str2);
            }
            Directory.Delete(path);
        }

        public static uint GetChecksum(BinaryReader rdr, int length, uint start)
        {
            uint num = start;
            if (length > 0)
            {
                byte[] buffer = rdr.ReadBytes(length);
                for (int i = 0; i < length; i++)
                {
                    num = checksumXorTable[buffer[i] ^ (num >> 0x18)] ^ (num << 8);
                }
            }
            return num;
        }

        public static int GetCompressedIntLength(int i)
        {
            if ((i <= 0x7f) && (i >= -127))
            {
                return 1;
            }
            return 5;
        }

        public static int GetEncodedStringLength(string s)
        {
            int num = 0;
            if (string.IsNullOrEmpty(s))
            {
                return 1;
            }
            bool flag = false;
            foreach (char ch in s)
            {
                if (ch > '\x00ff')
                {
                    flag = true;
                }
            }
            if (flag)
            {
                if (s.Length > 0x7e)
                {
                    num += 5;
                }
                else
                {
                    num++;
                }
                return (num + (s.Length * 2));
            }
            if (s.Length > 0x7f)
            {
                num += 5;
            }
            else
            {
                num++;
            }
            return (num + s.Length);
        }

        public static int ReadCompressedInt(BinaryReader reader)
        {
            sbyte num = reader.ReadSByte();
            if (num == -128)
            {
                return reader.ReadInt32();
            }
            return num;
        }

        public static string ReadDecodedString(BinaryReader FileReader)
        {
            uint num2;
            sbyte num = FileReader.ReadSByte();
            if (num == 0)
            {
                return "";
            }
            if (num > 0)
            {
                ushort num3 = 0xaaaa;
                if (num == 0x7f)
                {
                    num2 = FileReader.ReadUInt32();
                }
                else
                {
                    num2 = (uint) num;
                }
                if (num2 < 0)
                {
                    return "";
                }
                char[] chArray = new char[num2];
                for (int j = 0; j < num2; j++)
                {
                    ushort num5 = (ushort) (FileReader.ReadUInt16() ^ num3);
                    num3 = (ushort) (num3 + 1);
                    chArray[j] = (char) num5;
                }
                return DecryptString(chArray);
            }
            byte num6 = 170;
            if (num == -128)
            {
                num2 = FileReader.ReadUInt32();
            }
            else
            {
                num2 = (uint) -num;
            }
            char[] stringToDecrypt = new char[num2];
            for (int i = 0; i < num2; i++)
            {
                byte num8 = (byte) (FileReader.ReadByte() ^ num6);
                num6 = (byte) (num6 + 1);
                stringToDecrypt[i] = (char) num8;
            }
            return DecryptNonUnicodeString(stringToDecrypt);
        }

        public static string ReadDecodedStringAtOffset(BinaryReader FileReader, long Offset)
        {
            FileReader.BaseStream.Position = Offset;
            return ReadDecodedString(FileReader);
        }

        public static string ReadDecodedStringAtOffsetAndReset(BinaryReader FileReader, long Offset)
        {
            long position = FileReader.BaseStream.Position;
            FileReader.BaseStream.Position = Offset;
            string str = ReadDecodedString(FileReader);
            FileReader.BaseStream.Position = position;
            return str;
        }

        public static string ReadDecodedStringAtOffsetAndReset(BinaryReader FileReader, long Offset, bool flag)
        {
            long position = FileReader.BaseStream.Position;
            FileReader.BaseStream.Position = Offset;
            if (flag)
            {
                FileReader.ReadByte();
            }
            string str = ReadDecodedString(FileReader);
            FileReader.BaseStream.Position = position;
            return str;
        }

        // 读取WZ文件的版本信息：Package file v1.0 Copyright 2002 Wizet, ZMS
        /**
           50 61 63 6B 61 67 65 20 66 69 6C 65 20 76 31 2E 30 20 43 6F 70 79 72 69 67 68 74 20 32 30 30 32 20 57 69 7A 65 74 2C 20 5A 4D 53 00
         */
        public static string ReadNullTerminatedString(BinaryReader FileReader)
        {
            StringBuilder builder = new StringBuilder();
            sbyte num = 1;
            while (true)
            {
                num = FileReader.ReadSByte();
                if (num == 0)
                {
                    break;
                }
                builder.Append((char) ((ushort) num));
            }
            return builder.ToString();
        }

        public static void WriteCompressedInt(BinaryWriter wzWriter, int i)
        {
            sbyte num = -128;
            if ((i > 0x7f) || (i < -127))
            {
                wzWriter.Write(num);
                wzWriter.Write(i);
            }
            else
            {
                num = (sbyte) i;
                wzWriter.Write(num);
            }
        }

        public static void WriteEncodedString(BinaryWriter wzWriter, string s)
        {
            sbyte length = 0;
            bool flag = false;
            if (string.IsNullOrEmpty(s))
            {
                wzWriter.Write(length);
            }
            else
            {
                foreach (char ch in s)
                {
                    if (ch > '\x00ff')
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    ushort num2 = 0xaaaa;
                    if (s.Length > 0x7e)
                    {
                        length = 0x7f;
                        wzWriter.Write(length);
                        wzWriter.Write((uint) s.Length);
                    }
                    else
                    {
                        length = (sbyte) s.Length;
                        wzWriter.Write(length);
                    }
                    for (int i = 0; i < s.Length; i++)
                    {
                        ushort num4 = s[i];
                        num4 = (ushort) (num4 ^ num2);
                        num2 = (ushort) (num2 + 1);
                        num4 ^= wzUniKey[i];
                        wzWriter.Write(num4);
                    }
                }
                else
                {
                    byte num5 = 170;
                    if (s.Length > 0x7f)
                    {
                        length = -128;
                        wzWriter.Write(length);
                        wzWriter.Write((uint) s.Length);
                    }
                    else
                    {
                        length = (sbyte) -s.Length;
                        wzWriter.Write(length);
                    }
                    for (int j = 0; j < s.Length; j++)
                    {
                        byte num7 = (byte) s[j];
                        num7 = (byte) (num7 ^ num5);
                        num5 = (byte) (num5 + 1);
                        num7 = (byte) (num7 ^ wzKey[j]);
                        wzWriter.Write(num7);
                    }
                }
            }
        }

        public static void WriteNullTerminatedString(BinaryWriter wzWriter, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                wzWriter.Write((byte) s[i]);
            }
            byte num2 = 0;
            wzWriter.Write(num2);
        }
    }
}

