// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL
{
    [Flags]
    public enum FeedSourcesFlags
    {
        NewlyJoinedFeed = 0x1
    }

    public abstract class TLFeedSourcesBase : TLObject { }

    public class TLFeedSourcesNotModified : TLFeedSourcesBase
    {
        public const uint Signature = TLConstructors.TLFeedSourcesNotModified;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }

    public class TLFeedSources : TLFeedSourcesBase
    {
        public const uint Signature = TLConstructors.TLFeedSources;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLInt _newlyJoinedFeed;

        public TLInt NewlyJoinedFeed
        {
            get { return _newlyJoinedFeed; }
            set { SetField(out _newlyJoinedFeed, value, ref _flags, (int)FeedSourcesFlags.NewlyJoinedFeed); }
        }

        public TLVector<TLFeedBroadcastsBase> Feeds { get; set; }

        public TLVector<TLChatBase> Chats { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            _newlyJoinedFeed = GetObject<TLInt>(Flags, (int)FeedSourcesFlags.NewlyJoinedFeed, null, bytes, ref position);
            
            Feeds = GetObject<TLVector<TLFeedBroadcastsBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }
    }
}
