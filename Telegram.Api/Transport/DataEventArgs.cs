// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.Transport
{
    public class DataEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public DateTime? LastReceiveTime { get; set; }
        public int NextPacketLength { get; set; }

        public DataEventArgs(byte[] data)
        {
            Data = data;
        }

        public DataEventArgs(byte[] data, int packetLength, DateTime? lastReceiveTime)
        {
            Data = data;
            NextPacketLength = packetLength;
            LastReceiveTime = lastReceiveTime;
        }
    }
}