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
    public abstract class TLChannelAdminLogEventActionBase : TLObject { }

    public class TLChannelAdminLogEventActionChangeTitle : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionChangeTitle;

        public TLString PrevValue { get; set; }

        public TLString NewValue { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevValue = GetObject<TLString>(bytes, ref position);
            NewValue = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevValue = GetObject<TLString>(input);
            NewValue = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevValue.ToStream(output);
            NewValue.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionChangeAbout : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionChangeAbout;

        public TLString PrevValue { get; set; }

        public TLString NewValue { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevValue = GetObject<TLString>(bytes, ref position);
            NewValue = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevValue = GetObject<TLString>(input);
            NewValue = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevValue.ToStream(output);
            NewValue.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionChangeUsername : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionChangeUsername;

        public TLString PrevValue { get; set; }

        public TLString NewValue { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevValue = GetObject<TLString>(bytes, ref position);
            NewValue = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevValue = GetObject<TLString>(input);
            NewValue = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevValue.ToStream(output);
            NewValue.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionChangePhoto : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionChangePhoto;

        public TLPhotoBase PrevPhoto { get; set; }

        public TLPhotoBase NewPhoto { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NewPhoto = GetObject<TLPhotoBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevPhoto = GetObject<TLPhotoBase>(input);
            NewPhoto = GetObject<TLPhotoBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevPhoto.ToStream(output);
            NewPhoto.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionToggleInvites : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionToggleInvites;

        public TLBool NewValue { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NewValue = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            NewValue = GetObject<TLBool>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            NewValue.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionToggleSignatures : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionToggleSignatures;

        public TLBool NewValue { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NewValue = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            NewValue = GetObject<TLBool>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            NewValue.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionUpdatePinned : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionUpdatePinned;

        public TLMessageBase Message { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLMessageBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionEditMessage : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionEditMessage;

        public TLMessageBase PrevMessage { get; set; }

        public TLMessageBase NewMessage { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevMessage = GetObject<TLMessageBase>(bytes, ref position);
            NewMessage = GetObject<TLMessageBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevMessage = GetObject<TLMessageBase>(input);
            NewMessage = GetObject<TLMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevMessage.ToStream(output);
            NewMessage.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionDeleteMessage : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionDeleteMessage;

        public TLMessageBase Message { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLMessageBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionParticipantJoin : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionParticipantJoin;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLChannelAdminLogEventActionParticipantLeave : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionParticipantLeave;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLChannelAdminLogEventActionParticipantInvite : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionParticipantInvite;

        public TLChannelParticipantBase Participant { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Participant = GetObject<TLChannelParticipantBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Participant = GetObject<TLChannelParticipantBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Participant.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionParticipantToggleBan : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionParticipantToggleBan;

        public TLChannelParticipantBase PrevParticipant { get; set; }

        public TLChannelParticipantBase NewParticipant { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevParticipant = GetObject<TLChannelParticipantBase>(bytes, ref position);
            NewParticipant = GetObject<TLChannelParticipantBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevParticipant = GetObject<TLChannelParticipantBase>(input);
            NewParticipant = GetObject<TLChannelParticipantBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevParticipant.ToStream(output);
            NewParticipant.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionParticipantToggleAdmin : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionParticipantToggleAdmin;

        public TLChannelParticipantBase PrevParticipant { get; set; }

        public TLChannelParticipantBase NewParticipant { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevParticipant = GetObject<TLChannelParticipantBase>(bytes, ref position);
            NewParticipant = GetObject<TLChannelParticipantBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevParticipant = GetObject<TLChannelParticipantBase>(input);
            NewParticipant = GetObject<TLChannelParticipantBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevParticipant.ToStream(output);
            NewParticipant.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionChangeStickerSet : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionChangeStickerSet;

        public TLInputStickerSetBase PrevStickerSet { get; set; }

        public TLInputStickerSetBase NewStickerSet { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PrevStickerSet = GetObject<TLInputStickerSetBase>(bytes, ref position);
            NewStickerSet = GetObject<TLInputStickerSetBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PrevStickerSet = GetObject<TLInputStickerSetBase>(input);
            NewStickerSet = GetObject<TLInputStickerSetBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PrevStickerSet.ToStream(output);
            NewStickerSet.ToStream(output);
        }
    }

    public class TLChannelAdminLogEventActionTogglePreHistoryHidden : TLChannelAdminLogEventActionBase
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventActionTogglePreHistoryHidden;

        public TLBool NewValue { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NewValue = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            NewValue = GetObject<TLBool>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            NewValue.ToStream(output);
        }
    }
}
