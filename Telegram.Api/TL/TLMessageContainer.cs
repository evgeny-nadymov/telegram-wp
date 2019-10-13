// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Telegram.Api.TL
{
    public class TLContainer : TLObject
    {
        public const uint Signature = TLConstructors.TLContainer;

        public List<TLContainerTransportMessage> Messages { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = new List<TLContainerTransportMessage>();

            var length = BitConverter.ToInt32(bytes, position);
            position += 4;
            for (var i = 0; i < length; i++)
            {
                Messages.Add(GetObject<TLContainerTransportMessage>(bytes, ref position));
            }

            return this;
        }

        public override byte[] ToBytes()
        {
            var stream = new MemoryStream();
            foreach (var message in Messages)
            {
                var bytes = message.ToBytes();
                stream.Write(bytes, 0, bytes.Length);
            }

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                BitConverter.GetBytes(Messages.Count),
                stream.ToArray());
        }
    }
}