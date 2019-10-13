// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Account
{
    [Flags]
    public enum InitTakeoutSessionFlags
    {
        Contacts = 0x1,                 // 0
        MessageUsers = 0x2,             // 1
        MessageChats = 0x4,             // 2
        MessageMegagroups = 0x8,        // 3
        MessageChannels = 0x10,         // 4
        Files = 0x20,                   // 5
    }

    class TLInitTakeoutSession : TLObject
    {
        public const uint Signature = 0x768a4999;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Contacts
        {
            get { return IsSet(_flags, (int)InitTakeoutSessionFlags.Contacts); }
            set { SetUnset(ref _flags, value, (int)InitTakeoutSessionFlags.Contacts); }
        }

        public bool MessageUsers
        {
            get { return IsSet(_flags, (int)InitTakeoutSessionFlags.MessageUsers); }
            set { SetUnset(ref _flags, value, (int)InitTakeoutSessionFlags.MessageUsers); }
        }

        public bool MessageChats
        {
            get { return IsSet(_flags, (int)InitTakeoutSessionFlags.MessageChats); }
            set { SetUnset(ref _flags, value, (int)InitTakeoutSessionFlags.MessageChats); }
        }

        public bool MessageMegagroups
        {
            get { return IsSet(_flags, (int)InitTakeoutSessionFlags.MessageMegagroups); }
            set { SetUnset(ref _flags, value, (int)InitTakeoutSessionFlags.MessageMegagroups); }
        }

        public bool MessageChannels
        {
            get { return IsSet(_flags, (int)InitTakeoutSessionFlags.MessageChannels); }
            set { SetUnset(ref _flags, value, (int)InitTakeoutSessionFlags.MessageChannels); }
        }

        public bool Files
        {
            get { return IsSet(_flags, (int)InitTakeoutSessionFlags.Files); }
            set { SetUnset(ref _flags, value, (int)InitTakeoutSessionFlags.Files); }
        }

        private TLInt _fileMaxSize;

        public TLInt FileMaxSize
        {
            get { return _fileMaxSize; }
            set { SetField(out _fileMaxSize, value, ref _flags, (int)InitTakeoutSessionFlags.Files); }
        }

        public TLLong TakeoutId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(_fileMaxSize, Flags, (int)InitTakeoutSessionFlags.Files),
                TakeoutId.ToBytes());
        }
    }
}
