// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;

namespace Telegram.Api.Extensions
{
    public static class StreamExtensions
    {
        public static void Write(this Stream output, byte[] buffer)
        {
            output.Write(buffer, 0, buffer.Length);
        }
    }
}
