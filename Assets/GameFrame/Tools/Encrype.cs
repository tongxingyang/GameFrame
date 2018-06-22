using System;
namespace GameFrame
{
    public static class LuaEncrype
    {
        private const int KeyCount = 256;
        private static int[] sbox;
        private static  string KeyString = Platform.LuaBundleKey;
        public static void  InitKey()
        {
            sbox = new int[KeyCount];
            int b = 0;
            int[] key = new int[KeyCount];
            int n = KeyString.Length; //7
            for (int a = 0; a < KeyCount; a++)
            {
                key[a] = (int)KeyString[a % n];
                sbox[a] = a;
            }

            for (int a = 0; a < KeyCount; a++)
            {
                b = (b + sbox[a] + key[a]) % KeyCount;
                int tempSwap = sbox[a];
                sbox[a] = sbox[b];
                sbox[b] = tempSwap;
            }
        }
        public static byte[] Encrypt(byte[] src)
        {
            InitKey();
            int i = 0, j = 0, k  = 0;
            for (int a = 0; a < src.Length; a++)
            {
                i = (i + 1) % KeyCount;
                j = (j + sbox[i]) % KeyCount;
                int tempSwap = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = tempSwap;

                k = sbox[(sbox[i] + sbox[j]) % KeyCount];
                int cipherBy = ((int)src[a]) ^ k;  
                src[a] = Convert.ToByte(cipherBy);
            }
            return src;
        }
        public static byte[] Decrypt(byte[] src)
        {
            return Encrypt(src);
        }
    }

    public static class ConfigEncrypt
    {
        private const int KeyCount = 256;
        private static int[] sbox;
        private static string KeyString = Platform.ConfigBundleKey;
        public static void  InitKey()
        {
            sbox = new int[KeyCount];
            int b = 0;
            int[] key = new int[KeyCount];
            int n = KeyString.Length; //7
            for (int a = 0; a < KeyCount; a++)
            {
                key[a] = (int)KeyString[a % n];
                sbox[a] = a;
            }

            for (int a = 0; a < KeyCount; a++)
            {
                b = (b + sbox[a] + key[a]) % KeyCount;
                int tempSwap = sbox[a];
                sbox[a] = sbox[b];
                sbox[b] = tempSwap;
            }
        }
        public static byte[] Encrypt(byte[] src)
        {
            InitKey();
            int i = 0, j = 0, k  = 0;
            for (int a = 0; a < src.Length; a++)
            {
                i = (i + 1) % KeyCount;
                j = (j + sbox[i]) % KeyCount;
                int tempSwap = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = tempSwap;

                k = sbox[(sbox[i] + sbox[j]) % KeyCount];
                int cipherBy = ((int)src[a]) ^ k;  
                src[a] = Convert.ToByte(cipherBy);
            }
            return src;
        }
        public static byte[] Decrypt(byte[] src)
        {
            return Encrypt(src);
        }
    }
}