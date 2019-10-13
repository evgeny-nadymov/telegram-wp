// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public class TLSecureData : TLObject
    {
        public const uint Signature = TLConstructors.TLSecureData;

        public TLString Data { get; set; }

        public TLString DataHash { get; set; }

        public TLString Secret { get; set; }

        #region Additional
        public object DecryptedData { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Data = GetObject<TLString>(bytes, ref position);
            DataHash = GetObject<TLString>(bytes, ref position);
            Secret = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Data.ToBytes(),
                DataHash.ToBytes(),
                Secret.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Data = GetObject<TLString>(input);
            DataHash = GetObject<TLString>(input);
            Secret = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Data.ToStream(output);
            DataHash.ToStream(output);
            Secret.ToStream(output);
        }
    }
}
