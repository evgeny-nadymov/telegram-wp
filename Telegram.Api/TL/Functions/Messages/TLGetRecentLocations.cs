// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Messages
{
    class TLGetRecentLocations : TLObject
    {
        public const uint Signature = 0xbbc45b09;

        public TLInputPeerBase Peer { get; set; }

        public TLInt Limit { get; set; }

        public TLInt Hash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes(),
                Limit.ToBytes(),
                Hash.ToBytes());
        }
    }
}
