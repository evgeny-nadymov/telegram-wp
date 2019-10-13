// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public enum InitConnectionFlags
    {
        Proxy = 0x1
    }

    public class TLInitConnection : TLObject
    {
        public const uint Signature = 0x69796de9;

        public TLInt AppId { get; set; }

        public TLString DeviceModel { get; set; }

        public TLString SystemVersion { get; set; }

        public TLString AppVersion { get; set; }

        public TLString LangCode { get; set; }

        public TLObject Data { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                AppId.ToBytes(),
                DeviceModel.ToBytes(),
                SystemVersion.ToBytes(),
                AppVersion.ToBytes(),
                LangCode.ToBytes(),
                Data.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            AppId.ToStream(output);
            DeviceModel.ToStream(output);
            SystemVersion.ToStream(output);
            AppVersion.ToStream(output);
            LangCode.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            AppId = GetObject<TLInt>(input);
            DeviceModel = GetObject<TLString>(input);
            SystemVersion = GetObject<TLString>(input);
            AppVersion = GetObject<TLString>(input);
            LangCode = GetObject<TLString>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("app_id={0} device_model={1} system_version={2} app_version={3} lang_code={4}", AppId, DeviceModel, SystemVersion, AppVersion, LangCode);
        }
    }

    public class TLInitConnection67 : TLInitConnection
    {
        public new const uint Signature = 0xc7481da6;

        public TLString SystemLangCode { get; set; }

        public TLString LangPack { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                AppId.ToBytes(),
                DeviceModel.ToBytes(),
                SystemVersion.ToBytes(),
                AppVersion.ToBytes(),
                SystemLangCode.ToBytes(),
                LangPack.ToBytes(),
                LangCode.ToBytes(),
                Data.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            AppId.ToStream(output);
            DeviceModel.ToStream(output);
            SystemVersion.ToStream(output);
            AppVersion.ToStream(output);
            SystemLangCode.ToStream(output);
            LangPack.ToStream(output);
            LangCode.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            AppId = GetObject<TLInt>(input);
            DeviceModel = GetObject<TLString>(input);
            SystemVersion = GetObject<TLString>(input);
            AppVersion = GetObject<TLString>(input);
            SystemLangCode = GetObject<TLString>(input);
            LangPack = GetObject<TLString>(input);
            LangCode = GetObject<TLString>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("app_id={0} device_model={1} system_version={2} app_version={3} system_lang_code={4} lang_pack={5} lang_code={6}", AppId, DeviceModel, SystemVersion, AppVersion, SystemLangCode, LangPack, LangCode);
        }
    }

    public class TLInitConnection78 : TLInitConnection67
    {
        public new const uint Signature = 0x785188b8;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLInputClientProxy _proxy;

        public TLInputClientProxy Proxy
        {
            get { return _proxy; }
            set { SetField(out _proxy, value, ref _flags, (int)InitConnectionFlags.Proxy); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                AppId.ToBytes(),
                DeviceModel.ToBytes(),
                SystemVersion.ToBytes(),
                AppVersion.ToBytes(),
                SystemLangCode.ToBytes(),
                LangPack.ToBytes(),
                LangCode.ToBytes(),
                ToBytes(_proxy, _flags, (int)InitConnectionFlags.Proxy),
                Data.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            AppId.ToStream(output);
            DeviceModel.ToStream(output);
            SystemVersion.ToStream(output);
            AppVersion.ToStream(output);
            SystemLangCode.ToStream(output);
            LangPack.ToStream(output);
            LangCode.ToStream(output);
            ToStream(output, _proxy, _flags, (int)InitConnectionFlags.Proxy);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            AppId = GetObject<TLInt>(input);
            DeviceModel = GetObject<TLString>(input);
            SystemVersion = GetObject<TLString>(input);
            AppVersion = GetObject<TLString>(input);
            SystemLangCode = GetObject<TLString>(input);
            LangPack = GetObject<TLString>(input);
            LangCode = GetObject<TLString>(input);
            _proxy = GetObject<TLInputClientProxy>(_flags, (int)InitConnectionFlags.Proxy, null, input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("app_id={0} device_model={1} system_version={2} app_version={3} system_lang_code={4} lang_pack={5} lang_code={6} proxy=[{7}]", AppId, DeviceModel, SystemVersion, AppVersion, SystemLangCode, LangPack, LangCode, Proxy);
        }
    }
}
