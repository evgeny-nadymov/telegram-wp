// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define MTPROTO

using System;
using System.Linq;
using Org.BouncyCastle.Security;
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    public class TLEncryptedTransportMessage : TLObject
    {
        public TLLong AuthKeyId { get; set; }
        public byte[] MsgKey { get; set; } //128 bit
        public byte[] Data { get; set; }

        public TLEncryptedTransportMessage Decrypt(byte[] authKey)
        {
            return Decrypt(this, authKey);
        }

        public static TLEncryptedTransportMessage Decrypt(TLEncryptedTransportMessage transportMessage, byte[] authKey)
        {
            var keyIV = TLUtils.GetDecryptKeyIV(authKey, transportMessage.MsgKey);
            transportMessage.Data = Utils.AesIge(transportMessage.Data, keyIV.Item1, keyIV.Item2, false);

            //var msgKey = TLUtils.GetDecryptMsgKey(authKey, transportMessage.Data);
            //if (!TLUtils.ByteArraysEqual(msgKey, transportMessage.MsgKey))
            //{
            //    transportMessage.Data = null;
            //}

            return transportMessage;
        }

        public TLEncryptedTransportMessage Encrypt(byte[] authKey)
        {
            return Encrypt(this, authKey);
        }

        public static TLEncryptedTransportMessage Encrypt(TLEncryptedTransportMessage transportMessage, byte[] authKey)
        {
#if MTPROTO
            var random = new SecureRandom();
            
            var data = transportMessage.Data;

            var length = data.Length;
            var padding = 16 - (length % 16);
            byte[] paddingBytes = null;
            if (padding < 12)
            {
                padding += 16;
            }
            if (padding >= 12 && padding <= 1024)
            {
                paddingBytes = new byte[padding];
                random.NextBytes(paddingBytes);
            }

            var dataWithPadding = data;
            if (paddingBytes != null)
            {
                dataWithPadding = TLUtils.Combine(data, paddingBytes);
            }

            var msgKey = TLUtils.GetEncryptMsgKey(authKey, dataWithPadding);
            var keyIV = TLUtils.GetEncryptKeyIV(authKey, msgKey);
            var encryptedData = Utils.AesIge(dataWithPadding, keyIV.Item1, keyIV.Item2, true);

            var authKeyId = TLUtils.GenerateLongAuthKeyId(authKey);

            transportMessage.AuthKeyId = new TLLong(authKeyId);
            transportMessage.MsgKey = msgKey;
            transportMessage.Data = encryptedData;

            return transportMessage;
#else
            var random = new SecureRandom();

            var data = transportMessage.Data;

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
                dataWithPadding = TLUtils.Combine(data, paddingBytes);
            }


            var msgKey = TLUtils.GetMsgKey(data);
            var keyIV = TLUtils.GetEncryptKeyIV(authKey, msgKey);
            var encryptedData = Utils.AesIge(dataWithPadding, keyIV.Item1, keyIV.Item2, true);

            var authKeyId = TLUtils.GenerateLongAuthKeyId(authKey);

            transportMessage.AuthKeyId = new TLLong(authKeyId);
            transportMessage.MsgKey = msgKey;
            transportMessage.Data = encryptedData;

            return transportMessage;
#endif
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            var response = new TLEncryptedTransportMessage();
            response.AuthKeyId = GetObject<TLLong>(bytes, ref position);
            response.MsgKey = bytes.SubArray(position, 16);

            position += 16;
            response.Data = bytes.SubArray(position, bytes.Length - position);
            position = bytes.Length;
            return response;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                AuthKeyId.ToBytes(),
                MsgKey,
                Data);
        }
    }
}
