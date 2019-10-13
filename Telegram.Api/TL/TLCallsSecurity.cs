// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum CallsSecurityFlags
    {
        PeerToPeer = 0x1,

        PeerToPeerEverybody = 0x4,
        PeerToPeerContacts = 0x8,
        PeetToPeerNobody = 0x10
    }

    public class TLCallsSecurity : TLObject
    {
        public const uint Signature = TLConstructors.TLCallsSecurity;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool PeerToPeer
        {
            get { return IsSet(Flags, (int) CallsSecurityFlags.PeerToPeer); }
            set { SetUnset(ref _flags, value, (int) CallsSecurityFlags.PeerToPeer); }
        }

        public bool PeerToPeerEverybody
        {
            get { return IsSet(Flags, (int)CallsSecurityFlags.PeerToPeerEverybody); }
            set { SetUnset(ref _flags, value, (int)CallsSecurityFlags.PeerToPeerEverybody); }
        }

        public bool PeerToPeerContacts
        {
            get { return IsSet(Flags, (int)CallsSecurityFlags.PeerToPeerContacts); }
            set { SetUnset(ref _flags, value, (int)CallsSecurityFlags.PeerToPeerContacts); }
        }

        public bool PeerToPeerNobody
        {
            get { return IsSet(Flags, (int)CallsSecurityFlags.PeetToPeerNobody); }
            set { SetUnset(ref _flags, value, (int)CallsSecurityFlags.PeetToPeerNobody); }
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
        }

        public void Update(bool defaultP2PContacts)
        {
            var updated = PeerToPeerEverybody || PeerToPeerContacts || PeerToPeerNobody;

            if (!updated)
            {
                if (PeerToPeer)
                {
                    PeerToPeerEverybody = false;
                    PeerToPeerContacts = defaultP2PContacts;
                    PeerToPeerNobody = !defaultP2PContacts;
                }
                else
                {
                    PeerToPeerEverybody = false;
                    PeerToPeerContacts = false;
                    PeerToPeerNobody = true;
                }
            }
        }
    }
}
