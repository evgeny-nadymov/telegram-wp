// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Channels
{
    class TLCreateChannel : TLObject
    {
        public const uint Signature = 0xf4893d7f;

        public TLInt Flags { get; set; }

        public TLString Title { get; set; }

        public TLString About { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Title.ToBytes(),
                About.ToBytes());
        }
    }
}
