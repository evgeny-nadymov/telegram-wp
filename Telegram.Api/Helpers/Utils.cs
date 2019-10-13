// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Org.BouncyCastle.OpenSsl;
#if WINDOWS_PHONE
using System.Threading;
using System.Security.Cryptography;
using System.Windows;
#elif WIN_RT
using Windows.Security.Cryptography;
using System.Runtime.InteropServices.WindowsRuntime;
#endif
using System.Text;
using Windows.Security.Cryptography.Core;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Telegram.Api.TL;
using Buffer = System.Buffer;

namespace Telegram.Api.Helpers
{
    public class PollardRhoLong
    {
        public static long Gcd(long ths, long val)
        {
            if (val == 0)
                return Math.Abs(ths);
            if (ths == 0)
                return Math.Abs(val);

            long r;
            long u = ths;
            long v = val;

            while (v != 0)
            {
                r = u % v;
                u = v;
                v = r;
            }

            return u;
        }

        public static long Rho(long N)
        {
            var random = new Random();

            long divisor;
            var bytes = new byte[8];
            random.NextBytes(bytes);
            var c = BitConverter.ToInt64(bytes, 0);
            random.NextBytes(bytes);
            var x = BitConverter.ToInt64(bytes, 0);
            var xx = x;

            // check divisibility by 2
            if (N % 2 == 0) return 2;

            do
            {
                x = (x * x % N + c) % N;
                xx = (xx * xx % N + c) % N;
                xx = (xx * xx % N + c) % N;
                divisor = Gcd(x - xx, N);
            } while (divisor == 1);

            return divisor;
        }
    }

    public class PollardRho
    {
        private static readonly BigInteger ZERO = new BigInteger("0");
        private static readonly BigInteger ONE = new BigInteger("1");
        private static readonly BigInteger TWO = new BigInteger("2");
        private static readonly SecureRandom random = new SecureRandom();

        public static BigInteger Rho(BigInteger N)
        {
            BigInteger divisor;
            var c = new BigInteger(N.BitLength, random);
            var x = new BigInteger(N.BitLength, random);
            var xx = x;

            // check divisibility by 2
            if (N.Mod(TWO).CompareTo(ZERO) == 0) return TWO;

            do
            {
                x = x.Multiply(x).Mod(N).Add(c).Mod(N);
                xx = xx.Multiply(xx).Mod(N).Add(c).Mod(N);
                xx = xx.Multiply(xx).Mod(N).Add(c).Mod(N);
                divisor = x.Subtract(xx).Gcd(N);
            } while ((divisor.CompareTo(ONE)) == 0);

            return divisor;
        }

        public static WindowsPhone.Tuple<BigInteger, BigInteger> Factor(BigInteger N)
        {
            var divisor = Rho(N);

            var divisor2 = N.Divide(divisor);

            return divisor.CompareTo(divisor2) > 0
                ? new WindowsPhone.Tuple<BigInteger, BigInteger>(divisor2, divisor)
                : new WindowsPhone.Tuple<BigInteger, BigInteger>(divisor, divisor2);
        }
    }

    public static class Utils
    {
#if !WIN_RT
        public static bool XapContentFileExists(string relativePath)
        {
            return Application.GetResourceStream(new Uri(relativePath, UriKind.Relative)) != null;
        }
#endif

        public static long GetRSAFingerprint(string key)
        {
            using (var text = new StringReader(key))
            {
                var reader = new PemReader(text);
                var parameter = reader.ReadObject() as RsaKeyParameters;
                if (parameter != null)
                {
                    var modulus = parameter.Modulus.ToByteArray();
                    var exponent = parameter.Exponent.ToByteArray();

                    if (modulus.Length > 256)
                    {
                        var corrected = new byte[256];
                        Buffer.BlockCopy(modulus, modulus.Length - 256, corrected, 0, 256);

                        modulus = corrected;
                    }
                    else if (modulus.Length < 256)
                    {
                        var corrected = new byte[256];
                        Buffer.BlockCopy(modulus, 0, corrected, 256 - modulus.Length, modulus.Length);

                        for (int a = 0; a < 256 - modulus.Length; a++)
                        {
                            modulus[a] = 0;
                        }

                        modulus = corrected;
                    }

                    using (var stream = new MemoryStream())
                    {
                        var modulusString = TLString.FromBigEndianData(modulus);
                        var exponentString = TLString.FromBigEndianData(exponent);

                        modulusString.ToStream(stream);
                        exponentString.ToStream(stream);

                        var hash = ComputeSHA1(stream.ToArray());

                        var fingerprint = (((ulong)hash[19]) << 56) |
                                          (((ulong)hash[18]) << 48) |
                                          (((ulong)hash[17]) << 40) |
                                          (((ulong)hash[16]) << 32) |
                                          (((ulong)hash[15]) << 24) |
                                          (((ulong)hash[14]) << 16) |
                                          (((ulong)hash[13]) << 8) |
                                          ((ulong)hash[12]);

                        return (long)fingerprint;
                    }
                }
            }

            return -1;
        }

        public static byte[] GetRSABytes(byte[] bytes, string key)
        {
            using (var text = new StringReader(key))
            {
                var reader = new PemReader(text);
                var parameter = reader.ReadObject() as RsaKeyParameters;
                if (parameter != null)
                {
                    var modulus = parameter.Modulus;
                    var exponent = parameter.Exponent;

                    var num = new BigInteger(TLUtils.Combine(new byte[] { 0x00 }, bytes));
                    var rsa = num.ModPow(exponent, modulus).ToByteArray();

#if LOG_REGISTRATION
                    TLUtils.WriteLog("RSA bytes length " + rsa.Length);
#endif
                    if (rsa.Length == 257)
                    {
                        if (rsa[0] != 0x00) throw new Exception("rsa last byte is " + rsa[0]);

#if LOG_REGISTRATION
                        TLUtils.WriteLog("First RSA byte removes: byte value is " + rsa[0]);
#endif

                        rsa = rsa.SubArray(1, 256);
                    }
                    else if (rsa.Length < 256)
                    {
                        var correctedRsa = new byte[256];
                        Array.Copy(rsa, 0, correctedRsa, 256 - rsa.Length, rsa.Length);
                        for (var i = 0; i < 256 - rsa.Length; i++)
                        {
                            correctedRsa[i] = 0;
#if LOG_REGISTRATION
                            TLUtils.WriteLog("First RSA bytes added i=" + i + " " + correctedRsa[i]);
#endif
                        }
                        rsa = correctedRsa;
                    }

                    return rsa;
                }
            }

            return null;
        }

        private static UInt64 GetP(UInt64 data)
        {
            var sqrt = (UInt64)Math.Sqrt(data);
            if (sqrt % 2 == 0) sqrt++;


            for (UInt64 i = sqrt; i >= 1; i = i - 2)
            {
                if (data % i == 0) return i;
            }

            return data;
        }

        public static WindowsPhone.Tuple<UInt64, UInt64> GetPQ(UInt64 pq)
        {
            var p = GetP(pq);
            var q = pq / p;

            if (p > q)
            {
                var temp = p;
                p = q;
                q = temp;
            }

            return new WindowsPhone.Tuple<UInt64, UInt64>(p, q);
        }

        public static WindowsPhone.Tuple<UInt64, UInt64> GetPQPollard(UInt64 pq)
        {
            var n = new BigInteger(BitConverter.GetBytes(pq).Reverse().ToArray());
            var result = PollardRho.Factor(n);
            return new WindowsPhone.Tuple<UInt64, UInt64>((UInt64)result.Item1.LongValue, (UInt64)result.Item2.LongValue);
        }

        public static WindowsPhone.Tuple<UInt64, UInt64> GetFastPQ(UInt64 pq)
        {
            var first = FastFactor((long)pq);
            var second = (long)pq / first;

            return first < second ?
                new WindowsPhone.Tuple<UInt64, UInt64>((UInt64)first, (UInt64)second) :
                new WindowsPhone.Tuple<UInt64, UInt64>((UInt64)second, (UInt64)first);
        }

        public static long GCD(long a, long b)
        {
            while (a != 0 && b != 0)
            {
                while ((b & 1) == 0)
                {
                    b >>= 1;
                }
                while ((a & 1) == 0)
                {
                    a >>= 1;
                }
                if (a > b)
                {
                    a -= b;
                }
                else
                {
                    b -= a;
                }
            }
            return b == 0 ? a : b;
        }

        public static long FastFactor(long what)
        {
            Random r = new Random();
            long g = 0;
            int it = 0;
            for (int i = 0; i < 3; i++)
            {
                int q = (r.Next(128) & 15) + 17;
                long x = r.Next(1000000000) + 1, y = x;
                int lim = 1 << (i + 18);
                for (int j = 1; j < lim; j++)
                {
                    ++it;
                    long a = x, b = x, c = q;
                    while (b != 0)
                    {
                        if ((b & 1) != 0)
                        {
                            c += a;
                            if (c >= what)
                            {
                                c -= what;
                            }
                        }
                        a += a;
                        if (a >= what)
                        {
                            a -= what;
                        }
                        b >>= 1;
                    }
                    x = c;
                    long z = x < y ? y - x : x - y;
                    g = GCD(z, what);
                    if (g != 1)
                    {
                        break;
                    }
                    if ((j & (j - 1)) == 0)
                    {
                        y = x;
                    }
                }
                if (g > 1)
                {
                    break;
                }
            }

            long p = what / g;
            return Math.Min(p, g);
        }




        private static byte[] XorArrays(byte[] first, byte[] second)
        {
            var bytes = new byte[16];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(first[i] ^ second[i]);
            }

            return bytes;
        }
#if WINDOWS_PHONE || WIN_RT
        public static Stream AesIge(Stream data, byte[] key, byte[] iv, bool encrypt)
        {
            var cipher = CipherUtilities.GetCipher("AES/ECB/NOPADDING");
            var param = new KeyParameter(key);
            cipher.Init(encrypt, param);

            var inData = data;
            var outStream = new MemoryStream();
            var position = 0;

            byte[] xOld = new byte[16], yOld = new byte[16], x = new byte[16];

            Array.Copy(iv, 0, encrypt ? yOld : xOld, 0, 16);
            Array.Copy(iv, 16, encrypt ? xOld : yOld, 0, 16);

            while (position < inData.Length)
            {
                long length;
                if ((position + 16) < inData.Length)
                {
                    length = 16;
                }
                else
                {
                    length = inData.Length - position;
                }

                inData.Read(x, 0, (int)length);
                //Array.Copy(inData, position, x, 0, length);


                var processedBytes = cipher.ProcessBytes(XorArrays(x, yOld));
                byte[] y = XorArrays(processedBytes, xOld);

                xOld = (byte[])x.Clone();
                //xOld = new byte[x.Length];
                //Array.Copy(x, xOld, x.Length);
                yOld = y;

                outStream.Write(y, 0, y.Length);
                //outData = TLUtils.Combine(outData, y);

                position += 16;
            }
            return outStream;
            //return outData;
        }

        public static byte[] AesIge(byte[] data, byte[] key, byte[] iv, bool encrypt)
        {
            byte[] nextIV;
            return AesIge(data, key, iv, encrypt, out nextIV);
        }

#if WIN_RT
        public static byte[] AesIgeWinRT(byte[] data, byte[] key, byte[] iv, bool encrypt)
        {
            byte[] nextIV;
            return AesIge2(data, key, iv, encrypt, out nextIV);
        }

        public static byte[] AesIge2(byte[] data, byte[] key, byte[] iv, bool encrypt, out byte[] nextIV)
        {
            var cipher = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);
            var keyMaterial = CryptographicBuffer.CreateFromByteArray(key);
            var param = cipher.CreateSymmetricKey(keyMaterial);

            var inData = data;
            //var outData = new byte[]{};
            var outStream = new MemoryStream();
            var position = 0;

            byte[] xOld = new byte[16], yOld = new byte[16], x = new byte[16], y = new byte[16];

            Array.Copy(iv, 0, encrypt ? yOld : xOld, 0, 16);
            Array.Copy(iv, 16, encrypt ? xOld : yOld, 0, 16);

            while (position < inData.Length)
            {
                int length;
                if ((position + 16) < inData.Length)
                {
                    length = 16;
                }
                else
                {
                    length = inData.Length - position;
                }

                Array.Copy(inData, position, x, 0, length);

                y = XorArrays(x, yOld);
                var processedBytes = encrypt ? CryptographicEngine.Encrypt(param, CryptographicBuffer.CreateFromByteArray(y), null) : CryptographicEngine.Decrypt(param, CryptographicBuffer.CreateFromByteArray(y), null);
                y = XorArrays(processedBytes.ToArray(), xOld);

                xOld = (byte[])x.Clone();
                //xOld = new byte[x.Length];
                //Array.Copy(x, xOld, x.Length);
                yOld = y;

                outStream.Write(y, 0, y.Length);
                //outData = TLUtils.Combine(outData, y);

                position += 16;
            }

            nextIV = encrypt ? TLUtils.Combine(yOld, xOld) : TLUtils.Combine(xOld, yOld);

            return outStream.ToArray();
            //return outData;
        }
#endif

        public static byte[] AesIge(byte[] data, byte[] key, byte[] iv, bool encrypt, out byte[] nextIV)
        {
            var cipher = CipherUtilities.GetCipher("AES/ECB/NOPADDING");
            var param = new KeyParameter(key);
            cipher.Init(encrypt, param);

            var inData = data;
            //var outData = new byte[]{};
            var outStream = new MemoryStream();
            var position = 0;

            byte[] xOld = new byte[16], yOld = new byte[16], x = new byte[16];

            Array.Copy(iv, 0, encrypt ? yOld : xOld, 0, 16);
            Array.Copy(iv, 16, encrypt ? xOld : yOld, 0, 16);

            while (position < inData.Length)
            {
                int length;
                if ((position + 16) < inData.Length)
                {
                    length = 16;
                }
                else
                {
                    length = inData.Length - position;
                }

                Array.Copy(inData, position, x, 0, length);


                var processedBytes = cipher.ProcessBytes(XorArrays(x, yOld));
                byte[] y = XorArrays(processedBytes, xOld);

                xOld = (byte[])x.Clone();
                //xOld = new byte[x.Length];
                //Array.Copy(x, xOld, x.Length);
                yOld = y;

                outStream.Write(y, 0, y.Length);
                //outData = TLUtils.Combine(outData, y);

                position += 16;
            }

            nextIV = encrypt ? TLUtils.Combine(yOld, xOld) : TLUtils.Combine(xOld, yOld);

            return outStream.ToArray();
            //return outData;
        }

        private static System.Numerics.BigInteger MaxIvec = new System.Numerics.BigInteger(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });

        // Note: ivec - big-endian, but BigInterger.ctor and BigInteger.ToByteArray return little-endian
        public static byte[] AES_ctr128_encrypt(byte[] input, byte[] key, ref byte[] ivec, ref byte[] ecount_buf, ref uint num)
        {
            uint n;
            var output = new byte[input.Length];
            n = num;

            var cipher = CipherUtilities.GetCipher("AES/ECB/NOPADDING");
            var param = new KeyParameter(key);
            cipher.Init(true, param);

            for (uint i = 0; i < input.Length; i++)
            {
                if (n == 0)
                {
                    ecount_buf = cipher.DoFinal(ivec);
                    Array.Reverse(ivec);
                    var bi = new System.Numerics.BigInteger(TLUtils.Combine(ivec, new byte[] { 0x00 }));
                    bi = (bi + 1);
                    var biArray = bi.ToByteArray();
                    var b = new byte[16];
                    Buffer.BlockCopy(biArray, 0, b, 0, Math.Min(b.Length, biArray.Length));

                    //System.Diagnostics.Debug.WriteLine(bi);
                    Array.Reverse(b);
                    ivec = b;
                }

                output[i] = (byte)(input[i] ^ ecount_buf[n]);
                n = (n + 1) % 16;
            }

            num = n;
            return output;
        }

        public static byte[] AES_ctr128_encrypt2(byte[] input, byte[] key, ref byte[] ivec, ref byte[] ecount_buf, ref uint num)
        {
            uint n;
            var output = new byte[input.Length];
            n = num;

            var cipher = CipherUtilities.GetCipher("AES/ECB/NOPADDING");
            var param = new KeyParameter(key);
            cipher.Init(true, param);

            for (uint i = 0; i < input.Length; i++)
            {
                if (n == 0)
                {
                    ecount_buf = cipher.DoFinal(ivec);
                    Array.Reverse(ivec);
                    var bi = new System.Numerics.BigInteger(TLUtils.Combine(ivec, new byte[] { 0x00 }));
                    bi = (bi + 1);
                    var biArray = bi.ToByteArray();
                    var b = new byte[16];
                    Buffer.BlockCopy(biArray, 0, b, 0, Math.Min(b.Length, biArray.Length));

                    //System.Diagnostics.Debug.WriteLine(bi);
                    Array.Reverse(b);
                    ivec = b;
                }

                output[i] = (byte)(input[i] ^ ecount_buf[n]);
                n = (n + 1) % 16;
            }

            num = n;
            return output;
        }

        //public static byte[] AesCtr(byte[] data, byte[] key, byte[] iv, bool encrypt)
        //{
        //    var cipher = CipherUtilities.GetCipher("AES/CTR/NOPADDING");
        //    var keyIv = new ParametersWithIV(new KeyParameter(key), iv);
        //    cipher.Init(encrypt, keyIv);
        //    //cipher.Init(encrypt, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

        //    var outData = cipher.DoFinal(data);
        //    if (outData.Length != data.Length)
        //    {
        //        Execute.ShowDebugMessage(string.Format("Utils.AesCtr outData.Length!=data.Length outData={0} data={1}", outData.Length, data.Length));
        //    }

        //    return outData;
        //}
#else
        public static byte[] AesIge(byte[] data, byte[] key, byte[] iv, bool encrypt)
        {
            throw new NotImplementedException();
        }

        public static byte[] AesCtr(byte[] data, byte[] key, byte[] iv, bool encrypt)
        {
            var cipher = CipherUtilities.GetCipher("AES/CTR/NOPADDING");
            cipher.Init(encrypt, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            var outData = cipher.DoFinal(data);
            if (outData.Length != data.Length)
            {
                Execute.ShowDebugMessage(string.Format("Utils.AesCtr outData.Length!=data.Length outData={0} data={1}", outData.Length, data.Length));
            }

            return outData;
        }
#endif

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length / 2;
            byte[] bytes = new byte[NumberChars];
            StringReader sr = new StringReader(hex);
            for (int i = 0; i < NumberChars; i++)
                bytes[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            sr.Dispose();
            return bytes;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            if (index == 0 && length == data.Length)
            {
                return data;
            }

            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            // From local DateTime to UTC0 UnixTime

            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime.SpecifyKind(dtDateTime, DateTimeKind.Utc);

            return (dateTime.ToUniversalTime() - dtDateTime).TotalSeconds;
        }

        public static DateTime UnixTimestampToDateTime(double unixTimeStamp)
        {
            // From UTC0 UnixTime to local DateTime

            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime.SpecifyKind(dtDateTime, DateTimeKind.Utc);

            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static byte[] ComputeSHA1(byte[] data)
        {
#if WINDOWS_PHONE
            //var sha1 = new SHA1Managed(); // to avoid thread sync problems http://stackoverflow.com/questions/12644257/sha1managed-computehash-occasionally-different-on-different-servers
            //return sha1.ComputeHash(data);
            var sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            return sha1.HashData(data.AsBuffer()).ToArray();
#elif WIN_RT
            var sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            return sha1.HashData(data.AsBuffer()).ToArray();
#endif
        }

        public static byte[] ComputeSHA256(byte[] data)
        {
#if WINDOWS_PHONE
            //var sha256 = new SHA256Managed(); // to avoid thread sync problems http://stackoverflow.com/questions/12644257/sha1managed-computehash-occasionally-different-on-different-servers
            //return sha256.ComputeHash(data);
            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            return sha256.HashData(data.AsBuffer()).ToArray();
#elif WIN_RT
            var sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            return sha1.HashData(data.AsBuffer()).ToArray();
#endif
        }

        public static byte[] ComputeMD5(byte[] data)
        {
#if WINDOWS_PHONE
            //var md5 = new MD5Managed();
            //return md5.ComputeHash(data);
            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            return md5.HashData(data.AsBuffer()).ToArray();
#elif WIN_RT
            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            return md5.HashData(data.AsBuffer()).ToArray();
#endif
        }

        public static byte[] ComputeCRC32(string data)
        {
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(data);
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

            return ComputeCRC32(utf8Bytes);
        }

        public static byte[] ComputeCRC32(byte[] data)
        {
            var crc = new CRC32();
            var hash = crc.ComputeHash(data);

            return hash;
        }

        public static string CurrentUICulture()
        {
#if WIN_RT
            return Windows.Globalization.Language.CurrentInputMethodLanguageTag;
#elif WINDOWS_PHONE
            return Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
#endif
        }
    }
}
