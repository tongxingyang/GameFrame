using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameFrame
{
    class MD5Util
    {
        public static string GetHexString(byte[] bytes)
        {
            int bufferLength = bytes.Length << 1;
            char[] buffer = new char[bufferLength];
            int index = 0;
            for (int i = 0; i < bufferLength; i += 2)
            {
                byte one = bytes[index++];
                int a = one >> 4;
                int b = one - (a << 4);
                buffer[i] = a < 10 ? (char)(a + 48) : (char)(a - 10 + 97);
                buffer[i + 1] = b < 10 ? (char)(b + 48) : (char)(b - 10 + 97);
            }

            return new string(buffer);
        }

        public static string ComputeHash(byte[] bytes)
        {
            MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();
            byte[] buffer = _md5.ComputeHash(bytes);
            string hash = GetHexString(buffer);
            _md5.Clear();
            return hash;
        }

        public static string ComputeHashUTF8(string text)
        {
            MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(text);
            string hash = ComputeHash(bytes);
            _md5.Clear();
            return hash;
        }

        public static string ComputeHashUnicode(string text)
        {
            MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            string hash = ComputeHash(bytes);
            _md5.Clear();
            return hash;
        }

        public static string ComputeHash(Stream sm)
        {
            MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();
            byte[] buffer = _md5.ComputeHash(sm);
            string hash = GetHexString(buffer);
            _md5.Clear();
            return hash;
        }

        public static string ComputeFileHash(string filename)
        {
            try
            {
                MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();
                int bufferSize = 1048576; 
                byte[] buff = new byte[bufferSize];
                long offset = 0;
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    while (offset < fs.Length)
                    {
                        int readSize = bufferSize;
                        if (offset + readSize > fs.Length)
                        {
                            readSize = (int)(fs.Length - offset);
                        }

                        fs.Read(buff, 0, readSize); 

                        if (offset + readSize < fs.Length) 
                        {
                            _md5.TransformBlock(buff, 0, readSize, buff, 0);
                        }
                        else    
                        {
                            _md5.TransformFinalBlock(buff, 0, readSize);
                        }

                        offset += bufferSize;
                    }
                    string md5 = GetHexString(_md5.Hash);
                    _md5.Clear();
                    return md5;
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                //UnityEngine.Debug.Log(filename + " compute md5 error");
                //DebugEx.LogError("filename={0}, ex={1}", filename, ex);
            }

            return string.Empty;
        }
    }

}
