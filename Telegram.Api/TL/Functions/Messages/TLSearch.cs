// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Messages
{
    [Flags]
    public enum SearchFlags
    {
        FromId = 0x1,
    }

    public class TLSearch : TLObject
    {
        public const uint Signature = 0x8614ef68;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputPeerBase Peer { get; set; }

        public TLString Query { get; set; }

        private TLInputUserBase _fromId;

        public TLInputUserBase FromId
        {
            get { return _fromId; }
            set { SetField(out _fromId, value, ref _flags, (int) SearchFlags.FromId); }
        }

        public TLInputMessagesFilterBase Filter { get; set; }

        public TLInt MinDate { get; set; }

        public TLInt MaxDate { get; set; }

        public TLInt OffsetId { get; set; }

        public TLInt AddOffset { get; set; }

        public TLInt Limit { get; set; }

        public TLInt MaxId { get; set; }

        public TLInt MinId { get; set; }

        public TLInt Hash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes(),
                Query.ToBytes(),
                ToBytes(FromId, Flags, (int)SearchFlags.FromId),
                Filter.ToBytes(),
                MinDate.ToBytes(),
                MaxDate.ToBytes(),
                OffsetId.ToBytes(),
                AddOffset.ToBytes(),
                Limit.ToBytes(),
                MaxId.ToBytes(),
                MinId.ToBytes(),
                Hash.ToBytes());
        }
    }

    public class TLSearchGlobal : TLObject
    {
        public const uint Signature = 0x9e3cacb0;

        public TLString Query { get; set; }

        public TLInt OffsetDate { get; set; }

        public TLInputPeerBase OffsetPeer { get; set; }

        public TLInt OffsetId { get; set; }

        public TLInt Limit { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Query.ToBytes(),
                OffsetDate.ToBytes(),
                OffsetPeer.ToBytes(),
                OffsetId.ToBytes(),
                Limit.ToBytes());
        }
    }
}
