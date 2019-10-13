// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLInputAudioBase : TLObject { }

    public class TLInputAudioEmpty : TLInputAudioBase
    {
        public const uint Signature = TLConstructors.TLInputAudioEmpty;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputAudio : TLInputAudioBase
    {
        public const uint Signature = TLConstructors.TLInputAudio;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }
    }
}
