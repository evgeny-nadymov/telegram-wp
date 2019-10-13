using System;
using System.Linq;
using System.Security.Cryptography;
using Telegram.Api.Services;
using Telegram.Api.TL;
using Telegram.Api.Transport;

namespace Telegram.Api.Helpers
{
    class RequestHelper
    {
        private readonly ITransport _transport;

        public RequestHelper(ITransport transport)
        {
            _transport = transport;
        }

        public TLResponse Send(string caption, int seqNo, Func<byte[]> getData)
        {
            var authKey = MTProtoService.AuthKey;
            var salt = MTProtoService.Salt;
            var sessionId = MTProtoService.SessionId;

            TLUtils.WriteLine();
            TLUtils.WriteLine("------------------------");
            TLUtils.WriteLine(String.Format("--{0}--", caption));
            TLUtils.WriteLine("------------------------");

            SHA1 sha = new SHA1Managed();
            var random = new Random();
            var request = getData();

            TLUtils.WriteLine("Salt: " + salt);
            TLUtils.WriteLine("SessionId: " + sessionId);
            var messageId = TLUtils.GenerateMessageId();
            TLUtils.WriteLine("->MESSAGEID: " + TLUtils.MessageIdString(messageId));
            TLUtils.WriteLine("  SEQUENCENUMBER: " + seqNo);
            //var seqNo = BitConverter.GetBytes(3);
            var data = salt.ToBytes()
                .Concat(sessionId.ToBytes())
                .Concat(messageId.ToBytes())
                .Concat(BitConverter.GetBytes(seqNo))
                .Concat(BitConverter.GetBytes(request.Length))
                .Concat(request)
                .ToArray();

            var length = data.Length;
            var padding = 16 - (length % 16);
            byte[] paddingBytes = null;
            if (padding > 0 && padding < 16)
            {
                paddingBytes = new byte[padding];
                random.NextBytes(paddingBytes);
            }

            byte[] dataWithPadding = data;
            if (paddingBytes != null)
            {
                dataWithPadding = data.Concat(paddingBytes).ToArray();
            }


            var msgKey = TLUtils.GetMsgKey(data);
            var keyIV = Utils.GetEncryptKeyIV(authKey, msgKey);
            var encryptedData = Utils.AesIge(dataWithPadding, keyIV.Item1, keyIV.Item2, true);

            //TLUtils.WriteLine("--Compute auth key sha1--");
            var authKeyHash = sha.ComputeHash(authKey);
            var authKeyId = authKeyHash.SubArray(12, 8);
            //TLUtils.WriteLine("Auth key sha1:           " + BitConverter.ToString(authKeyHash));
            //TLUtils.WriteLine("Auth key little 8 bytes: " + BitConverter.ToString(authKeyId));

            //TLUtils.WriteLine("--Check phone request--");
            var reqBytes = authKeyId.Concat(msgKey).Concat(encryptedData).ToArray();

            return null;
           // var buffer = _transport.SendBytes(reqBytes);

            //TLUtils.WriteLine("Buffer:");
            //TLUtils.WriteLine(BitConverter.ToString(buffer));

            //Console.ReadKey();
            //return TLResponse.Parse(buffer, authKey);
        }
    }
}
