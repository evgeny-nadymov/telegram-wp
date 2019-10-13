// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum EncryptedChatCustomFlags
    {
        OriginalKey = 0x1,
        ExtendedKey = 0x2,
        LinkPreview = 0x4
    }

    public abstract class TLEncryptedChatBase : TLObject
    {
        public int Index { get { return Id.Value; } }

        public TLInt Id { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Id = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        #region Additional

        protected TLString _key;

        public TLString Key
        {
            get { return _key; }
            set
            {
                if (_key == null && value != null)
                {
                    if (OriginalKey == null)
                    {
                        OriginalKey = value;
                    }

                    if (ExtendedKey == null)
                    {
                        var chat17 = this as TLEncryptedChat17;
                        if (chat17 != null && chat17.Layer != null
                            && chat17.Layer.Value >= Constants.MinSecretChatWithExtendedKeyVisualizationLayer)
                        {
                            ExtendedKey = value;
                        }
                    }
                }

                _key = value;
            }
        }

        public TLLong KeyFingerprint { get; set; }

        public TLString P { get; set; }

        public TLInt G { get; set; }

        public TLString A { get; set; }

        public TLInt MessageTTL { get; set; }

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        protected TLString _originalKey;

        public TLString OriginalKey
        {
            get { return _originalKey; }
            set
            {
                if (value != null)
                {
                    Set(ref _customFlags, (int)EncryptedChatCustomFlags.OriginalKey);
                    _originalKey = value;
                }
                else
                {
                    Unset(ref _customFlags, (int)EncryptedChatCustomFlags.OriginalKey);
                    _originalKey = null;
                }
            }
        }

        protected TLString _extendedKey;

        public TLString ExtendedKey
        {
            get { return _extendedKey; }
            set
            {
                if (value != null)
                {
                    Set(ref _customFlags, (int)EncryptedChatCustomFlags.ExtendedKey);
                    _extendedKey = value;
                }
                else
                {
                    Unset(ref _customFlags, (int)EncryptedChatCustomFlags.ExtendedKey);
                    _extendedKey = null;
                }
            }
        }
        #endregion

        public virtual void Update(TLEncryptedChatBase chat)
        {
            Id = chat.Id;
            if (chat.Key != null) _key = chat._key;
            if (chat.KeyFingerprint != null) KeyFingerprint = chat.KeyFingerprint;
            if (chat.P != null) P = chat.P;
            if (chat.G != null) G = chat.G;
            if (chat.A != null) A = chat.A;

            if (chat.CustomFlags != null) CustomFlags = chat.CustomFlags;
            if (chat.OriginalKey != null) OriginalKey = chat.OriginalKey;
            if (chat.ExtendedKey != null) ExtendedKey = chat.ExtendedKey;
        }
    }

    public class TLEncryptedChatEmpty : TLEncryptedChatBase
    {
        public const uint Signature = TLConstructors.TLEncryptedChatEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            base.FromBytes(bytes, ref position);

            return this;
        }

        public override void Update(TLEncryptedChatBase chat)
        {
            base.Update(chat);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);

            _key = GetNullableObject<TLString>(input);
            KeyFingerprint = GetNullableObject<TLLong>(input);
            P = GetNullableObject<TLString>(input);
            G = GetNullableObject<TLInt>(input);
            A = GetNullableObject<TLString>(input);
            MessageTTL = GetNullableObject<TLInt>(input);
            
            CustomFlags = GetNullableObject<TLLong>(input);
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey))
            {
                OriginalKey = GetObject<TLString>(input);
            }
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey))
            {
                ExtendedKey = GetObject<TLString>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());

            Key.NullableToStream(output);
            KeyFingerprint.NullableToStream(output);
            P.NullableToStream(output);
            G.NullableToStream(output);
            A.NullableToStream(output);
            MessageTTL.NullableToStream(output);

            CustomFlags.NullableToStream(output);
            ToStream(output, OriginalKey, CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey);
            ToStream(output, ExtendedKey, CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey);
        }
    }

    public abstract class TLEncryptedChatCommon : TLEncryptedChatBase
    {
        public TLLong AccessHash { get; set; }

        public TLInt Date { get; set; }

        public TLInt AdminId { get; set; }

        public TLInt ParticipantId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            base.FromBytes(bytes, ref position);

            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            AdminId = GetObject<TLInt>(bytes, ref position);
            ParticipantId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void Update(TLEncryptedChatBase chat)
        {
            base.Update(chat);

            var chatCommon = chat as TLEncryptedChatCommon;
            if (chatCommon != null)
            {
                AccessHash = chatCommon.AccessHash;
                Date = chatCommon.Date;
                AdminId = chatCommon.AdminId;
                ParticipantId = chatCommon.ParticipantId;
            }
        }
    }

    public class TLEncryptedChatWaiting : TLEncryptedChatCommon
    {
        public const uint Signature = TLConstructors.TLEncryptedChatWaiting;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            base.FromBytes(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);

            _key = GetNullableObject<TLString>(input);
            KeyFingerprint = GetNullableObject<TLLong>(input);
            P = GetNullableObject<TLString>(input);
            G = GetNullableObject<TLInt>(input);
            A = GetNullableObject<TLString>(input);
            MessageTTL = GetNullableObject<TLInt>(input);
            
            CustomFlags = GetNullableObject<TLLong>(input);
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey))
            {
                OriginalKey = GetObject<TLString>(input);
            }
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey))
            {
                ExtendedKey = GetObject<TLString>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Date.ToBytes());
            output.Write(AdminId.ToBytes());
            output.Write(ParticipantId.ToBytes());

            Key.NullableToStream(output);
            KeyFingerprint.NullableToStream(output);
            P.NullableToStream(output);
            G.NullableToStream(output);
            A.NullableToStream(output);
            MessageTTL.NullableToStream(output);

            CustomFlags.NullableToStream(output);
            ToStream(output, OriginalKey, CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey);
            ToStream(output, ExtendedKey, CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey);
        }
    }

    public class TLEncryptedChatRequested : TLEncryptedChatCommon
    {
        public const uint Signature = TLConstructors.TLEncryptedChatRequested;

        public TLString GA { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            base.FromBytes(bytes, ref position);

            GA = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void Update(TLEncryptedChatBase chat)
        {
            base.Update(chat);

            var chatRequested = chat as TLEncryptedChatRequested;
            if (chatRequested != null)
            {
                GA = chatRequested.GA;
            }
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GA = GetObject<TLString>(input);

            _key = GetNullableObject<TLString>(input);
            KeyFingerprint = GetNullableObject<TLLong>(input);
            P = GetNullableObject<TLString>(input);
            G = GetNullableObject<TLInt>(input);
            A = GetNullableObject<TLString>(input);
            MessageTTL = GetNullableObject<TLInt>(input);
            
            CustomFlags = GetNullableObject<TLLong>(input);
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey))
            {
                OriginalKey = GetObject<TLString>(input);
            }
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey))
            {
                ExtendedKey = GetObject<TLString>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Date.ToBytes());
            output.Write(AdminId.ToBytes());
            output.Write(ParticipantId.ToBytes());
            output.Write(GA.ToBytes());

            Key.NullableToStream(output);
            KeyFingerprint.NullableToStream(output);
            P.NullableToStream(output);
            G.NullableToStream(output);
            A.NullableToStream(output);
            MessageTTL.NullableToStream(output);

            CustomFlags.NullableToStream(output);
            ToStream(output, OriginalKey, CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey);
            ToStream(output, ExtendedKey, CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey);
        }
    }

    public class TLEncryptedChat20 : TLEncryptedChat17
    {
        public new const uint Signature = TLConstructors.TLEncryptedChat20;

        public TLLong PFS_ExchangeId { get; set; }
        public TLString PFS_A { get; set; }
        public TLString PFS_Key { get; set; }

        public TLLong PFS_KeyFingerprint { get; set; }

        public override void Update(TLEncryptedChatBase chat)
        {
            base.Update(chat);

            var encryptedChat = chat as TLEncryptedChat20;
            if (encryptedChat != null)
            {
                PFS_ExchangeId = encryptedChat.PFS_ExchangeId;
                PFS_A = encryptedChat.PFS_A;
                PFS_Key = encryptedChat.PFS_Key;
                PFS_KeyFingerprint = encryptedChat.PFS_KeyFingerprint;
            }
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GAorB = GetObject<TLString>(input);

            _key = GetNullableObject<TLString>(input);
            KeyFingerprint = GetNullableObject<TLLong>(input);
            P = GetNullableObject<TLString>(input);
            G = GetNullableObject<TLInt>(input);
            A = GetNullableObject<TLString>(input);
            MessageTTL = GetNullableObject<TLInt>(input);

            CustomFlags = GetNullableObject<TLLong>(input);
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey))
            {
                OriginalKey = GetObject<TLString>(input);
            }
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey))
            {
                ExtendedKey = GetObject<TLString>(input);
            }

            RawInSeqNo = GetNullableObject<TLInt>(input);
            RawOutSeqNo = GetNullableObject<TLInt>(input);
            Layer = GetNullableObject<TLInt>(input);

            PFS_ExchangeId = GetNullableObject<TLLong>(input);
            PFS_A = GetNullableObject<TLString>(input);
            PFS_Key = GetNullableObject<TLString>(input);
            PFS_KeyFingerprint = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Date.ToBytes());
            output.Write(AdminId.ToBytes());
            output.Write(ParticipantId.ToBytes());
            output.Write(GAorB.ToBytes());

            Key.NullableToStream(output);
            KeyFingerprint.NullableToStream(output);
            P.NullableToStream(output);
            G.NullableToStream(output);
            A.NullableToStream(output);
            MessageTTL.NullableToStream(output);

            CustomFlags.NullableToStream(output);
            ToStream(output, OriginalKey, CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey);
            ToStream(output, ExtendedKey, CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey);

            RawInSeqNo.NullableToStream(output);
            RawOutSeqNo.NullableToStream(output);
            Layer.NullableToStream(output);

            PFS_ExchangeId.NullableToStream(output);
            PFS_A.NullableToStream(output);
            PFS_Key.NullableToStream(output);
            PFS_KeyFingerprint.NullableToStream(output);
        }

        public override string ToString()
        {
            return string.Format("EncryptedChat20={0} Hash={1}", Index, GetHashCode());
        }
    }

    public class TLEncryptedChat17 : TLEncryptedChat
    {
        public new const uint Signature = TLConstructors.TLEncryptedChat17;

        private TLInt _rawInSeqNo;

        // from 0, increment by 1 after each received decryptedMessage/decryptedMessageService
        public TLInt RawInSeqNo
        {
            get { return _rawInSeqNo; }
            set
            {
                System.Diagnostics.Debug.WriteLine("  raw_in_seq_no chat={0} old={1} new={2}", Id, _rawInSeqNo, value);
                _rawInSeqNo = value;
            }
        }

        private TLInt _rawOutSeqNo;

        // from 0, increment by 1 after each sent decryptedMessage/decryptedMessageService
        public TLInt RawOutSeqNo
        {
            get { return _rawOutSeqNo; }
            set
            {
                System.Diagnostics.Debug.WriteLine("  raw_out_seq_no chat={0} old={1} new={2}", Id, _rawOutSeqNo, value);
                _rawOutSeqNo = value;
            }
        }

        private TLInt _layer;

        public TLInt Layer
        {
            get { return _layer; }
            set
            {
                System.Diagnostics.Debug.WriteLine("  layer chat={0} old={1} new={2}", Id, _layer, value);
                _layer = value;
            }
        }

        public TLBool IsConfirmed { get; set; }

        public override void Update(TLEncryptedChatBase chat)
        {
            base.Update(chat);

            var encryptedChat = chat as TLEncryptedChat17;
            if (encryptedChat != null)
            {
                RawInSeqNo = encryptedChat.RawInSeqNo;
                RawOutSeqNo = encryptedChat.RawOutSeqNo;
                Layer = encryptedChat.Layer;
            }
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GAorB = GetObject<TLString>(input);

            _key = GetNullableObject<TLString>(input);
            KeyFingerprint = GetNullableObject<TLLong>(input);
            P = GetNullableObject<TLString>(input);
            G = GetNullableObject<TLInt>(input);
            A = GetNullableObject<TLString>(input);
            MessageTTL = GetNullableObject<TLInt>(input);
            
            CustomFlags = GetNullableObject<TLLong>(input);
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey))
            {
                OriginalKey = GetObject<TLString>(input);
            }
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey))
            {
                ExtendedKey = GetObject<TLString>(input);
            }

            RawInSeqNo = GetNullableObject<TLInt>(input);
            RawOutSeqNo = GetNullableObject<TLInt>(input);
            Layer = GetNullableObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Date.ToBytes());
            output.Write(AdminId.ToBytes());
            output.Write(ParticipantId.ToBytes());
            output.Write(GAorB.ToBytes());

            Key.NullableToStream(output);
            KeyFingerprint.NullableToStream(output);
            P.NullableToStream(output);
            G.NullableToStream(output);
            A.NullableToStream(output);
            MessageTTL.NullableToStream(output);

            CustomFlags.NullableToStream(output);
            ToStream(output, OriginalKey, CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey);
            ToStream(output, ExtendedKey, CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey);

            RawInSeqNo.NullableToStream(output);
            RawOutSeqNo.NullableToStream(output);
            Layer.NullableToStream(output);
        }

        public override string ToString()
        {
            return string.Format("EncryptedChat17={0} Hash={1}", Index, GetHashCode());
        }
    }

    public class TLEncryptedChat : TLEncryptedChatCommon
    {
        public const uint Signature = TLConstructors.TLEncryptedChat;

        public TLString GAorB { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            base.FromBytes(bytes, ref position);

            GAorB = GetObject<TLString>(bytes, ref position);
            KeyFingerprint = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override void Update(TLEncryptedChatBase chat)
        {
            base.Update(chat);

            var encryptedChat = chat as TLEncryptedChat;
            if (encryptedChat != null)
            {
                GAorB = encryptedChat.GAorB;
                KeyFingerprint = encryptedChat.KeyFingerprint;
            }
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GAorB = GetObject<TLString>(input);

            _key = GetNullableObject<TLString>(input);
            KeyFingerprint = GetNullableObject<TLLong>(input);
            P = GetNullableObject<TLString>(input);
            G = GetNullableObject<TLInt>(input);
            A = GetNullableObject<TLString>(input);
            MessageTTL = GetNullableObject<TLInt>(input);
            
            CustomFlags = GetNullableObject<TLLong>(input);
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey))
            {
                OriginalKey = GetObject<TLString>(input);
            }
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey))
            {
                ExtendedKey = GetObject<TLString>(input);
            }
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Date.ToBytes());
            output.Write(AdminId.ToBytes());
            output.Write(ParticipantId.ToBytes());
            output.Write(GAorB.ToBytes());

            Key.NullableToStream(output);
            KeyFingerprint.NullableToStream(output);
            P.NullableToStream(output);
            G.NullableToStream(output);
            A.NullableToStream(output);
            MessageTTL.NullableToStream(output);

            CustomFlags.NullableToStream(output);
            ToStream(output, OriginalKey, CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey);
            ToStream(output, ExtendedKey, CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey);
        }

        public override string ToString()
        {
            return string.Format("EncryptedChat={0} Hash={1}", Index, GetHashCode());
        }
    }

    public class TLEncryptedChatDiscarded : TLEncryptedChatBase
    {
        public const uint Signature = TLConstructors.TLEncryptedChatDiscarded;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            base.FromBytes(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);

            _key = GetNullableObject<TLString>(input);
            KeyFingerprint = GetNullableObject<TLLong>(input);
            P = GetNullableObject<TLString>(input);
            G = GetNullableObject<TLInt>(input);
            A = GetNullableObject<TLString>(input);
            MessageTTL = GetNullableObject<TLInt>(input);

            CustomFlags = GetNullableObject<TLLong>(input);
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey))
            {
                OriginalKey = GetObject<TLString>(input);
            }
            if (IsSet(CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey))
            {
                ExtendedKey = GetObject<TLString>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());

            Key.NullableToStream(output);
            KeyFingerprint.NullableToStream(output);
            P.NullableToStream(output);
            G.NullableToStream(output);
            A.NullableToStream(output);
            MessageTTL.NullableToStream(output);

            CustomFlags.NullableToStream(output);
            ToStream(output, OriginalKey, CustomFlags, (int)EncryptedChatCustomFlags.OriginalKey);
            ToStream(output, ExtendedKey, CustomFlags, (int)EncryptedChatCustomFlags.ExtendedKey);
        }
    }
}
