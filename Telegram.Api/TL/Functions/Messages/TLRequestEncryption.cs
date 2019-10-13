// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Helpers;
using Telegram.Logs;

namespace Telegram.Api.TL.Functions.Messages
{
    public class TLRequestEncryption : TLObject
    {
        public const string Signature = "#f64daf43";

        public TLInputUserBase UserId { get; set; }

        public TLInt RandomId { get; set; }

        public TLString G_A { get; set; }

        public override byte[] ToBytes()
        {
            byte[] bytes = null;
            try
            {
                bytes = TLUtils.Combine(
                    TLUtils.SignatureToBytes(Signature),
                    UserId.ToBytes(),
                    RandomId.ToBytes(),
                    G_A.ToBytes());
            }
            catch (Exception ex)
            {
                var str = "TLRequestEncryption.ToBytes error user_id=" + UserId + " random_id=" + RandomId + " g_a=" + G_A;

                Log.Write(str);

                Execute.ShowDebugMessage(ex.ToString());
            }

            return bytes;
        }
    }
}
