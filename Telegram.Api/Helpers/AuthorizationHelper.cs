using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Telegram.Api.TL;
using Telegram.Api.Transport;

namespace Telegram.Api.Helpers
{
    /*public class AuthorizationHelper : IAuthorizationHelper
    {
        public static byte[] AuthKey { get; set; }
        public static TLLong Salt { get; set; }
        public static TLLong SessionId { get; set; }

        private readonly ITransport _transport;

        public AuthorizationHelper(ITransport transport)
        {
            _transport = transport;
        }

        public void InitAsync(Action<Tuple<byte[], byte[], byte[]>> callback, Action<TLRPCError> faultCallback = null)
        {
            var authTime = Stopwatch.StartNew();
            // 1 stage
            var authRequest = ComposeBeginAuthRequest();
            var message = CreatePlainMessageBody(authRequest);
            var guid = 1;
            _transport.SendBytesAsync("resPQ " + guid, message,
            x1 =>
            {
                var buffer = x1;
                // 2 stage
                var authResponse = AuthResponse.Parse(buffer);

                // 3 stage
                TLUtils.WriteLine("pq: " + authResponse.pq);
                var pqCalcTime = Stopwatch.StartNew();
                var tuple = Utils.GetPQPollard(authResponse.pq);
                pqCalcTime.Stop();
                TLUtils.WriteLineAtBegin("pqCalc time: " + pqCalcTime.Elapsed);
                var p = tuple.Item1;
                var q = tuple.Item2;
                TLUtils.WriteLine("p: " + tuple.Item1);
                var pStr = TLString.FromUInt64(tuple.Item1);
                //TLUtils.WriteLine("p bytes: " + BitConverter.ToString(pStr.ToBytes(8)));

                var qStr = TLString.FromUInt64(tuple.Item2);
                TLUtils.WriteLine("q: " + tuple.Item2);
                //TLUtils.WriteLine("q bytes: " + BitConverter.ToString(qStr.ToBytes(8)));






                // 4 stage
                var random1 = new Random();
                var newNonce = new byte[32];
                random1.NextBytes(newNonce);

                var data = ComposeData(authResponse, p, q, newNonce); //newNonce 32
                //TLUtils.WriteLine("-----------------------------------------");
                //TLUtils.WriteLine(string.Format("data [{1}]: {0}", BitConverter.ToString(data), data.Length));
                //TLUtils.WriteLine("-----------------------------------------");



                SHA1 sha = new SHA1Managed();
                var sha1 = sha.ComputeHash(data); // data 96
                //TLUtils.WriteLine("-----------------------------------------");
                //TLUtils.WriteLine(string.Format("SHA1 data [{1}]: {0}", BitConverter.ToString(sha1), sha1.Length));
                //TLUtils.WriteLine("-----------------------------------------");

                var dataWithHash = sha1.Concat(data).ToArray(); //116
                var data255 = new byte[255];
                var random = new Random();
                random.NextBytes(data255);
                Array.Copy(dataWithHash, data255, dataWithHash.Length);
                //TLUtils.WriteLine("-----------------------------------------");
                //TLUtils.WriteLine(string.Format("data with hash [{1}]: {0}", BitConverter.ToString(data255), data255.Length));
                //TLUtils.WriteLine("-----------------------------------------");

                var rsa = GetRSABytes(data255); //data255 255 bytes

                var dhRequest = ComposeBeginDHRequest(authResponse, p, q, rsa); //rsa 256 bytes
                var dhMessage = CreatePlainMessageBody(dhRequest); //dhRequest 320 bytes
                guid = 2;
                _transport.SendBytesAsync("req_DH_params " + guid, dhMessage,
                    dhResponseBuffer =>
                    {
                        var dhResponse = BeginDHResponse.Parse(dhResponseBuffer);

                        var aesParams = GetAesKeyIV(authResponse.ServerNonce, newNonce);

                        var decryptedAnswerWithHash = Utils.AesIge(dhResponse.EncryptedAnswer, aesParams.Item1, aesParams.Item2, false);
                        TLUtils.WriteLine("---Decrypted answer with hash----------------");
                        TLUtils.WriteLine(BitConverter.ToString(decryptedAnswerWithHash));
                        //var encryptedAnswerWithHash = Utils.AesIge(dhResponse.EncryptedAnswer, aesParams.Item1, aesParams.Item2, true);

                        var answer = Answer.Parse(decryptedAnswerWithHash.Skip(20).ToArray());
                        var bBytes = new byte[256]; //big endian B
                        random.NextBytes(bBytes);
                        //TLUtils.WriteLine("B bytes: " + BitConverter.ToString(bBytes));

                        var g_bBytes = GetG_B(bBytes, answer.G, answer.DHPrime); // big-endian g_b

                        //TLUtils.WriteLine("--G_B big endian bytes----------------------");
                        //TLUtils.WriteLine(BitConverter.ToString(g_bBytes));

                        var client_DH_inner_data = ComposeClientDHInnerData(g_bBytes, authResponse);

                        var client_DH_inner_dataWithHash = sha.ComputeHash(client_DH_inner_data).Concat(client_DH_inner_data).ToArray();
                        var addedBytesLength = 16 - (client_DH_inner_dataWithHash.Length % 16);
                        if (addedBytesLength > 0 && addedBytesLength < 16)
                        {
                            var addedBytes = new byte[addedBytesLength];
                            random.NextBytes(addedBytes);
                            client_DH_inner_dataWithHash = client_DH_inner_dataWithHash.Concat(addedBytes).ToArray();
                            //TLUtils.WriteLine(string.Format("Added {0} bytes", addedBytesLength));
                        }

                        var aesEncryptClientDHInnerDataWithHash = Utils.AesIge(client_DH_inner_dataWithHash, aesParams.Item1, aesParams.Item2, true);
                        //TLUtils.WriteLine("--Last encrypted data------------------");
                        //TLUtils.WriteLine(BitConverter.ToString(aesEncryptClientDHInnerDataWithHash));


                        var requestSetClientDHParams = ComposeRequestSetClientDHParams(authResponse, aesEncryptClientDHInnerDataWithHash);
                        var requestSetClientDHParamsMessage = CreatePlainMessageBody(requestSetClientDHParams);
                        guid = 3;
                        _transport.SendBytesAsync("set_client_DH_params " + guid, requestSetClientDHParamsMessage,
                            x2 =>
                            {
                                authTime.Stop();
                                TLUtils.WriteLineAtBegin("pqCalc time: " + pqCalcTime.Elapsed);
                                TLUtils.WriteLineAtBegin("Auth time: " + authTime.Elapsed);
                                TLUtils.WriteLineAtBegin("Auth - pqCalc time: " + (authTime.Elapsed - pqCalcTime.Elapsed)); 
                                buffer = x2;
                                var endAuthResponse = EndAuthResponse.Parse(buffer);

                                var authKey = GetAuthKey(bBytes, answer.G_A, answer.DHPrime);
                                TLUtils.WriteLine("-Big endian auth key----------------------------------");
                                TLUtils.WriteLine(BitConverter.ToString(authKey));
                                TLUtils.WriteLine("-Big endian auth key----------------------------------");

#if !SILVERLIGHT
                                using (StreamWriter w = File.AppendText("log.txt"))
                                {
                                    w.WriteLine(DateTime.Now);
                                    w.WriteLine(BitConverter.ToString(authKey));
                                }
#endif
                                //newNonce - little endian
                                //authResponse.ServerNonce - little endian
                                var salt = GetSalt(newNonce, authResponse.ServerNonce);
                                var sessionId = new byte[8];
                                random.NextBytes(sessionId);

                                AuthKey = authKey;
                                Salt = new TLLong(BitConverter.ToInt64(salt, 0));
                                SessionId = new TLLong(BitConverter.ToInt64(sessionId, 0));
                                TLUtils.WriteLine("Salt " + Salt + " (" + BitConverter.ToString(salt) + ")");
                                TLUtils.WriteLine("Session id " +SessionId + " (" + BitConverter.ToString(sessionId) + ")");

                                callback(new Tuple<byte[], byte[], byte[]>(authKey, salt, sessionId));
                            },
                            () => { if (faultCallback != null) faultCallback(null); });
                    },
                    () => { if (faultCallback != null) faultCallback(null); }); // dhMessage340bytes 404 here
            },
            () => { if (faultCallback != null) faultCallback(null); });
        }

        public Tuple<byte[], byte[], byte[]> Init()
        {
            // 1 stage
            var authRequest = ComposeBeginAuthRequest();
            var message = CreatePlainMessageBody(authRequest);
            var buffer = _transport.SendBytes(message);

            // 2 stage
            var authResponse = AuthResponse.Parse(buffer);

            // 3 stage
            TLUtils.WriteLine("pq: ", authResponse.pq);
            var time = Stopwatch.StartNew();
            var tuple = Utils.GetPQ(authResponse.pq);
            var p = tuple.Item1;
            var q = tuple.Item2;
            TLUtils.WriteLine("p: " + tuple.Item1);
            var pStr = TLString.FromUInt64(tuple.Item1);
            //TLUtils.WriteLine("p bytes: " + BitConverter.ToString(pStr.ToBytes(8)));

            var qStr = TLString.FromUInt64(tuple.Item2);
            TLUtils.WriteLine("q: " + tuple.Item2);
            //TLUtils.WriteLine("q bytes: " + BitConverter.ToString(qStr.ToBytes(8)));
            TLUtils.WriteLine("Calculation time: " + time.ElapsedMilliseconds);

            // 4 stage
            var random1 = new Random();
            var newNonce = new byte[32];
            random1.NextBytes(newNonce);

            var data = ComposeData(authResponse, p, q, newNonce); //newNonce 32
            //TLUtils.WriteLine("-----------------------------------------");
            //TLUtils.WriteLine(string.Format("data [{1}]: {0}", BitConverter.ToString(data), data.Length));
            //TLUtils.WriteLine("-----------------------------------------");



            SHA1 sha = new SHA1Managed();
            var sha1 = sha.ComputeHash(data); // data 96
            //TLUtils.WriteLine("-----------------------------------------");
            //TLUtils.WriteLine(string.Format("SHA1 data [{1}]: {0}", BitConverter.ToString(sha1), sha1.Length));
            //TLUtils.WriteLine("-----------------------------------------");

            var dataWithHash = sha1.Concat(data).ToArray(); //116
            var data255 = new byte[255];
            var random = new Random();
            random.NextBytes(data255);
            Array.Copy(dataWithHash, data255, dataWithHash.Length);
            //TLUtils.WriteLine("-----------------------------------------");
            //TLUtils.WriteLine(string.Format("data with hash [{1}]: {0}", BitConverter.ToString(data255), data255.Length));
            //TLUtils.WriteLine("-----------------------------------------");

            var rsa = GetRSABytes(data255); //data255 255 bytes

            var dhRequest = ComposeBeginDHRequest(authResponse, p, q, rsa); //rsa 256 bytes
            var dhMessage = CreatePlainMessageBody(dhRequest); //dhRequest 320 bytes
            var stamp = Stopwatch.StartNew();
            var dhResponseBuffer = _transport.SendBytes(dhMessage); // dhMessage340bytes 404 here

            var dhResponse = BeginDHResponse.Parse(dhResponseBuffer);

            var aesParams = GetAesKeyIV(authResponse.ServerNonce, newNonce);

            var decryptedAnswerWithHash = Utils.AesIge(dhResponse.EncryptedAnswer, aesParams.Item1, aesParams.Item2, false);
            //TLUtils.WriteLine("---Decrypted answer with hash----------------");
            //TLUtils.WriteLine(BitConverter.ToString(decryptedAnswerWithHash));
            //var encryptedAnswerWithHash = Utils.AesIge(dhResponse.EncryptedAnswer, aesParams.Item1, aesParams.Item2, true);

            var answer = Answer.Parse(decryptedAnswerWithHash.Skip(20).ToArray());
            var bBytes = new byte[256]; //big endian B
            random.NextBytes(bBytes);
            //TLUtils.WriteLine("B bytes: " + BitConverter.ToString(bBytes));

            var g_bBytes = GetG_B(bBytes, answer.G, answer.DHPrime); // big-endian g_b

            //TLUtils.WriteLine("--G_B big endian bytes----------------------");
            //TLUtils.WriteLine(BitConverter.ToString(g_bBytes));

            var client_DH_inner_data = ComposeClientDHInnerData(g_bBytes, authResponse);

            var client_DH_inner_dataWithHash = sha.ComputeHash(client_DH_inner_data).Concat(client_DH_inner_data).ToArray();
            var addedBytesLength = 16 - (client_DH_inner_dataWithHash.Length % 16);
            if (addedBytesLength > 0 && addedBytesLength < 16)
            {
                var addedBytes = new byte[addedBytesLength];
                random.NextBytes(addedBytes);
                client_DH_inner_dataWithHash = client_DH_inner_dataWithHash.Concat(addedBytes).ToArray();
                //TLUtils.WriteLine(string.Format("Added {0} bytes", addedBytesLength));
            }

            var aesEncryptClientDHInnerDataWithHash = Utils.AesIge(client_DH_inner_dataWithHash, aesParams.Item1, aesParams.Item2, true);
            //TLUtils.WriteLine("--Last encrypted data------------------");
            //TLUtils.WriteLine(BitConverter.ToString(aesEncryptClientDHInnerDataWithHash));


            var requestSetClientDHParams = ComposeRequestSetClientDHParams(authResponse, aesEncryptClientDHInnerDataWithHash);
            var requestSetClientDHParamsMessage = CreatePlainMessageBody(requestSetClientDHParams);
            buffer = _transport.SendBytes(requestSetClientDHParamsMessage);
            //TLUtils.WriteLine("--RESPONSE--------------");
            //TLUtils.WriteLine(BitConverter.ToString(buffer));
            var endAuthResponse = EndAuthResponse.Parse(buffer);

            var authKey = GetAuthKey(bBytes, answer.G_A, answer.DHPrime);
            TLUtils.WriteLine("-Big endian auth key----------------------------------");
            TLUtils.WriteLine(BitConverter.ToString(authKey));
            TLUtils.WriteLine("-Big endian auth key----------------------------------");

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine(DateTime.Now);
                w.WriteLine(BitConverter.ToString(authKey));
            }

            // II saveDeveloperInfo

            //var saveDeveloperInfoRequest = ComposeSaveDeveloperInfoRequest();
            //newNonce - little endian
            //authResponse.ServerNonce - little endian
            var salt = GetSalt(newNonce, authResponse.ServerNonce);
            TLUtils.WriteLine("Salt " + BitConverter.ToString(salt));
            var sessionId = new byte[8];
            random.NextBytes(sessionId);
            TLUtils.WriteLine("Session id " + BitConverter.ToString(sessionId));

            AuthKey = authKey;
            Salt = new TLLong(BitConverter.ToInt64(salt, 0));
            SessionId = new TLLong(BitConverter.ToInt64(sessionId, 0));

            return new Tuple<byte[], byte[], byte[]>(authKey, salt, sessionId);
        }


        public static byte[] GetSalt(byte[] newNonce, byte[] serverNonce)
        {
            var newNonceBytes = newNonce.Take(8).ToArray();
            var serverNonceBytes = serverNonce.Take(8).ToArray();

            //TLUtils.WriteLine("--Generate salt--");
            //TLUtils.WriteLine("NewNonce little endian   " + BitConverter.ToString(newNonce));
            //TLUtils.WriteLine("ServerNonce little endian " + BitConverter.ToString(serverNonce));

            //TLUtils.WriteLine("Getted 8 first bytes");
            //TLUtils.WriteLine("NewNonce    " + BitConverter.ToString(newNonceBytes));
            //TLUtils.WriteLine("ServerNonce " + BitConverter.ToString(serverNonceBytes));
            var returnBytes = new byte[8];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = (byte)(newNonceBytes[i] ^ serverNonceBytes[i]);
            }

            return returnBytes;
        }

        private static byte[] ComposeBeginAuthRequest()
        {
            byte[] res_pq = { 0x60, 0x46, 0x97, 0x78 };

            var randomNumber = new byte[16];
            var random = new Random();
            random.NextBytes(randomNumber);

            return res_pq.Reverse()
                .Concat(randomNumber).ToArray();
        }

        static byte[] CreatePlainMessageBody(byte[] data)
        {
            byte[] authKeyId = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            var now = DateTime.Now;
            var fullTimeBytes = BitConverter.GetBytes((long)Utils.DateTimeToUnixTimestamp(now));
            var unixTime = (long)Utils.DateTimeToUnixTimestamp(now) << 32;
            byte[] date = //{ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            BitConverter.GetBytes(unixTime);
            var messageBodyLength = BitConverter.GetBytes(data.Length);

            return authKeyId
                .Concat(date)
                .Concat(messageBodyLength)
                .Concat(data).ToArray();
        }

        // return big-endian authKey
        public static byte[] GetAuthKey(byte[] bBytes, byte[] g_aData, byte[] dhPrimeData)
        {
            int position = 0;
            var b = new BigInteger(bBytes.Reverse().Concat(new byte[] { 0x00 }).ToArray());
            var dhPrime = TLObject.GetObject<TLString>(dhPrimeData, ref position).ToBigInteger();
            position = 0;
            var g_a = TLObject.GetObject<TLString>(g_aData, ref position).ToBigInteger();

            var authKey = BigInteger.ModPow(g_a, b, dhPrime).ToByteArray(); // little endian + (may be) zero last byte

            //remove last zero byte
            if (authKey[authKey.Length - 1] == 0x00)
            {
                authKey = authKey.SubArray(0, authKey.Length - 1);
            }

            return authKey.Reverse().ToArray();
        }

        // b - big endian bytes
        // g - serialized data
        // dhPrime - serialized data
        // returns big-endian G_B
        public static byte[] GetG_B(byte[] bBytes, byte[] gData, byte[] dhPrimeData)
        {
            //var bBytes = new byte[256]; // big endian bytes
            //var random = new Random();
            //random.NextBytes(bBytes);
            int position = 0;
            var g = new BigInteger(gData);
            var dhPrime = TLObject.GetObject<TLString>(dhPrimeData, ref position).ToBigInteger();

            var b = new BigInteger(bBytes.Reverse().Concat(new byte[] { 0x00 }).ToArray());

            var g_b = BigInteger.ModPow(g, b, dhPrime).ToByteArray(); // little endian + (may be) zero last byte

            //remove last zero byte
            if (g_b[g_b.Length - 1] == 0x00)
            {
                g_b = g_b.SubArray(0, g_b.Length - 1);
            }

            return g_b.Reverse().ToArray();
        }

        public static Tuple<byte[], byte[]> GetAesKeyIV(byte[] serverNonce, byte[] newNonce)
        {
            SHA1 sha = new SHA1Managed();

            var newNonceServerNonce = newNonce.Concat(serverNonce).ToArray();
            var serverNonceNewNonce = serverNonce.Concat(newNonce).ToArray();
            var key = sha.ComputeHash(newNonceServerNonce)
                .Concat(sha.ComputeHash(serverNonceNewNonce).SubArray(0, 12));
            var im = sha.ComputeHash(serverNonceNewNonce).SubArray(12, 8)
                .Concat(sha.ComputeHash(newNonce.Concat(newNonce).ToArray()))
                .Concat(newNonce.SubArray(0, 4));

            return new Tuple<byte[], byte[]>(key.ToArray(), im.ToArray());
        }

        // encryptedData - big-endian number
        private static byte[] ComposeBeginDHRequest(AuthResponse response, UInt64 p, UInt64 q, byte[] encryptedData)
        {
            //TLUtils.WriteLine("---------------------------------");
            //TLUtils.WriteLine("Begin DH");
            //TLUtils.WriteLine("---------------------------------");
            var req_DH_params = new byte[] { 0xd7, 0x12, 0xe4, 0xbe };
            var nonce = response.Nonce;
            //TLUtils.WriteLine("ServerNonce: " + BitConverter.ToString(response.Nonce));
            var serverNonce = response.ServerNonce;
            //TLUtils.WriteLine("ServerNonce: " + BitConverter.ToString(response.ServerNonce));
            var pBytes = TLString.FromUInt64(p).ToBytes(); // 8
            //TLUtils.WriteLine("p: " + BitConverter.ToString(pBytes));
            var qBytes = TLString.FromUInt64(q).ToBytes(); // 8
            //TLUtils.WriteLine("q: " + BitConverter.ToString(qBytes));
            var fingerPrints = response.FingerPrints;
            //TLUtils.WriteLine("FingerPrints: " + BitConverter.ToString(response.FingerPrints));
            var encryptedDataBytes = new byte[] { 0xFE, 0x00, 0x01, 0x00 }.Concat(encryptedData).ToArray();
            //TLUtils.WriteLine("encryptedDataBytes: " + BitConverter.ToString(encryptedDataBytes));

            return req_DH_params.Reverse()
                .Concat(nonce)
                .Concat(serverNonce)
                .Concat(pBytes)
                .Concat(qBytes)
                .Concat(fingerPrints)
                .Concat(encryptedDataBytes).ToArray();
        }

        

        private static byte[] ComposeRequestSetClientDHParams(AuthResponse response, byte[] encryptedData)
        {
            //TLUtils.WriteLine("----Compose SetClientDHParams-------------");
            var set_client_DH_params = new byte[] { 0x1f, 0x5f, 0x04, 0xf5 };
            //TLUtils.WriteLine("set_client_DH_params " + BitConverter.ToString(set_client_DH_params));

            var nonce = response.Nonce;
            //TLUtils.WriteLine("Nonce " + BitConverter.ToString(nonce));

            var serverNonce = response.ServerNonce;
            //TLUtils.WriteLine("Server nonce " + BitConverter.ToString(serverNonce));

            var encryptedDataStr = TLString.FromBigEndianData(encryptedData);
            //TLUtils.WriteLine("encrypted data serialized");
            //TLUtils.WriteLine(BitConverter.ToString(encryptedDataStr.ToBytes(340)));

            return set_client_DH_params
                .Concat(nonce)
                .Concat(serverNonce)
                .Concat(encryptedDataStr.ToBytes()) // 340
                .ToArray();
        }

        public static byte[] ComposeClientDHInnerData(byte[] g_bBigEndianBytes, AuthResponse response)
        {
            //TLUtils.WriteLine("----Compose ClientDHInnerData-------------");
            var client_DH_inner_data = new byte[] { 0x54, 0xb6, 0x43, 0x66 };
            //TLUtils.WriteLine("client_DH_inner_data " + BitConverter.ToString(client_DH_inner_data));

            var nonce = response.Nonce;
            //TLUtils.WriteLine("Nonce " + BitConverter.ToString(nonce));

            var serverNonce = response.ServerNonce;
            //TLUtils.WriteLine("Server nonce " + BitConverter.ToString(serverNonce));

            Int64 retryId = 0;
            var retryIdBytes = BitConverter.GetBytes(retryId);
            //TLUtils.WriteLine("Retry id " + BitConverter.ToString(retryIdBytes));

            var strG_b = TLString.FromBigEndianData(g_bBigEndianBytes);
            //TLUtils.WriteLine("g_b serialized");
            //TLUtils.WriteLine(BitConverter.ToString(strG_b.ToBytes(260)));

            return client_DH_inner_data
                .Concat(nonce)
                .Concat(serverNonce)
                .Concat(retryIdBytes)
                .Concat(strG_b.ToBytes()).ToArray(); // 260

        }

        private static byte[] GetRSABytes(byte[] bytes)
        {
            // big-endian exponent and modulus
            const string exponentString = "010001";
            const string modulusString = "C150023E2F70DB7985DED064759CFECF" +
                                     "0AF328E69A41DAF4D6F01B538135A6F91F8F8B2A0EC9BA9720CE352EFCF6C5680FFC424BD6348649" +
                                     "02DE0B4BD6D49F4E580230E3AE97D95C8B19442B3C0A10D8F5633FECEDD6926A7F6DAB0DDB7D457F" +
                                     "9EA81B8465FCD6FFFEED114011DF91C059CAEDAF97625F6C96ECC74725556934EF781D866B34F011" +
                                     "FCE4D835A090196E9A5F0E4449AF7EB697DDB9076494CA5F81104A305B6DD27665722C46B60E5DF6" +
                                     "80FB16B210607EF217652E60236C255F6A28315F4083A96791D7214BF64C1DF4FD0DB1944FB26A2A" +
                                     "57031B32EEE64AD15A8BA68885CDE74A5BFC920F6ABF59BA5C75506373E7130F9042DA922179251F";
            var modulusBytes = Utils.StringToByteArray(modulusString);
            var exponentBytes = Utils.StringToByteArray(exponentString);
            var modulus = new BigInteger(modulusBytes.Reverse().Concat(new byte[] { 0x00 }).ToArray());
            var exponent = new BigInteger(exponentBytes.Reverse().Concat(new byte[] { 0x00 }).ToArray());
            var num = new BigInteger(bytes.Reverse().Concat(new byte[] { 0x00 }).ToArray());

            var rsa = BigInteger.ModPow(num, exponent, modulus).ToByteArray().Reverse().ToArray();
            if (rsa.Length == 257)
            {
                if (rsa[0] != 0x00) throw new Exception("rsa last byte is " + rsa[0]);

                TLUtils.WriteLine("First RSA byte removes: byte value is " + rsa[0]);
                rsa = rsa.SubArray(1, 256);
            }
            return rsa;
        }

        public static byte[] ComposeData(AuthResponse response, UInt64 p, UInt64 q, byte[] newNonce)
        {
            var pqInnerData = new byte[] { 0x83, 0xc9, 0x5a, 0xec };

            var pq = response.pqString.ToBytes(); //12
            var pBytes = TLString.FromUInt64(p).ToBytes(); //8
            var qBytes = TLString.FromUInt64(q).ToBytes(); //8

            var nonce = response.Nonce;
            var serverNonce = response.ServerNonce;

            return pqInnerData.Reverse().ToArray()
                .Concat(pq).ToArray()
                .Concat(pBytes).ToArray()
                .Concat(qBytes).ToArray()
                .Concat(nonce).ToArray()
                .Concat(serverNonce).ToArray()
                .Concat(newNonce).ToArray();
        }
    }



    public class AuthResponse
    {
        public byte[] AuthKeyId { get; set; }

        public byte[] MessageId { get; set; }

        public Int32 MessageLength { get; set; }

        public byte[] Nonce { get; set; }

        public byte[] ServerNonce { get; set; }

        public byte[] FingerPrints { get; set; }

        public UInt64 pq { get; set; }

        public TLString pqString { get; set; }

        public static AuthResponse Parse(byte[] bytes)
        {
            var response = new AuthResponse();
            response.AuthKeyId = bytes.SubArray(0, 8);
            //TLUtils.WriteLine("AuthKeyId: " + BitConverter.ToString(response.AuthKeyId));

            response.MessageId = bytes.SubArray(8, 8);
            var unixTime = BitConverter.ToInt64(response.MessageId, 0) >> 32;
            //var serverDate = Utils.UnixTimestampToDateTime(unixTime);
            //TLUtils.WriteLine("Server time: " + serverDate);
            //TLUtils.WriteLine("MessageId: " + BitConverter.ToString(response.MessageId));

            response.MessageLength = BitConverter.ToInt32(bytes.SubArray(16, 4), 0);
            //TLUtils.WriteLine("MessageLength: " + response.MessageLength);

            response.Nonce = bytes.SubArray(24, 16);
            //TLUtils.WriteLine("Nonce: " + BitConverter.ToString(response.Nonce));

            response.ServerNonce = bytes.SubArray(40, 16);
            //TLUtils.WriteLine("ServerNonce: " + BitConverter.ToString(response.ServerNonce));

            var pqBytes = //new byte[] { 0x08, 0x17, 0xED, 0x48, 0x94, 0x1A, 0x08, 0xF9, 0x81, 0x00, 0x00, 0x00 }; 
            bytes.SubArray(56, 12);
            //TLUtils.WriteLine("pq bytes: " + BitConverter.ToString(pqBytes));
            int position = 0;
            response.pqString = TLObject.GetObject<TLString>(pqBytes, ref position);
            response.pq = BitConverter.ToUInt64(response.pqString.Data, 0);
            //TLUtils.WriteLine("pq: " + response.pq);

            response.FingerPrints = bytes.SubArray(76, 8);
            //TLUtils.WriteLine("FingerPrints: " + BitConverter.ToString(response.FingerPrints));

            return response;
        }
    }

    internal class EndAuthResponse
    {
        public byte[] AuthKeyId { get; set; }

        public byte[] MessageId { get; set; }

        public Int32 MessageLength { get; set; }

        public byte[] Status { get; set; }

        public byte[] Nonce { get; set; }

        public byte[] ServerNonce { get; set; }

        public byte[] NewNonceSHA1 { get; set; }

        public static EndAuthResponse Parse(byte[] bytes)
        {
            TLUtils.WriteLine("----------------------------");
            TLUtils.WriteLine("--Parse end auth response---");
            TLUtils.WriteLine("----------------------------");

            var response = new EndAuthResponse();


            response.AuthKeyId = bytes.SubArray(0, 8);
            TLUtils.WriteLine("AuthKeyId: " + BitConverter.ToString(response.AuthKeyId));

            response.MessageId = bytes.SubArray(8, 8);
            var unixTime = BitConverter.ToInt64(response.MessageId, 0) >> 32;
            var serverDate = Utils.UnixTimestampToDateTime(unixTime);
            TLUtils.WriteLine("Server time: " + serverDate);
            TLUtils.WriteLine("  MESSAGEID: " + BitConverter.ToString(response.MessageId));

            response.MessageLength = BitConverter.ToInt32(bytes.SubArray(16, 4), 0);
            TLUtils.WriteLine("MessageLength: " + response.MessageLength);

            response.Status = bytes.SubArray(20, 4);
            TLUtils.WriteLine("Status " + BitConverter.ToString(response.Status));
            TLUtils.WriteLine(string.Equals(BitConverter.ToString(response.Status), "34-f7-cb-3b",
                                            StringComparison.OrdinalIgnoreCase)
                                  ? "Auth OK"
                                  : "Auth Fail");

            response.Nonce = bytes.SubArray(24, 16);
            TLUtils.WriteLine("Nonce: " + BitConverter.ToString(response.Nonce));

            response.ServerNonce = bytes.SubArray(40, 16);
            TLUtils.WriteLine("ServerNonce: " + BitConverter.ToString(response.ServerNonce));

            response.NewNonceSHA1 = bytes.SubArray(56, 16);
            TLUtils.WriteLine("NewNonceSHA1: " + BitConverter.ToString(response.NewNonceSHA1));

            return response;
        }
    }

    internal class Answer
    {

        public byte[] Status { get; set; }

        public byte[] Nonce { get; set; }

        public byte[] ServerNonce { get; set; }

        public byte[] G { get; set; }

        public byte[] DHPrime { get; set; }

        public byte[] G_A { get; set; }

        public byte[] ServerTime { get; set; }

        public static Answer Parse(byte[] bytes)
        {
            //TLUtils.WriteLine("----------------------------");
            //TLUtils.WriteLine("--Server_DH_inner_dat-------");
            //TLUtils.WriteLine("----------------------------");

            var answer = new Answer();

            answer.Status = bytes.SubArray(0, 4);
            //TLUtils.WriteLine(string.Equals(BitConverter.ToString(answer.Status), "BA-0D-89-B5",
            //                                StringComparison.OrdinalIgnoreCase)
            //                      ? "Server_DH_inner_dat OK"
            //                      : "Server_DH_inner_dat Fail");

            answer.Nonce = bytes.SubArray(4, 16);
            TLUtils.WriteLine("Nonce: " + BitConverter.ToString(answer.Nonce));

            answer.ServerNonce = bytes.SubArray(20, 16);
            TLUtils.WriteLine("ServerNonce: " + BitConverter.ToString(answer.ServerNonce));

            answer.G = bytes.SubArray(36, 4);
            //TLUtils.WriteLine("G: " + BitConverter.ToString(answer.G));

            answer.DHPrime = bytes.SubArray(40, 260);
            //TLUtils.WriteLine("DHPrime: " + BitConverter.ToString(answer.DHPrime));
            //TLUtils.WriteLine();
            answer.G_A = bytes.SubArray(300, 260);
            //TLUtils.WriteLine("G_A: " + BitConverter.ToString(answer.DHPrime));

            answer.ServerTime = bytes.SubArray(560, 4);
            var unixTime = BitConverter.ToInt32(answer.ServerTime, 0);
            var serverDate = Utils.UnixTimestampToDateTime(unixTime);
            //TLUtils.WriteLine("Server time: " + serverDate);


            return answer;
        }
    }


    internal class BeginDHResponse
    {
        public byte[] AuthKeyId { get; set; }

        public byte[] MessageId { get; set; }

        public Int32 MessageLength { get; set; }

        public byte[] Status { get; set; }

        public byte[] Nonce { get; set; }

        public byte[] ServerNonce { get; set; }

        public byte[] EncryptedAnswer { get; set; }

        public static BeginDHResponse Parse(byte[] bytes)
        {
            //TLUtils.WriteLine("----------------------------");
            //TLUtils.WriteLine("--server_DH_params----------");
            //TLUtils.WriteLine("----------------------------");

            var response = new BeginDHResponse();
            response.AuthKeyId = bytes.SubArray(0, 8);
            //TLUtils.WriteLine("AuthKeyId: " + BitConverter.ToString(response.AuthKeyId));

            response.MessageId = bytes.SubArray(8, 8);
            var unixTime = BitConverter.ToInt64(response.MessageId, 0) >> 32;
            var serverDate = Utils.UnixTimestampToDateTime(unixTime);
            //TLUtils.WriteLine("Server time: " + serverDate);
            //TLUtils.WriteLine("MessageId: " + BitConverter.ToString(response.MessageId));

            response.MessageLength = BitConverter.ToInt32(bytes.SubArray(16, 4), 0);
            //TLUtils.WriteLine("MessageLength: " + response.MessageLength);

            response.Status = bytes.SubArray(20, 4);
            //TLUtils.WriteLine(string.Equals(BitConverter.ToString(response.Status), "5c-07-e8-d0",
            //                                StringComparison.OrdinalIgnoreCase)
            //                      ? "DH Params OK"
            //                      : "DH Params Fail");

            response.Nonce = bytes.SubArray(24, 16);
            //TLUtils.WriteLine("Nonce: " + BitConverter.ToString(response.Nonce));

            response.ServerNonce = bytes.SubArray(40, 16);
            //TLUtils.WriteLine("ServerNonce: " + BitConverter.ToString(response.ServerNonce));

            response.EncryptedAnswer = bytes.SubArray(60, 592);
            //TLUtils.WriteLine("EncryptedAnswer: ");
            //TLUtils.WriteLine(BitConverter.ToString(response.EncryptedAnswer));

            return response;
        }
    }*/
}
