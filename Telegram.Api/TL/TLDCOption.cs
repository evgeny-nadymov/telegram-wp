// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum DCOptionFlags
    {
        IPv6 = 0x1,
        Media = 0x2,
        TCPO = 0x4,
        CDN = 0x8,
        Static = 0x10,
        Secret = 0x400
    }

    [KnownType(typeof(TLDCOption78))]
    [KnownType(typeof(TLDCOption30))]
    [DataContract]
    public class TLDCOption : TLObject
    {
        public const uint Signature = TLConstructors.TLDCOption;

        [DataMember]
        public TLInt Id { get; set; }

        [DataMember]
        public TLString Hostname { get; set; }

        private TLString _ipAddress;

        [DataMember]
        public TLString IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        [DataMember]
        public TLInt Port { get; set; }

        #region Additional
        public TLLong CustomFlags { get; set; }

        [DataMember]
        public byte[] AuthKey { get; set; }

        [DataMember]
        public bool IsAuthorized { get; set; }

        [DataMember]
        public TLLong Salt { get; set; }

        [DataMember]
        public long ClientTicksDelta { get; set; }

        //[DataMember] //Important this field initialize with random value on each app startup to avoid TLBadMessage result with 32, 33 code (incorrect MsgSeqNo)
        public TLLong SessionId { get; set; }

        public virtual TLBool IPv6
        {
            get { return TLBool.False; }
            set { }
        }

        public virtual TLBool Media
        {
            get { return TLBool.False; }
            set { }
        }

        public virtual TLBool TCPO
        {
            get { return TLBool.False; }
            set { }
        }

        public virtual TLBool CDN
        {
            get { return TLBool.False; }
            set { }
        }

        public virtual TLBool Static
        {
            get { return TLBool.False; }
            set { }
        }

        public bool IsValidIPv4Option(TLInt dcId)
        {
            return !IPv6.Value && Id != null && Id.Value == dcId.Value;
        }

        public virtual bool IsValidIPv4WithTCPO25Option(TLInt dcId)
        {
            return false;
        }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Hostname = GetObject<TLString>(bytes, ref position);
            IpAddress = GetObject<TLString>(bytes, ref position);
            Port = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Hostname.ToStream(output);
            IpAddress.ToStream(output);
            Port.ToStream(output);

            CustomFlags.NullableToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            Hostname = GetObject<TLString>(input);
            IpAddress = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public bool AreEquals(TLDCOption dcOption)
        {
            if (dcOption == null) return false;

            return Id.Value == dcOption.Id.Value;
        }

        public override string ToString()
        {
            return string.Format("{0}) {1}:{2} (AuthKey {3})\n  Salt {4} TicksDelta {5}", Id, IpAddress, Port, AuthKey != null, Salt, ClientTicksDelta);
        }

        protected string AuthKeySignature(byte[] authKey)
        {
            if (authKey == null || authKey.Length == 0) return "null";

            return string.Join(" ", AuthKey.Take(7).ToArray());
        }
    }

    [DataContract]
    public class TLDCOption30 : TLDCOption
    {
        public new const uint Signature = TLConstructors.TLDCOption30;

        protected TLInt _flags;

        [DataMember]
        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool IPv6
        {
            get { return new TLBool(IsSet(_flags, (int)DCOptionFlags.IPv6)); }
            set { SetUnset(ref _flags, value.Value, (int)DCOptionFlags.IPv6); }
        }

        public override TLBool Media
        {
            get { return new TLBool(IsSet(_flags, (int)DCOptionFlags.Media)); }
            set { SetUnset(ref _flags, value.Value, (int)DCOptionFlags.Media); }
        }

        public override TLBool TCPO
        {
            get { return new TLBool(IsSet(_flags, (int)DCOptionFlags.TCPO)); }
            set { SetUnset(ref _flags, value.Value, (int)DCOptionFlags.TCPO); }
        }

        public override TLBool CDN
        {
            get { return new TLBool(IsSet(_flags, (int)DCOptionFlags.CDN)); }
            set { SetUnset(ref _flags, value.Value, (int)DCOptionFlags.CDN); }
        }

        public override TLBool Static
        {
            get { return new TLBool(IsSet(_flags, (int)DCOptionFlags.Static)); }
            set { SetUnset(ref _flags, value.Value, (int)DCOptionFlags.Static); }
        }

        public static string DCOptionFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (DCOptionFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            //Hostname = GetObject<TLString>(bytes, ref position);
            IpAddress = GetObject<TLString>(bytes, ref position);
            Port = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            //Hostname.ToStream(output);
            IpAddress.ToStream(output);
            Port.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            //Hostname = GetObject<TLString>(input);
            IpAddress = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("{0}) {1}:{2} (AuthKey {3} IsAuthorized={7})\n Flags {6}  Salt {4} TicksDelta {5}", Id, IpAddress, Port, AuthKeySignature(AuthKey), Salt, ClientTicksDelta, DCOptionFlagsString(Flags), IsAuthorized);
        }
    }

    [DataContract]
    public class TLDCOption78 : TLDCOption30
    {
        public new const uint Signature = TLConstructors.TLDCOption78;

        protected TLString _secret;

        [DataMember]
        public TLString Secret
        {
            get { return _secret; }
            set { SetField(out _secret, value, ref _flags, (int)DCOptionFlags.Secret); }
        }

        public override bool IsValidIPv4WithTCPO25Option(TLInt dcId)
        {
            return IsValidIPv4Option(dcId) && !TLString.IsNullOrEmpty(Secret) && Secret.Data.Length == 16;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            IpAddress = GetObject<TLString>(bytes, ref position);
            Port = GetObject<TLInt>(bytes, ref position);
            _secret = GetObject<TLString>(Flags, (int)DCOptionFlags.Secret, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            IpAddress.ToStream(output);
            Port.ToStream(output);
            ToStream(output, _secret, _flags, (int)DCOptionFlags.Secret);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            IpAddress = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);
            _secret = GetObject<TLString>(Flags, (int)DCOptionFlags.Secret, null, input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("{0}) {1}:{2} (AuthKey {3} IsAuthorized={7})\n Flags {6}  Salt {4} TicksDelta {5}", Id, IpAddress, Port, AuthKeySignature(AuthKey), Salt, ClientTicksDelta, DCOptionFlagsString(Flags), IsAuthorized);
        }
    }
}