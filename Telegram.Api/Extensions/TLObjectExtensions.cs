// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.TL;

namespace Telegram.Api.Extensions
{
    public static class TLObjectExtensions
    {
        public static void NullableToStream(this TLObject obj, Stream output)
        {
            if (obj == null)
            {
                output.Write(new TLNull().ToBytes());
            }
            else
            {
                obj.ToStream(output);
            }
        }

        public static T NullableFromStream<T>(Stream input) where T : TLObject
        {
            var obj = TLObjectGenerator.GetNullableObject<T>(input);
            
            if (obj == null) return null;

            return (T)obj.FromStream(input);
        }
    }
}
