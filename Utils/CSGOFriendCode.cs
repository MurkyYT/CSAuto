using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils
{
    public static class CSGOFriendCode
    {
        const string alnum = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public static string B32(ulong id)
        {
            StringBuilder sr = new StringBuilder();
            UInt64 tmp = BinaryPrimitives.ReverseEndianness(id);
            for (int i = 0; i < 13; i++)
            {
                if (i == 4 || i == 9)
                    sr.Append("-");
                int num = (int)(tmp & 0x1f);
                sr.Append(alnum[num]);
                tmp >>= 5;
            }
            return sr.ToString();
        }
        private static ulong CalculateHash (ulong id) 
        {
            const int DEST_SIZE = 16; // 128 bits / 8 = 16 bytes
            ulong account_id = id & 0xFFFFFFFF;
            ulong strange_steam_id = account_id | 0x4353474F00000000;
            byte[] source = new byte[8];
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = (byte)(strange_steam_id);
                strange_steam_id >>= 8;
            }
            Span<byte> dest = stackalloc byte[DEST_SIZE];
            var md5 = new MD5CryptoServiceProvider();
            try
            {
                dest = md5.ComputeHash(source, 0, 8);
                return BinaryPrimitives.ReadUInt32LittleEndian(dest);
            }
            catch
            {
                throw new CryptographicException();
            }
        }
        public static string Encode(string steamid)
        {
            UInt64 tmp = UInt64.Parse(steamid);
            ulong h = CalculateHash(tmp);

            ulong r = 0;
            for (int i = 0; i < 8; i++)
            {
                ulong id_nibble = tmp & 0xF;
                tmp >>= 4;

                ulong hash_nibble = (h >> i) & 1;

                ulong a = r << 4 | id_nibble;

                r = MakeU64(r >> 28, a);
                r = MakeU64(r >> 31, a << 1 | hash_nibble);
            }
            string res = B32(r);
            if(res.Substring(0,4) == "AAAA")
                res = res.Substring(5);
            return res;
        }

        private static ulong MakeU64(ulong v, ulong a)
        {
            return v << 32 | a;
        }
    }
}
