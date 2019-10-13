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
    public enum CdnPublicKeyCustomFlags
    {
        PublicKeyFingerprint = 0x1
    }

    public class TLCdnPublicKey : TLObject
    {
        public const uint Signature = TLConstructors.TLCdnPublicKey;

        public TLInt DCId { get; set; }

        public TLString PublicKey { get; set; }

        #region Additional

        private TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        private TLLong _publicKeyFingerprint;

        public TLLong PublicKeyFingerprint
        {
            get { return _publicKeyFingerprint; }
            set { SetField(out _publicKeyFingerprint, value, ref _customFlags, (int) CdnPublicKeyCustomFlags.PublicKeyFingerprint); }
        }

        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            DCId = GetObject<TLInt>(bytes, ref position);
            PublicKey = GetObject<TLString>(bytes, ref position);

            CustomFlags = new TLLong(0);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            DCId = GetObject<TLInt>(input);
            PublicKey = GetObject<TLString>(input);
            CustomFlags = GetObject<TLLong>(input);
            _publicKeyFingerprint = GetObject<TLLong>(CustomFlags, (int) CdnPublicKeyCustomFlags.PublicKeyFingerprint, null, input);


            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            DCId.ToStream(output);
            PublicKey.ToStream(output);
            CustomFlags.ToStream(output);
            ToStream(output, _publicKeyFingerprint, CustomFlags, (int)CdnPublicKeyCustomFlags.PublicKeyFingerprint);
        }
    }
}
