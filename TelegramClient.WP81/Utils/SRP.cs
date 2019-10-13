// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Telegram.Api.TL;

namespace TelegramClient.Utils
{
    static class SRP
    {
        /// to big-endian data
        public static byte[] GetBigIntegerBytes(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            if (bytes.Length > 256)
            {
                byte[] correctedAuth = new byte[256];
                System.Array.Copy(bytes, 1, correctedAuth, 0, 256);
                return correctedAuth;
            }
            if (bytes.Length < 256)
            {
                byte[] correctedAuth = new byte[256];
                System.Array.Copy(bytes, 0, correctedAuth, 256 - bytes.Length, bytes.Length);
                for (int a = 0; a < 256 - bytes.Length; a++)
                {
                    correctedAuth[a] = 0;
                }
                return correctedAuth;
            }
            return bytes;
        }

        public static TLString GetX(TLString password, TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow algo)
        {
            var x_bytes = Telegram.Api.Helpers.Utils.ComputeSHA256(TLUtils.Combine(algo.Salt1.Data, password.Data, algo.Salt1.Data));
            x_bytes = Telegram.Api.Helpers.Utils.ComputeSHA256(TLUtils.Combine(algo.Salt2.Data, x_bytes, algo.Salt2.Data));
            x_bytes = PBKDF2.GetHash(x_bytes.AsBuffer(), algo.Salt1.Data.AsBuffer()).ToArray();
            x_bytes = Telegram.Api.Helpers.Utils.ComputeSHA256(TLUtils.Combine(algo.Salt2.Data, x_bytes, algo.Salt2.Data));

            return TLString.FromBigEndianData(x_bytes);
        }

        private static BigInteger GetV(TLString passwordBytes, TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow algo)
        {
            var g = BigInteger.ValueOf(algo.G.Value);
            var p = new BigInteger(1, algo.P.Data);

            var x_bytes = GetX(passwordBytes, algo);
            var x = new BigInteger(1, x_bytes.Data);

            return g.ModPow(x, p);
        }

        public static TLString GetVBytes(TLString passwordBytes, TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow algo)
        {
            if (!TLUtils.CheckPrime(algo.P.Data, algo.G.Value))
            {
                return null;
            }

            return TLString.FromBigEndianData(GetBigIntegerBytes(GetV(passwordBytes, algo)));
        }

        public static TLInputCheckPasswordBase GetCheck(TLString xStr, TLLong srpId, TLString srpB, TLPasswordKdfAlgoBase algoBase)
        {
            var algo = algoBase as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (algo != null)
            {
                if (xStr == null || srpB == null || srpB.Data.Length == 0 || !TLUtils.CheckPrime(algo.P.Data, algo.G.Value))
                {
                    return new TLInputCheckPasswordEmpty();
                }

                var g = new BigInteger(1, algo.G.ToBytes().Reverse().ToArray()); // from big-endian to BI (ToBytes - little endian output)
                var g_bytes = GetBigIntegerBytes(g);
                
                var p = new BigInteger(1, algo.P.Data);
                var p_bytes = GetBigIntegerBytes(p);

                var k_bytes = Telegram.Api.Helpers.Utils.ComputeSHA256(TLUtils.Combine(p_bytes, g_bytes));
                var k = new BigInteger(1, k_bytes);

                var x = new BigInteger(1, xStr.Data);

                var a_bytes = new byte[256];
                var secureRandom = new SecureRandom();
                secureRandom.NextBytes(a_bytes);
                var a = new BigInteger(1, a_bytes);

                var A = g.ModPow(a, p);
                var A_bytes = GetBigIntegerBytes(A);

                var B = new BigInteger(1, srpB.Data);
                if (B.CompareTo(BigInteger.Zero) <= 0 || B.CompareTo(p) >= 0)
                {
                    return null;
                }
                var B_bytes = GetBigIntegerBytes(B);

                var u_bytes = Telegram.Api.Helpers.Utils.ComputeSHA256(TLUtils.Combine(A_bytes, B_bytes));
                var u = new BigInteger(1, u_bytes);
                if (u.CompareTo(BigInteger.Zero) == 0)
                {
                    return null;
                }

                var B_kgx = B.Subtract(k.Multiply(g.ModPow(x, p)).Mod(p));
                if (B_kgx.CompareTo(BigInteger.Zero) < 0)
                {
                    B_kgx = B_kgx.Add(p);
                }
                if (!TLUtils.CheckGaAndGb(B_kgx, p))
                {
                    return null;
                }
                var S = B_kgx.ModPow(a.Add(u.Multiply(x)), p);
                var S_bytes = GetBigIntegerBytes(S);

                var K_bytes = Telegram.Api.Helpers.Utils.ComputeSHA256(S_bytes);

                var p_hash = Telegram.Api.Helpers.Utils.ComputeSHA256(algo.P.Data);
                var g_hash = Telegram.Api.Helpers.Utils.ComputeSHA256(g_bytes);
                for (var i = 0; i < p_hash.Length; i++)
                {
                    p_hash[i] = (byte)(g_hash[i] ^ p_hash[i]);
                }

                var M1 = Telegram.Api.Helpers.Utils.ComputeSHA256(TLUtils.Combine(
                    p_hash, 
                    Telegram.Api.Helpers.Utils.ComputeSHA256(algo.Salt1.Data), 
                    Telegram.Api.Helpers.Utils.ComputeSHA256(algo.Salt2.Data), 
                    A_bytes, 
                    B_bytes, 
                    K_bytes));

                return new TLInputCheckPasswordSRP
                {
                    SRPId = srpId,
                    A = TLString.FromBigEndianData(A_bytes),
                    M1 = TLString.FromBigEndianData(M1)
                };
            }

            return new TLInputCheckPasswordEmpty();
        }
    }
}
