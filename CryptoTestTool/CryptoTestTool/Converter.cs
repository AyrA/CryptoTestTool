using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace CryptoTest
{
    public static class Converter
    {
        public static string FromBytes(byte[] data)
        {
            StringBuilder SB = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                SB.AppendFormat("{0:X2}", b);
            }
            return SB.ToString();
        }

        public static byte[] FromString(string S)
        {
            byte[] ret = new byte[S.Length / 2];
            for (int i = 0; i < S.Length; i+=2)
            {
                ret[i / 2] = byte.Parse(S.Substring(i, 2), NumberStyles.HexNumber);
            }
            return ret;
        }
    }
}
