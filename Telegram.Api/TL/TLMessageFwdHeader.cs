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
using Telegram.Api.Services.Cache;

namespace Telegram.Api.TL
{

    [Flags]
    public enum MessageFwdHeaderFlags
    {
        From = 0x1,             // 0
        Channel = 0x2,          // 1
        ChannelPost = 0x4,      // 2
        PostAuthor = 0x8,       // 3
        Saved = 0x10,           // 4
    }

    public class TLMessageFwdHeader : TLObject
    {
        public const uint Signature = TLConstructors.TLMessageFwdHeader;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLInt _fromId;

        public TLInt FromId
        {
            get { return _fromId; }
            set
            {
                if (value != null)
                {
                    _fromId = value;
                    Set(ref _flags, (int)MessageFwdHeaderFlags.From);
                }
                else
                {
                    Unset(ref _flags, (int)MessageFwdHeaderFlags.From);
                }
            }
        }

        public TLInt Date { get; set; }

        private TLInt _channelId;

        public TLInt ChannelId
        {
            get { return _channelId; }
            set
            {
                if (value != null)
                {
                    _channelId = value;
                    Set(ref _flags, (int) MessageFwdHeaderFlags.Channel);
                }
                else
                {
                    Unset(ref _flags, (int)MessageFwdHeaderFlags.Channel);
                }
            }
        }

        private TLInt _channelPost;

        public TLInt ChannelPost
        {
            get { return _channelPost; }
            set
            {
                if (value != null)
                {
                    _channelPost = value;
                    Set(ref _flags, (int)MessageFwdHeaderFlags.ChannelPost);
                }
                else
                {
                    Unset(ref _flags, (int)MessageFwdHeaderFlags.ChannelPost);
                }
            }
        }

        private string _fullName;

        public virtual string FullName
        {
            get
            {
                if (_fullName != null) return _fullName;

                var cacheService = InMemoryCacheService.Instance;

                var channel = ChannelId != null? cacheService.GetChat(ChannelId) : null;
                var user = FromId != null? cacheService.GetUser(FromId) : null;

                if (channel != null && user != null)
                {
                    _fullName = string.Format("{0} ({1})", channel.FullName, user.FullName);
                    return _fullName;
                }

                if (channel != null)
                {
                    _fullName = channel.FullName;
                    return _fullName;
                }

                if (user != null)
                {
                    _fullName = user.FullName2;
                    return _fullName;
                }

                return null;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            if (IsSet(Flags, (int) MessageFwdHeaderFlags.From))
            {
                FromId = GetObject<TLInt>(bytes, ref position);
            }
            Date = GetObject<TLInt>(bytes, ref position);
            if (IsSet(Flags, (int) MessageFwdHeaderFlags.Channel))
            {
                ChannelId = GetObject<TLInt>(bytes, ref position);
            }
            if (IsSet(Flags, (int)MessageFwdHeaderFlags.ChannelPost))
            {
                ChannelPost = GetObject<TLInt>(bytes, ref position);
            }

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            if (IsSet(Flags, (int) MessageFwdHeaderFlags.From))
            {
                FromId = GetObject<TLInt>(input);
            }
            Date = GetObject<TLInt>(input);
            if (IsSet(Flags, (int) MessageFwdHeaderFlags.Channel))
            {
                ChannelId = GetObject<TLInt>(input);
            }
            if (IsSet(Flags, (int)MessageFwdHeaderFlags.ChannelPost))
            {
                ChannelPost = GetObject<TLInt>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, FromId, Flags, (int)MessageFwdHeaderFlags.From);
            Date.ToStream(output);
            ToStream(output, ChannelId, Flags, (int)MessageFwdHeaderFlags.Channel);
            ToStream(output, ChannelPost, Flags, (int)MessageFwdHeaderFlags.ChannelPost);
        }

        public TLPeerBase ToFwdFromPeer()
        {
            if (ChannelId != null)
            {
                return new TLPeerChannel{ Id = ChannelId };
            }

            return new TLPeerUser { Id = FromId };
        }
    }

    public class TLMessageFwdHeader73 : TLMessageFwdHeader70
    {
        public new const uint Signature = TLConstructors.TLMessageFwdHeader73;

        private TLPeerBase _savedFromPeer;

        public TLPeerBase SavedFromPeer
        {
            get { return _savedFromPeer; }
            set { SetField(out _savedFromPeer, value, ref _flags, (int)MessageFwdHeaderFlags.Saved); }
        }

        private TLInt _savedFromMsgId;

        public TLInt SavedFromMsgId
        {
            get { return _savedFromMsgId; }
            set { SetField(out _savedFromMsgId, value, ref _flags, (int)MessageFwdHeaderFlags.Saved); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.From, null, bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            ChannelId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.Channel, null, bytes, ref position);
            ChannelPost = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.ChannelPost, null, bytes, ref position);
            PostAuthor = GetObject<TLString>(Flags, (int)MessageFwdHeaderFlags.PostAuthor, null, bytes, ref position);
            SavedFromPeer = GetObject<TLPeerBase>(Flags, (int)MessageFwdHeaderFlags.Saved, null, bytes, ref position);
            SavedFromMsgId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.Saved, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            FromId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.From, null, input);
            Date = GetObject<TLInt>(input);
            ChannelId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.Channel, null, input);
            ChannelPost = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.ChannelPost, null, input);
            PostAuthor = GetObject<TLString>(Flags, (int)MessageFwdHeaderFlags.PostAuthor, null, input);
            SavedFromPeer = GetObject<TLPeerBase>(Flags, (int)MessageFwdHeaderFlags.Saved, null, input);
            SavedFromMsgId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.Saved, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, FromId, Flags, (int)MessageFwdHeaderFlags.From);
            Date.ToStream(output);
            ToStream(output, ChannelId, Flags, (int)MessageFwdHeaderFlags.Channel);
            ToStream(output, ChannelPost, Flags, (int)MessageFwdHeaderFlags.ChannelPost);
            ToStream(output, PostAuthor, Flags, (int)MessageFwdHeaderFlags.PostAuthor);
            ToStream(output, SavedFromPeer, Flags, (int)MessageFwdHeaderFlags.Saved);
            ToStream(output, SavedFromMsgId, Flags, (int)MessageFwdHeaderFlags.Saved);
        }
    }

    public class TLMessageFwdHeader70 : TLMessageFwdHeader
    {
        public new const uint Signature = TLConstructors.TLMessageFwdHeader70;

        private TLString _postAuthor;

        public TLString PostAuthor
        {
            get { return _postAuthor; }
            set { SetField(out _postAuthor, value, ref _flags, (int) MessageFwdHeaderFlags.PostAuthor); }
        }

        private TLObject _from;

        public TLObject From
        {
            get
            {
                if (_from != null) return _from;

                if (FromId == null && ChannelId == null) return null;

                if (ChannelId != null)
                {
                    var cacheService = InMemoryCacheService.Instance;
                    _from = cacheService.GetChat(ChannelId);
                }
                else if (FromId != null)
                {
                    var cacheService = InMemoryCacheService.Instance;
                    _from = cacheService.GetUser(FromId);
                }

                return _from;
            }
        }

        private string _fullName;

        public override string FullName
        {
            get
            {
                if (_fullName != null) return _fullName;

                var cacheService = InMemoryCacheService.Instance;

                var channel = ChannelId != null ? cacheService.GetChat(ChannelId) : null;
                var user = FromId != null ? cacheService.GetUser(FromId) : null;

                if (channel != null && (user != null || !TLString.IsNullOrEmpty(PostAuthor)))
                {
                    _fullName = string.Format("{0} ({1})", channel.FullName, user != null ? user.FullName : PostAuthor.ToString());
                    return _fullName;
                }

                if (channel != null)
                {
                    _fullName = channel.FullName;
                    return _fullName;
                }

                if (!TLString.IsNullOrEmpty(PostAuthor))
                {
                    return _fullName = PostAuthor.ToString();
                }

                if (user != null)
                {
                    _fullName = user.FullName2;
                    return _fullName;
                }

                return null;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.From, null, bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            ChannelId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.Channel, null, bytes, ref position);
            ChannelPost = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.ChannelPost, null, bytes, ref position);
            PostAuthor = GetObject<TLString>(Flags, (int)MessageFwdHeaderFlags.PostAuthor, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            FromId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.From, null, input);
            Date = GetObject<TLInt>(input);
            ChannelId = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.Channel, null, input);
            ChannelPost = GetObject<TLInt>(Flags, (int)MessageFwdHeaderFlags.ChannelPost, null, input);
            PostAuthor = GetObject<TLString>(Flags, (int)MessageFwdHeaderFlags.PostAuthor, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, FromId, Flags, (int)MessageFwdHeaderFlags.From);
            Date.ToStream(output);
            ToStream(output, ChannelId, Flags, (int)MessageFwdHeaderFlags.Channel);
            ToStream(output, ChannelPost, Flags, (int)MessageFwdHeaderFlags.ChannelPost);
            ToStream(output, PostAuthor, Flags, (int)MessageFwdHeaderFlags.PostAuthor);
        }
    }
}
