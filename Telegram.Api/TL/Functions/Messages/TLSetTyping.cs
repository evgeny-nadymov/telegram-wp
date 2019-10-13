// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Messages
{
    public class TLSetTyping : TLObject
    {
        public const string Signature = "#a3825e50";

        public TLInputPeerBase Peer { get; set; }

        public TLSendMessageActionBase Action { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes(),
                Action.ToBytes());
        }
    }
}
