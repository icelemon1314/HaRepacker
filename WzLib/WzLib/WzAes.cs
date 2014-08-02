/**
 * AES加解密类
 * 
 */

namespace WzLib
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public class WzAes
    {
        private AesManaged crypto = new AesManaged();
        private CryptoStream cryptoStream;
        private byte[] key = new byte[] { 
            0x13, 0, 0, 0, 8, 0, 0, 0, 6, 0, 0, 0, 180, 0, 0, 0, 
            0x1b, 0, 0, 0, 15, 0, 0, 0, 0x33, 0, 0, 0, 0x52, 0, 0, 0
         };
        private MemoryStream memStream;

        public WzAes()
        {
            this.crypto.KeySize = 0x100;
            this.crypto.Key = this.key;
            this.crypto.Mode = CipherMode.ECB;
            this.memStream = new MemoryStream();
            this.cryptoStream = new CryptoStream(this.memStream, this.crypto.CreateEncryptor(), CryptoStreamMode.Write);
        }

        public void dispose()
        {
            this.memStream.Dispose();
            this.cryptoStream.Dispose();
        }

        // 根据IV获取一组KEY
        public byte[] getKeys(byte[] iv)
        {
            byte[] destinationArray = new byte[0xffff];
            byte[] buffer = this.multiplyBytes(iv, 4, 4);
            for (int i = 0; i < (destinationArray.Length / 0x10); i++)
            {
                this.cryptoStream.Write(buffer, 0, 0x10);
                buffer = this.memStream.ToArray();
                Array.Copy(this.memStream.ToArray(), 0, destinationArray, i * 0x10, 0x10);
                this.memStream.Position = 0L;
            }
            this.cryptoStream.Write(buffer, 0, 0x10);
            Array.Copy(this.memStream.ToArray(), 0, destinationArray, destinationArray.Length - 15, 15);
            return destinationArray;
        }

        private byte[] multiplyBytes(byte[] input, int count, int mul)
        {
            byte[] buffer = new byte[count * mul];
            for (int i = 0; i < (count * mul); i++)
            {
                buffer[i] = input[i % count];
            }
            return buffer;
        }
    }
}

