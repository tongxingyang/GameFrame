using System.IO;
using System.Security.Cryptography;
using System.Text;
using GameFrameDebuger;

namespace GameFrame
{
    public class MD5Util
    {
        public static string GetHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++) {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
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
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] retVal = _md5.ComputeHash(fs);
                    string str = GetHexString(retVal);
                    return str;
                }
            }
            catch (System.Exception ex)
            {
                Debuger.LogError(ex);
            }

            return string.Empty;
        }
    }

}
