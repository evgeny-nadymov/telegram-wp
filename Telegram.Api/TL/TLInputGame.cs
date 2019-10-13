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
    public abstract class TLInputGameBase : TLObject { }

    public class TLInputGameId : TLInputGameBase
    {
        public const uint Signature = TLConstructors.TLInputGameId;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            AccessHash.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLInputGameId id={0} access_hash={1}", Id, AccessHash);
        }
    }

    public class TLInputGameShortName : TLInputGameBase
    {
        public const uint Signature = TLConstructors.TLInputGameShortName;

        public TLInputUserBase BotId { get; set; }

        public TLString ShortName { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                BotId.ToBytes(),
                ShortName.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            BotId = GetObject<TLInputUserBase>(input);
            ShortName = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            BotId.ToStream(output);
            ShortName.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLInputGameShortName bot_id={0} short_name={1}", BotId, ShortName);
        }
    }
}
