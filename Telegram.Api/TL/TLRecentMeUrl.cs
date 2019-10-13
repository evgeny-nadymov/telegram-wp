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
    public abstract class TLRecentMeUrlBase : TLObject
    {
        public TLString Url { get; set; }
    }

    public class TLRecentMeUrlUnknown : TLRecentMeUrlBase
    {
        public const uint Signature = TLConstructors.TLRecentMeUrlUnknown;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
        }
    }

    public class TLRecentMeUrlUser : TLRecentMeUrlBase
    {
        public const uint Signature = TLConstructors.TLRecentMeUrlUser;

        public TLInt UserId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
            UserId.ToStream(output);
        }
    }

    public class TLRecentMeUrlChat : TLRecentMeUrlBase
    {
        public const uint Signature = TLConstructors.TLRecentMeUrlChat;

        public TLInt ChatId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            ChatId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            ChatId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
            ChatId.ToStream(output);
        }
    }

    public class TLRecentMeUrlChatInvite : TLRecentMeUrlBase
    {
        public const uint Signature = TLConstructors.TLRecentMeUrlChatInvite;

        public TLChatInviteBase ChatInvite { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            ChatInvite = GetObject<TLChatInviteBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            ChatInvite = GetObject<TLChatInviteBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
            ChatInvite.ToStream(output);
        }
    }

    public class TLRecentMeUrlStickerSet : TLRecentMeUrlBase
    {
        public const uint Signature = TLConstructors.TLRecentMeUrlStickerSet;

        public TLStickerSetCoveredBase Set { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            Set = GetObject<TLStickerSetCoveredBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            Set = GetObject<TLStickerSetCoveredBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
            Set.ToStream(output);
        }
    }
}
