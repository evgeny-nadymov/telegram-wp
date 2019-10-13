// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Telegram.Api.TL
{
    [Flags]
    public enum ConfigFlags
    {
        TmpSessions = 0x1,              // 0
        PhoneCallsEnabled = 0x2,        // 1
        Lang = 0x4,                     // 2
        DefaultP2PContacts = 0x8,       // 3
        PreloadFeaturedStickers = 0x10, // 4
        IgnorePhoneEntities = 0x20,     // 5
        RevokePmInbox = 0x40,           // 6
        AutoupdateUrlPrefix = 0x80,     // 7
        BlockedMode = 0x100,            // 8
        GifSearchUsername = 0x200,      // 9
        VenueSearchUsername = 0x400,    // 10
        ImgSearchUsername = 0x800,      // 11
        StaticMapsProvider = 0x1000,    // 12
    }

    [DataContract]
    public class TLConfig : TLObject
    {
        public const uint Signature = TLConstructors.TLConfig;

        [DataMember]
        public TLInt Date { get; set; }

        [DataMember]
        public TLBool TestMode { get; set; }

        /// <summary>
        /// Номер датацентра, ему может соответствовать несколько записей в DCOptions
        /// </summary>
        [DataMember]
        public TLInt ThisDC { get; set; }

        [DataMember]
        public TLVector<TLDCOption> DCOptions { get; set; }

        [DataMember]
        public TLInt ChatSizeMax { get; set; }

        [DataMember]
        public TLInt BroadcastSizeMax { get; set; }

        #region Additional
        /// <summary>
        /// Время последней загрузки config
        /// </summary>
        [DataMember]
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Номер конкретного датацентра внутри списка DCOptions, однозначно определяет текущий датацентр
        /// </summary>
        [DataMember]
        public int ActiveDCOptionIndex { get; set; }

        [DataMember]
        public string Country { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public static TLConfig Merge(TLConfig oldConfig, TLConfig newConfig)
        {
            if (oldConfig == null)
                return newConfig;

            if (newConfig == null)
                return oldConfig;

            foreach (var dcOption in oldConfig.DCOptions)
            {
                if (dcOption.AuthKey != null)
                {
                    var option = dcOption;
                    foreach (var newDCOption in newConfig.DCOptions.Where(x => x.AreEquals(option)))
                    {
                        newDCOption.AuthKey = dcOption.AuthKey;
                        newDCOption.Salt = dcOption.Salt;
                        newDCOption.SessionId = dcOption.SessionId;
                        newDCOption.ClientTicksDelta = dcOption.ClientTicksDelta;
                    }
                }
            }
            if (!string.IsNullOrEmpty(oldConfig.Country))
            {
                newConfig.Country = oldConfig.Country;
            }
            if (oldConfig.ActiveDCOptionIndex != default(int))
            {
                var oldActiveDCOption = oldConfig.DCOptions[oldConfig.ActiveDCOptionIndex];
                var dcId = oldConfig.DCOptions[oldConfig.ActiveDCOptionIndex].Id.Value;
                var ipv6 = oldActiveDCOption.IPv6.Value;
                var media = oldActiveDCOption.Media.Value;

                TLDCOption newActiveDCOption = null;
                int newActiveDCOptionIndex = 0;
                for (var i = 0; i < newConfig.DCOptions.Count; i++)
                {
                    if (newConfig.DCOptions[i].Id.Value == dcId
                        && newConfig.DCOptions[i].IPv6.Value == ipv6
                        && newConfig.DCOptions[i].Media.Value == media)
                    {
                        newActiveDCOption = newConfig.DCOptions[i];
                        newActiveDCOptionIndex = i;
                        break;
                    }
                }

                if (newActiveDCOption == null)
                {
                    for (var i = 0; i < newConfig.DCOptions.Count; i++)
                    {
                        if (newConfig.DCOptions[i].Id.Value == dcId)
                        {
                            newActiveDCOption = newConfig.DCOptions[i];
                            newActiveDCOptionIndex = i;
                            break;
                        }
                    }
                }

                newConfig.ActiveDCOptionIndex = newActiveDCOptionIndex;
            }
            if (oldConfig.LastUpdate != default(DateTime))
            {
                newConfig.LastUpdate = oldConfig.LastUpdate;
            }

            return newConfig;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("BroadcastSizeMax {0}", BroadcastSizeMax));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig23 : TLConfig
    {
        public new const uint Signature = TLConstructors.TLConfig23;

        [DataMember]
        public TLInt Expires { get; set; }

        [DataMember]
        public TLInt ChatBigSize { get; set; }

        [DataMember]
        public TLVector<TLDisabledFeature> DisabledFeatures { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("BroadcastSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig24 : TLConfig23
    {
        public new const uint Signature = TLConstructors.TLConfig24;

        [DataMember]
        public TLInt OnlineUpdatePeriodMs { get; set; }

        [DataMember]
        public TLInt OfflineBlurTimeoutMs { get; set; }

        [DataMember]
        public TLInt OfflineIdleTimeoutMs { get; set; }

        [DataMember]
        public TLInt OnlineCloudTimeoutMs { get; set; }

        [DataMember]
        public TLInt NotifyCloudDelayMs { get; set; }

        [DataMember]
        public TLInt NotifyDefaultDelayMs { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("BroadcastSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig26 : TLConfig24
    {
        public new const uint Signature = TLConstructors.TLConfig26;

        [DataMember]
        public TLInt ForwardedCountMax { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("BroadcastSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig28 : TLConfig26
    {
        public new const uint Signature = TLConstructors.TLConfig28;

        [DataMember]
        public TLInt PushChatPeriodMs { get; set; }

        [DataMember]
        public TLInt PushChatLimit { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("BroadcastSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig41 : TLConfig28
    {
        public new const uint Signature = TLConstructors.TLConfig41;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig44 : TLConfig41
    {
        public new const uint Signature = TLConstructors.TLConfig44;

        [DataMember]
        public TLInt SavedGifsLimit { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig48 : TLConfig44
    {
        public new const uint Signature = TLConstructors.TLConfig48;

        [DataMember]
        public TLInt EditTimeLimit { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig52 : TLConfig48
    {
        public new const uint Signature = TLConstructors.TLConfig52;

        [DataMember]
        public TLInt RatingEDecay { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig54 : TLConfig52
    {
        public new const uint Signature = TLConstructors.TLConfig54;

        [DataMember]
        public TLInt StickersRecentLimit { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig55 : TLConfig54
    {
        public new const uint Signature = TLConstructors.TLConfig55;

        protected TLInt _flags;

        [DataMember]
        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        [DataMember]
        public TLInt TmpSessions { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public static string ConfigFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (ConfigFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig60 : TLConfig55
    {
        public new const uint Signature = TLConstructors.TLConfig60;

        public bool PhoneCallsEnabled { get { return IsSet(Flags, (int)ConfigFlags.PhoneCallsEnabled); } }

        [DataMember]
        public TLInt CallReceiveTimeoutMs { get; set; }

        [DataMember]
        public TLInt CallRingTimeoutMs { get; set; }

        [DataMember]
        public TLInt CallConnectTimeoutMs { get; set; }

        [DataMember]
        public TLInt CallPacketTimeoutMs { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig61 : TLConfig60
    {
        public new const uint Signature = TLConstructors.TLConfig61;

        [DataMember]
        public TLInt PinnedDialogsCountMax { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig63 : TLConfig61
    {
        public new const uint Signature = TLConstructors.TLConfig63;

        [DataMember]
        public TLString MeUrlPrefix { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            MeUrlPrefix = GetObject<TLString>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("MeUrlPrefix {0}", MeUrlPrefix));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig67 : TLConfig63
    {
        public new const uint Signature = TLConstructors.TLConfig67;

        protected TLString _suggestedLangCode;

        [DataMember]
        public TLString SuggestedLangCode
        {
            get { return _suggestedLangCode; }
            set { SetField(out _suggestedLangCode, value, ref _flags, (int)ConfigFlags.Lang); }
        }

        protected TLInt _langPackVersion;

        [DataMember]
        public TLInt LangPackVersion
        {
            get { return _langPackVersion; }
            set { SetField(out _langPackVersion, value, ref _flags, (int)ConfigFlags.Lang); }
        }


        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            MeUrlPrefix = GetObject<TLString>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);
            _suggestedLangCode = GetObject<TLString>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);
            _langPackVersion = GetObject<TLInt>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("MeUrlPrefix {0}", MeUrlPrefix));
            sb.AppendLine(string.Format("SuggestedLangCode {0}", SuggestedLangCode));
            sb.AppendLine(string.Format("LangPackVersion {0}", LangPackVersion));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig71 : TLConfig67
    {
        public new const uint Signature = TLConstructors.TLConfig71;

        [DataMember]
        public TLInt StickersFavedLimit { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            StickersFavedLimit = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            MeUrlPrefix = GetObject<TLString>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);
            _suggestedLangCode = GetObject<TLString>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);
            _langPackVersion = GetObject<TLInt>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("StickersFavedLimit {0}", StickersFavedLimit));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("MeUrlPrefix {0}", MeUrlPrefix));
            sb.AppendLine(string.Format("SuggestedLangCode {0}", SuggestedLangCode));
            sb.AppendLine(string.Format("LangPackVersion {0}", LangPackVersion));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig72 : TLConfig71
    {
        public new const uint Signature = TLConstructors.TLConfig72;

        [DataMember]
        public TLInt ChannelsReadMediaPeriod { get; set; }

        public bool PreloadFeaturedStickers { get { return IsSet(Flags, (int)ConfigFlags.PreloadFeaturedStickers); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = GetObject<TLInt>(bytes, ref position);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            StickersFavedLimit = GetObject<TLInt>(bytes, ref position);
            ChannelsReadMediaPeriod = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            MeUrlPrefix = GetObject<TLString>(bytes, ref position);
            DisabledFeatures = GetObject<TLVector<TLDisabledFeature>>(bytes, ref position);
            _suggestedLangCode = GetObject<TLString>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);
            _langPackVersion = GetObject<TLInt>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("ChatBigSize {0}", ChatBigSize));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("StickersFavedLimit {0}", StickersFavedLimit));
            sb.AppendLine(string.Format("ChannelsReadMediaPeriod {0}", ChannelsReadMediaPeriod));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("MeUrlPrefix {0}", MeUrlPrefix));
            sb.AppendLine(string.Format("SuggestedLangCode {0}", SuggestedLangCode));
            sb.AppendLine(string.Format("LangPackVersion {0}", LangPackVersion));
            sb.AppendLine(string.Format("DisabledFeatures {0}", DisabledFeatures.Count));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig76 : TLConfig72
    {
        public new const uint Signature = TLConstructors.TLConfig76;

        [DataMember]
        public TLInt RevokeTimeLimit { get; set; }

        [DataMember]
        public TLInt RevokePmTimeLimit { get; set; }

        public bool IgnorePhoneEntities { get { return IsSet(Flags, (int)ConfigFlags.IgnorePhoneEntities); } }

        public bool RevokePmInbox { get { return IsSet(Flags, (int)ConfigFlags.RevokePmInbox); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = new TLInt(int.MaxValue);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RevokeTimeLimit = GetObject<TLInt>(bytes, ref position);
            RevokePmTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            StickersFavedLimit = GetObject<TLInt>(bytes, ref position);
            ChannelsReadMediaPeriod = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            MeUrlPrefix = GetObject<TLString>(bytes, ref position);
            DisabledFeatures = new TLVector<TLDisabledFeature>();
            _suggestedLangCode = GetObject<TLString>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);
            _langPackVersion = GetObject<TLInt>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RevokeTimeLimit {0}", RevokeTimeLimit));
            sb.AppendLine(string.Format("RevokePmTimeLimit {0}", RevokePmTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("StickersFavedLimit {0}", StickersFavedLimit));
            sb.AppendLine(string.Format("ChannelsReadMediaPeriod {0}", ChannelsReadMediaPeriod));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("MeUrlPrefix {0}", MeUrlPrefix));
            sb.AppendLine(string.Format("SuggestedLangCode {0}", SuggestedLangCode));
            sb.AppendLine(string.Format("LangPackVersion {0}", LangPackVersion));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig78 : TLConfig76
    {
        public new const uint Signature = TLConstructors.TLConfig78;

        protected TLString _autoupdateUrlPrefix;

        [DataMember]
        public TLString AutoupdateUrlPrefix
        {
            get { return _autoupdateUrlPrefix; }
            set { SetField(out _autoupdateUrlPrefix, value, ref _flags, (int)ConfigFlags.AutoupdateUrlPrefix); }
        }

        public bool BlockedMode { get { return IsSet(Flags, (int)ConfigFlags.BlockedMode); } }

        public bool DefaultP2PContacts { get { return IsSet(Flags, (int)ConfigFlags.DefaultP2PContacts); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = new TLInt(int.MaxValue);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RevokeTimeLimit = GetObject<TLInt>(bytes, ref position);
            RevokePmTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            StickersFavedLimit = GetObject<TLInt>(bytes, ref position);
            ChannelsReadMediaPeriod = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            MeUrlPrefix = GetObject<TLString>(bytes, ref position);
            _autoupdateUrlPrefix = GetObject<TLString>(_flags, (int)ConfigFlags.AutoupdateUrlPrefix, null, bytes, ref position);
            DisabledFeatures = new TLVector<TLDisabledFeature>();
            _suggestedLangCode = GetObject<TLString>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);
            _langPackVersion = GetObject<TLInt>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("PhoneCallsEnabled {0}", PhoneCallsEnabled));
            sb.AppendLine(string.Format("DefaultP2PContacts {0}", DefaultP2PContacts));
            sb.AppendLine(string.Format("PreloadFeaturedStickers {0}", PreloadFeaturedStickers));
            sb.AppendLine(string.Format("IgnorePhoneEntities {0}", IgnorePhoneEntities));
            sb.AppendLine(string.Format("RevokePmInbox {0}", RevokePmInbox));
            sb.AppendLine(string.Format("BlockedMode {0}", BlockedMode));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ThisDC {0}", ThisDC));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RevokeTimeLimit {0}", RevokeTimeLimit));
            sb.AppendLine(string.Format("RevokePmTimeLimit {0}", RevokePmTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("StickersFavedLimit {0}", StickersFavedLimit));
            sb.AppendLine(string.Format("ChannelsReadMediaPeriod {0}", ChannelsReadMediaPeriod));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("MeUrlPrefix {0}", MeUrlPrefix));
            sb.AppendLine(string.Format("AutoupdateUrlPrefix {0}", AutoupdateUrlPrefix));
            sb.AppendLine(string.Format("SuggestedLangCode {0}", SuggestedLangCode));
            sb.AppendLine(string.Format("LangPackVersion {0}", LangPackVersion));

            return sb.ToString();
        }
    }

    [DataContract]
    public class TLConfig82 : TLConfig78
    {
        public new const uint Signature = TLConstructors.TLConfig82;

        [DataMember]
        public TLString DCTxtDomainName { get; set; }

        protected TLString _gifSearchUsername;

        [DataMember]
        public TLString GifSearchUsername
        {
            get { return _gifSearchUsername; }
            set { SetField(out _gifSearchUsername, value, ref _flags, (int)ConfigFlags.GifSearchUsername); }
        }

        protected TLString _venueSearchUsername;

        [DataMember]
        public TLString VenueSearchUsername
        {
            get { return _venueSearchUsername; }
            set { SetField(out _venueSearchUsername, value, ref _flags, (int)ConfigFlags.VenueSearchUsername); }
        }

        protected TLString _imgSearchUsername;

        [DataMember]
        public TLString ImgSearchUsername
        {
            get { return _imgSearchUsername; }
            set { SetField(out _imgSearchUsername, value, ref _flags, (int)ConfigFlags.ImgSearchUsername); }
        }

        protected TLString _staticMapsProvider;

        [DataMember]
        public TLString StaticMapsProvider
        {
            get { return _staticMapsProvider; }
            set { SetField(out _staticMapsProvider, value, ref _flags, (int)ConfigFlags.StaticMapsProvider); }
        }

        [DataMember]
        public TLInt CaptionLengthMax { get; set; }

        [DataMember]
        public TLInt MessageLengthMax { get; set; }

        [DataMember]
        public TLInt WebfileDCId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            TestMode = GetObject<TLBool>(bytes, ref position);
            ThisDC = GetObject<TLInt>(bytes, ref position);
            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);
            DCTxtDomainName = GetObject<TLString>(bytes, ref position);
            ChatSizeMax = GetObject<TLInt>(bytes, ref position);
            BroadcastSizeMax = GetObject<TLInt>(bytes, ref position);   // MegagroupSizeMax
            ForwardedCountMax = GetObject<TLInt>(bytes, ref position);
            OnlineUpdatePeriodMs = GetObject<TLInt>(bytes, ref position);
            OfflineBlurTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OfflineIdleTimeoutMs = GetObject<TLInt>(bytes, ref position);
            OnlineCloudTimeoutMs = GetObject<TLInt>(bytes, ref position);
            NotifyCloudDelayMs = GetObject<TLInt>(bytes, ref position);
            NotifyDefaultDelayMs = GetObject<TLInt>(bytes, ref position);
            ChatBigSize = new TLInt(int.MaxValue);
            PushChatPeriodMs = GetObject<TLInt>(bytes, ref position);
            PushChatLimit = GetObject<TLInt>(bytes, ref position);
            SavedGifsLimit = GetObject<TLInt>(bytes, ref position);
            EditTimeLimit = GetObject<TLInt>(bytes, ref position);
            RevokeTimeLimit = GetObject<TLInt>(bytes, ref position);
            RevokePmTimeLimit = GetObject<TLInt>(bytes, ref position);
            RatingEDecay = GetObject<TLInt>(bytes, ref position);
            StickersRecentLimit = GetObject<TLInt>(bytes, ref position);
            StickersFavedLimit = GetObject<TLInt>(bytes, ref position);
            ChannelsReadMediaPeriod = GetObject<TLInt>(bytes, ref position);
            TmpSessions = GetObject<TLInt>(Flags, (int)ConfigFlags.TmpSessions, null, bytes, ref position);
            PinnedDialogsCountMax = GetObject<TLInt>(bytes, ref position);
            CallReceiveTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallRingTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallConnectTimeoutMs = GetObject<TLInt>(bytes, ref position);
            CallPacketTimeoutMs = GetObject<TLInt>(bytes, ref position);
            MeUrlPrefix = GetObject<TLString>(bytes, ref position);
            _autoupdateUrlPrefix = GetObject<TLString>(_flags, (int)ConfigFlags.AutoupdateUrlPrefix, null, bytes, ref position);
            _gifSearchUsername = GetObject<TLString>(_flags, (int)ConfigFlags.GifSearchUsername, null, bytes, ref position);
            _venueSearchUsername = GetObject<TLString>(_flags, (int)ConfigFlags.VenueSearchUsername, null, bytes, ref position);
            _imgSearchUsername = GetObject<TLString>(_flags, (int)ConfigFlags.ImgSearchUsername, null, bytes, ref position);
            _staticMapsProvider = GetObject<TLString>(_flags, (int)ConfigFlags.StaticMapsProvider, null, bytes, ref position);
            CaptionLengthMax = GetObject<TLInt>(bytes, ref position);
            MessageLengthMax = GetObject<TLInt>(bytes, ref position);
            WebfileDCId = GetObject<TLInt>(bytes, ref position);
            DisabledFeatures = new TLVector<TLDisabledFeature>();
            _suggestedLangCode = GetObject<TLString>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);
            _langPackVersion = GetObject<TLInt>(Flags, (int)ConfigFlags.Lang, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Flags {0}", ConfigFlagsString(Flags)));
            sb.AppendLine(string.Format("PhoneCallsEnabled {0}", PhoneCallsEnabled));
            sb.AppendLine(string.Format("DefaultP2PContacts {0}", DefaultP2PContacts));
            sb.AppendLine(string.Format("PreloadFeaturedStickers {0}", PreloadFeaturedStickers));
            sb.AppendLine(string.Format("IgnorePhoneEntities {0}", IgnorePhoneEntities));
            sb.AppendLine(string.Format("RevokePmInbox {0}", RevokePmInbox));
            sb.AppendLine(string.Format("BlockedMode {0}", BlockedMode));
            sb.AppendLine(string.Format("Date utc0 {0} {1}", Date.Value, TLUtils.ToDateTime(Date).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("Expires utc0 {0} {1}", Expires.Value, TLUtils.ToDateTime(Expires).ToUniversalTime().ToString("HH:mm:ss.fff dd-MM-yyyy")));
            sb.AppendLine(string.Format("TestMode {0}", TestMode));
            sb.AppendLine(string.Format("ThisDC {0}", ThisDC));
            sb.AppendLine(string.Format("DCTxtDomainName {0}", DCTxtDomainName));
            sb.AppendLine(string.Format("ChatSizeMax {0}", ChatSizeMax));
            sb.AppendLine(string.Format("MegagroupSizeMax {0}", BroadcastSizeMax));
            sb.AppendLine(string.Format("ForwardedCountMax {0}", ForwardedCountMax));
            sb.AppendLine(string.Format("OnlineUpdatePeriodMs {0}", OnlineUpdatePeriodMs));
            sb.AppendLine(string.Format("OfflineBlurTimeoutMs {0}", OfflineBlurTimeoutMs));
            sb.AppendLine(string.Format("OfflineIdleTimeoutMs {0}", OfflineIdleTimeoutMs));
            sb.AppendLine(string.Format("OnlineCloudTimeoutMs {0}", OnlineCloudTimeoutMs));
            sb.AppendLine(string.Format("NotifyCloudDelayMs {0}", NotifyCloudDelayMs));
            sb.AppendLine(string.Format("NotifyDefaultDelayMs {0}", NotifyDefaultDelayMs));
            sb.AppendLine(string.Format("PushChatPeriodMs {0}", PushChatPeriodMs));
            sb.AppendLine(string.Format("PushChatLimit {0}", PushChatLimit));
            sb.AppendLine(string.Format("SavedGifsLimit {0}", SavedGifsLimit));
            sb.AppendLine(string.Format("EditTimeLimit {0}", EditTimeLimit));
            sb.AppendLine(string.Format("RevokeTimeLimit {0}", RevokeTimeLimit));
            sb.AppendLine(string.Format("RevokePmTimeLimit {0}", RevokePmTimeLimit));
            sb.AppendLine(string.Format("RatingEDecay {0}", RatingEDecay));
            sb.AppendLine(string.Format("StickersRecentLimit {0}", StickersRecentLimit));
            sb.AppendLine(string.Format("StickersFavedLimit {0}", StickersFavedLimit));
            sb.AppendLine(string.Format("ChannelsReadMediaPeriod {0}", ChannelsReadMediaPeriod));
            sb.AppendLine(string.Format("TmpSessions {0}", TmpSessions));
            sb.AppendLine(string.Format("PinnedDialogsCountMax {0}", PinnedDialogsCountMax));
            sb.AppendLine(string.Format("CallReceiveTimeoutMs {0}", CallReceiveTimeoutMs));
            sb.AppendLine(string.Format("CallRingTimeoutMs {0}", CallRingTimeoutMs));
            sb.AppendLine(string.Format("CallConnectTimeoutMs {0}", CallConnectTimeoutMs));
            sb.AppendLine(string.Format("CallPacketTimeoutMs {0}", CallPacketTimeoutMs));
            sb.AppendLine(string.Format("MeUrlPrefix {0}", MeUrlPrefix));
            sb.AppendLine(string.Format("AutoupdateUrlPrefix {0}", AutoupdateUrlPrefix));
            sb.AppendLine(string.Format("GifSearchUsername {0}", GifSearchUsername));
            sb.AppendLine(string.Format("VenueSearchUsername {0}", VenueSearchUsername));
            sb.AppendLine(string.Format("ImgSearchUsername {0}", ImgSearchUsername));
            sb.AppendLine(string.Format("StaticMapsProvider {0}", StaticMapsProvider));
            sb.AppendLine(string.Format("CaptionLengthMax {0}", CaptionLengthMax));
            sb.AppendLine(string.Format("MessageLengthMax {0}", MessageLengthMax));
            sb.AppendLine(string.Format("WebfileDCId {0}", WebfileDCId));
            sb.AppendLine(string.Format("SuggestedLangCode {0}", SuggestedLangCode));
            sb.AppendLine(string.Format("LangPackVersion {0}", LangPackVersion));

            return sb.ToString();
        }
    }
}
