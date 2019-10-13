// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define MTPROTO
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Telegram.Api.TL.Functions.Channels;
using Telegram.Api.TL.Functions.Messages;
using Telegram.Logs;
using Telegram.Api.Helpers;
using Telegram.Api.Services;

namespace Telegram.Api.TL
{
    public static partial class TLUtils
    {
        public static TLInputClientProxy GetInputProxy(TLProxyConfigBase proxyConfig)
        {
            TLInputClientProxy inputProxy = null;
            if (proxyConfig != null && !proxyConfig.IsEmpty && proxyConfig.IsEnabled.Value)
            {
                var proxy = proxyConfig.GetProxy();
                inputProxy = proxy != null ? proxy.ToInputProxy() : null;
            }

            return inputProxy;
        }

        public static TLDCOption GetDCOption(TLConfig config, TLInt dcId, bool media = false)
        {
            TLDCOption dcOption = null;
            if (config != null)
            {
                dcOption = config.DCOptions.FirstOrDefault(x => x.IsValidIPv4WithTCPO25Option(dcId) && x.Media.Value == media);
                if (dcOption == null)
                {
                    dcOption = config.DCOptions.FirstOrDefault(x => x.IsValidIPv4Option(dcId) && x.Media.Value == media);
                }
            }

            return dcOption;
        }

        public static short GetProtocolDCId(int dcId, bool media, bool testServer)
        {
            dcId = testServer ? 10000 + dcId : dcId;

            return media ? (short)-dcId : (short)dcId;
        }

        public static byte[] ParseSecret(TLDCOption dcOption)
        {
            var dcOption78 = dcOption as TLDCOption78;
            if (dcOption78 != null && !TLString.IsNullOrEmpty(dcOption78.Secret))
            {
                return dcOption78.Secret.Data; //ParseSecret(dcOption78.Secret);
            }

            return null;
        }

        public static byte[] ParseSecret(TLString str)
        {
            if (TLString.IsNullOrEmpty(str)) return null;

            var hexStr = str.ToString().ToLowerInvariant();
            for (var i = 0; i < hexStr.Length; i++)
            {
                if (hexStr[i] >= '0' && hexStr[i] <= '9')
                {
                    continue;
                }
                if (hexStr[i] >= 'a' && hexStr[i] <= 'f')
                {
                    continue;
                }

                return null;
            }

            var bytes = new byte[hexStr.Length / 2];
            for (var i = 0; i < hexStr.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexStr.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string ContentTypeToFileExt(TLString contentType)
        {
            if (contentType != null)
            {
                if (TLString.Equals(contentType, new TLString("image/jpg"), StringComparison.OrdinalIgnoreCase))
                {
                    return ".jpg";
                }
                if (TLString.Equals(contentType, new TLString("video/mp4"), StringComparison.OrdinalIgnoreCase))
                {
                    return ".mp4";
                }
            }

            return null;
        }

        public static TLInt ToTLInt(TLString value)
        {
            try
            {
                var intValue = Convert.ToInt32(value.Value);
                return new TLInt(intValue);
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public static TLString ToTLString(TLInt value)
        {
            try
            {
                var intValue = value.Value.ToString(CultureInfo.InvariantCulture);
                return new TLString(intValue);
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public static int GetTopPeersHash(TLTopPeers topPeers)
        {
            long acc = 0;
            foreach (var category in topPeers.Categories)
            {
                foreach (var topPeer in category.Peers)
                {
                    acc = ((acc * 20261) + 0x80000000 + topPeer.Peer.Id.Value) % 0x80000000;
                }
            }

            return (int)acc;
        }

        public static int GetAllStickersHash(IList<TLStickerSetBase> sets)
        {
            long acc = 0;
            foreach (var set in sets)
            {
                var stickerSet32 = set as TLStickerSet32;
                if (stickerSet32 != null)
                {
                    if (stickerSet32.Archived) continue;

                    acc = ((acc * 20261) + 0x80000000 + stickerSet32.Hash.Value) % 0x80000000;
                }
            }

            return (int)acc;
        }

        public static int GetFeaturedStickersHash(IList<TLStickerSetBase> sets, IList<TLLong> unreadSets)
        {
            long acc = 0;
            foreach (var set in sets)
            {
                var stickerSet32 = set as TLStickerSet32;
                if (stickerSet32 != null)
                {
                    if (stickerSet32.Archived) continue;

                    var high_id = (int)(stickerSet32.Id.Value >> 32);
                    var low_id = (int)stickerSet32.Id.Value;

                    acc = ((acc * 20261) + 0x80000000L + high_id) % 0x80000000L;
                    acc = ((acc * 20261) + 0x80000000L + low_id) % 0x80000000L;

                    if (unreadSets.FirstOrDefault(x => x.Value == stickerSet32.Id.Value) != null)
                    {
                        acc = ((acc * 20261) + 0x80000000L + 1) % 0x80000000L;
                    }
                }
            }

            return (int)acc;
        }

        public static TLInt GetFavedStickersHash(IList<TLDocumentBase> documents)
        {
            long acc = 0;
            foreach (var documentBase in documents)
            {
                var document = documentBase as TLDocument;
                if (document == null) continue;

                var high_id = (int)(document.Id.Value >> 32);
                var low_id = (int)document.Id.Value;

                acc = ((acc * 20261) + 0x80000000L + high_id) % 0x80000000L;
                acc = ((acc * 20261) + 0x80000000L + low_id) % 0x80000000L;
            }

            return new TLInt((int)acc);
        }

        public static TLInt GetRecentStickersHash(IList<TLDocumentBase> documents)
        {
            long acc = 0;
            foreach (var documentBase in documents)
            {
                var document = documentBase as TLDocument;
                if (document == null) continue;

                var high_id = (int)(document.Id.Value >> 32);
                var low_id = (int)document.Id.Value;

                acc = ((acc * 20261) + 0x80000000L + high_id) % 0x80000000L;
                acc = ((acc * 20261) + 0x80000000L + low_id) % 0x80000000L;
            }

            return new TLInt((int)acc);
        }

        public static int GetContactsHash(TLInt savedCount, IList<TLUserBase> contacts)
        {
            savedCount = savedCount ?? new TLInt(0);
            if (contacts == null)
            {
                return 0;
            }

            long acc = 0;
            acc = ((acc * 20261) + 0x80000000L + savedCount.Value) % 0x80000000L;

            foreach (var contact in contacts)
            {
                if (contact == null) continue;

                acc = ((acc * 20261) + 0x80000000L + contact.Id.Value) % 0x80000000L;
            }

            return (int)acc;
        }

        public static void AddStickerSetCovered(IStickers stickers, TLMessagesStickerSet messagesSet, TLVector<TLStickerSetCoveredBase> coveredSets, TLStickerSetCoveredBase coveredSet)
        {
            coveredSets.Insert(0, coveredSet);
            //stickers.SetsCovered.Insert(0, messagesSet.Set);

            stickers.Hash = new TLString(GetAllStickersHash(stickers.Sets).ToString(CultureInfo.InvariantCulture));

            var packsDict = new Dictionary<string, TLStickerPack>();
            for (var i = 0; i < messagesSet.Packs.Count; i++)
            {
                packsDict[messagesSet.Packs[i].Emoticon.ToString()] = messagesSet.Packs[i];
            }

            for (var i = 0; i < messagesSet.Packs.Count; i++)
            {
                TLStickerPack pack;
                if (packsDict.TryGetValue(messagesSet.Packs[i].Emoticon.ToString(), out pack))
                {
                    for (var j = messagesSet.Packs[i].Documents.Count - 1; j >= 0; j--)
                    {
                        pack.Documents.Insert(0, messagesSet.Packs[i].Documents[j]);
                    }
                }
                else
                {
                    stickers.Packs.Insert(0, messagesSet.Packs[i]);
                }
            }

            for (var i = messagesSet.Documents.Count - 1; i >= 0; i--)
            {
                stickers.Documents.Insert(0, messagesSet.Documents[i]);
            }
        }

        public static void AddStickerSet(IStickers stickers, TLMessagesStickerSet messagesSet)
        {
            stickers.Sets.Insert(0, messagesSet.Set);

            stickers.Hash = new TLString(GetAllStickersHash(stickers.Sets).ToString(CultureInfo.InvariantCulture));

            var packsDict = new Dictionary<string, TLStickerPack>();
            for (var i = 0; i < messagesSet.Packs.Count; i++)
            {
                packsDict[messagesSet.Packs[i].Emoticon.ToString()] = messagesSet.Packs[i];
            }

            for (var i = 0; i < messagesSet.Packs.Count; i++)
            {
                TLStickerPack pack;
                if (packsDict.TryGetValue(messagesSet.Packs[i].Emoticon.ToString(), out pack))
                {
                    for (var j = messagesSet.Packs[i].Documents.Count - 1; j >= 0; j--)
                    {
                        pack.Documents.Insert(0, messagesSet.Packs[i].Documents[j]);
                    }
                }
                else
                {
                    stickers.Packs.Insert(0, messagesSet.Packs[i]);
                }
            }

            for (var i = messagesSet.Documents.Count - 1; i >= 0; i--)
            {
                stickers.Documents.Insert(0, messagesSet.Documents[i]);
            }
        }

        public static TLMessagesStickerSet RemoveStickerSetCovered(IStickers stickers, TLStickerSetBase set, TLVector<TLStickerSetCoveredBase> coveredSets)
        {
            TLStickerSetCoveredBase s = null;
            for (var i = 0; i < coveredSets.Count; i++)
            {
                if (coveredSets[i].StickerSet.Id.Value == set.Id.Value)
                {
                    s = coveredSets[i];
                    coveredSets.RemoveAt(i);
                    break;
                }
            }

            var s32 = s.StickerSet as TLStickerSet32;
            var set32 = set as TLStickerSet32;
            if (s32 != null && set32 != null)
            {
                s32.Flags = set32.Flags;
            }

            stickers.Hash = new TLString(GetAllStickersHash(stickers.Sets).ToString(CultureInfo.InvariantCulture));

            var documents = new TLVector<TLDocumentBase>();
            var documentsDict = new Dictionary<long, long>();
            for (var i = 0; i < stickers.Documents.Count; i++)
            {
                var document = stickers.Documents[i] as TLDocument54;
                if (document != null)
                {
                    var inputStickerSetId = document.StickerSet as TLInputStickerSetId;
                    if (inputStickerSetId != null && inputStickerSetId.Id.Value == set.Id.Value)
                    {
                        documents.Add(document);
                        documentsDict[document.Id.Value] = document.Id.Value;

                        stickers.Documents.RemoveAt(i--);
                        continue;
                    }

                    var inputStickerSetShortName = document.StickerSet as TLInputStickerSetShortName;
                    if (inputStickerSetShortName != null && inputStickerSetShortName.ShortName.ToString() == set.ShortName.ToString())
                    {
                        documents.Add(document);
                        documentsDict[document.Id.Value] = document.Id.Value;

                        stickers.Documents.RemoveAt(i--);
                    }
                }
            }

            var packs = new TLVector<TLStickerPack>();
            for (var i = 0; i < stickers.Packs.Count; i++)
            {
                var pack = new TLStickerPack { Emoticon = stickers.Packs[i].Emoticon, Documents = new TLVector<TLLong>() };

                for (var j = 0; j < stickers.Packs[i].Documents.Count; j++)
                {
                    if (documentsDict.ContainsKey(stickers.Packs[i].Documents[j].Value))
                    {
                        pack.Documents.Add(stickers.Packs[i].Documents[j]);
                        stickers.Packs[i].Documents.RemoveAt(j--);
                    }
                }

                if (pack.Documents.Count > 0)
                {
                    packs.Add(pack);
                }

                if (stickers.Packs[i].Documents.Count == 0)
                {
                    stickers.Packs.RemoveAt(i--);
                }
            }

            return new TLMessagesStickerSet { Set = s.StickerSet, Packs = packs, Documents = documents };
        }

        public static TLMessagesStickerSet RemoveStickerSet(IStickers stickers, TLStickerSetBase set)
        {
            TLStickerSetBase s = null;
            for (var i = 0; i < stickers.Sets.Count; i++)
            {
                if (stickers.Sets[i].Id.Value == set.Id.Value)
                {
                    s = stickers.Sets[i];
                    stickers.Sets.RemoveAt(i);
                    break;
                }
            }

            var s32 = s as TLStickerSet32;
            var set32 = set as TLStickerSet32;
            if (s32 != null && set32 != null)
            {
                s32.Flags = set32.Flags;
            }

            stickers.Hash = new TLString(GetAllStickersHash(stickers.Sets).ToString(CultureInfo.InvariantCulture));

            var documents = new TLVector<TLDocumentBase>();
            var documentsDict = new Dictionary<long, long>();
            for (var i = 0; i < stickers.Documents.Count; i++)
            {
                var document = stickers.Documents[i] as TLDocument54;
                if (document != null)
                {
                    var inputStickerSetId = document.StickerSet as TLInputStickerSetId;
                    if (inputStickerSetId != null && inputStickerSetId.Id.Value == set.Id.Value)
                    {
                        documents.Add(document);
                        documentsDict[document.Id.Value] = document.Id.Value;

                        stickers.Documents.RemoveAt(i--);
                        continue;
                    }

                    var inputStickerSetShortName = document.StickerSet as TLInputStickerSetShortName;
                    if (inputStickerSetShortName != null && inputStickerSetShortName.ShortName.ToString() == set.ShortName.ToString())
                    {
                        documents.Add(document);
                        documentsDict[document.Id.Value] = document.Id.Value;

                        stickers.Documents.RemoveAt(i--);
                    }
                }
            }

            var packs = new TLVector<TLStickerPack>();
            for (var i = 0; i < stickers.Packs.Count; i++)
            {
                var pack = new TLStickerPack { Emoticon = stickers.Packs[i].Emoticon, Documents = new TLVector<TLLong>() };

                for (var j = 0; j < stickers.Packs[i].Documents.Count; j++)
                {
                    if (documentsDict.ContainsKey(stickers.Packs[i].Documents[j].Value))
                    {
                        pack.Documents.Add(stickers.Packs[i].Documents[j]);
                        stickers.Packs[i].Documents.RemoveAt(j--);
                    }
                }

                if (pack.Documents.Count > 0)
                {
                    packs.Add(pack);
                }

                if (stickers.Packs[i].Documents.Count == 0)
                {
                    stickers.Packs.RemoveAt(i--);
                }
            }

            return new TLMessagesStickerSet { Set = s ?? set32, Packs = packs, Documents = documents };
        }

        public static bool IsValidAction(TLObject obj)
        {
            var readChannelHistoryAction = obj as TLReadChannelHistory;
            if (readChannelHistoryAction != null)
            {
                return true;
            }

            var readHistoryAction = obj as TLReadHistory;
            if (readHistoryAction != null)
            {
                return true;
            }

            var readMessageContents = obj as Functions.Messages.TLReadMessageContents;
            if (readMessageContents != null)
            {
                return true;
            }

            var readChannelMessageContents = obj as Functions.Channels.TLReadMessageContents;
            if (readChannelMessageContents != null)
            {
                return true;
            }

            var sendMessageAction = obj as TLSendMessage;
            if (sendMessageAction != null)
            {
                return true;
            }

            var sendMediaAction = obj as TLSendMedia;
            if (sendMediaAction != null)
            {
                var mediaGame = sendMediaAction.Media as TLInputMediaGame;
                if (mediaGame != null)
                {
                    return true;
                }

                var mediaContact = sendMediaAction.Media as TLInputMediaContact;
                if (mediaContact != null)
                {
                    return true;
                }

                var mediaGeoPoint = sendMediaAction.Media as TLInputMediaGeoPoint;
                if (mediaGeoPoint != null)
                {
                    return true;
                }

                var mediaVenue = sendMediaAction.Media as TLInputMediaVenue;
                if (mediaVenue != null)
                {
                    return true;
                }
            }

            var forwardMessagesAction = obj as TLForwardMessages;
            if (forwardMessagesAction != null)
            {
                return true;
            }

            var forwardMessageAction = obj as TLForwardMessage;
            if (forwardMessageAction != null)
            {
                return true;
            }

            var startBotAction = obj as TLStartBot;
            if (startBotAction != null)
            {
                return true;
            }

            var sendInlineBotResult = obj as TLSendInlineBotResult;
            if (sendInlineBotResult != null)
            {
                return true;
            }

            var sendEncrypted = obj as TLSendEncrypted;
            if (sendEncrypted != null)
            {
                return true;
            }

            var sendEncryptedFile = obj as TLSendEncryptedFile;
            if (sendEncryptedFile != null)
            {
                return true;
            }

            var sendEncryptedService = obj as TLSendEncryptedService;
            if (sendEncryptedService != null)
            {
                return true;
            }

            var readEncryptedHistory = obj as TLReadEncryptedHistory;
            if (readEncryptedHistory != null)
            {
                return true;
            }

            return false;
        }

        public static TLMessage48 GetShortMessage(
            TLInt id,
            TLInt fromId,
            TLPeerBase toId,
            TLInt date,
            TLString message)
        {

            var m = new TLMessage73
            {
                Flags = new TLInt(0),
                Id = id,
                FromId = fromId,
                ToId = toId,
                Out = TLBool.False,
                _date = date,
                Message = message,
                _media = new TLMessageMediaEmpty()
            };

            if (m.FromId != null) m.SetFromId();
            if (m._media != null) m.SetMedia();

            return m;
        }

        public static TLMessage36 GetMessage(
            TLInt fromId,
            TLPeerBase toId,
            MessageStatus status,
            TLBool outFlag,
            TLBool unreadFlag,
            TLInt date,
            TLString message,
            TLMessageMediaBase media,
            TLLong randomId,
            TLInt replyToMsgId)
        {
            var m = new TLMessage73
            {
                Flags = new TLInt(0),
                FromId = fromId,
                ToId = toId,
                _status = status,
                Out = outFlag,
                Unread = unreadFlag,
                _date = date,
                Message = message,
                _media = media,
                RandomId = randomId,
                ReplyToMsgId = replyToMsgId
            };

            if (m.FromId != null) m.SetFromId();
            if (m._media != null) m.SetMedia();
            if (m.ReplyToMsgId != null && m.ReplyToMsgId.Value != 0) m.SetReply();

            return m;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (((a == null) || (b == null)) || (a.Length != b.Length))
            {
                return false;
            }
            var flag = true;
            for (var i = 0; i < a.Length; i++)
            {
                flag &= a[i] == b[i];
            }
            return flag;
        }

        public static IList<TLInt> GetPtsRange(IMultiPts multiPts)
        {
            var pts = multiPts.Pts;
            var ptsCount = multiPts.PtsCount;

            return GetPtsRange(pts, ptsCount);
        }

        public static IList<TLInt> GetPtsRange(TLInt pts, TLInt ptsCount)
        {
            var ptsList = new List<TLInt>(ptsCount.Value);
            for (var i = ptsCount.Value - 1; i >= 0; i--)
            {
                ptsList.Add(new TLInt(pts.Value - i));
            }

            return ptsList;
        }

        public static bool IsDisplayedDecryptedMessage(TLDecryptedMessageBase message, bool displayEmpty = false)
        {
            if (message == null) return false;

#if DEBUG
            //return true;
#endif
            return IsDisplayedDecryptedMessageInternal(message, displayEmpty);
        }

        public static bool CheckPrime(byte[] prime, int g)
        {
            if (!(g >= 2 && g <= 7))
            {
                return false;
            }

            if (prime.Length != 256 || prime[0] <= 127)
            {
                return false;
            }

            var dhBI = new BigInteger(1, prime);

            if (g == 2)
            { // p mod 8 = 7 for g = 2;
                var res = dhBI.Mod(BigInteger.ValueOf(8));
                if (res.IntValue != 7)
                {
                    return false;
                }
            }
            else if (g == 3)
            { // p mod 3 = 2 for g = 3;
                var res = dhBI.Mod(BigInteger.ValueOf(3));
                if (res.IntValue != 2)
                {
                    return false;
                }
            }
            else if (g == 5)
            { // p mod 5 = 1 or 4 for g = 5;
                var res = dhBI.Mod(BigInteger.ValueOf(5));
                int val = res.IntValue;
                if (val != 1 && val != 4)
                {
                    return false;
                }
            }
            else if (g == 6)
            { // p mod 24 = 19 or 23 for g = 6;
                var res = dhBI.Mod(BigInteger.ValueOf(24));
                int val = res.IntValue;
                if (val != 19 && val != 23)
                {
                    return false;
                }
            }
            else if (g == 7)
            { // p mod 7 = 3, 5 or 6 for g = 7.
                var res = dhBI.Mod(BigInteger.ValueOf(7));
                int val = res.IntValue;
                if (val != 3 && val != 5 && val != 6)
                {
                    return false;
                }
            }

            var hex = BitConverter.ToString(prime).Replace("-", String.Empty).ToUpperInvariant();
            if (hex.Equals("C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B"))
            {
                return true;
            }

            var dhBI2 = dhBI.Subtract(BigInteger.ValueOf(1)).Divide(BigInteger.ValueOf(2));
            return !(!dhBI.IsProbablePrime(30) || !dhBI2.IsProbablePrime(30));
        }

        public static bool CheckGaAndGb(byte[] ga, byte[] prime)
        {
            var g_a = new BigInteger(1, ga);
            var p = new BigInteger(1, prime);

            return CheckGaAndGb(g_a, p);
        }

        public static bool CheckGaAndGb(BigInteger ga, BigInteger prime)
        {
            return !(ga.CompareTo(BigInteger.ValueOf(1)) != 1 || ga.CompareTo(prime.Subtract(BigInteger.ValueOf(1))) != -1);
        }

        public static bool IsDisplayedDecryptedMessageInternal(TLDecryptedMessageBase message, bool displayEmpty = false)
        {
            var serviceMessage = message as TLDecryptedMessageService;
            if (serviceMessage != null)
            {
                var emptyAction = serviceMessage.Action as TLDecryptedMessageActionEmpty;
                if (emptyAction != null)
                {
                    if (displayEmpty)
                    {
                        return true;
                    }

                    return false;
                }

                var notifyLayerAction = serviceMessage.Action as TLDecryptedMessageActionNotifyLayer;
                if (notifyLayerAction != null)
                {
                    return false;
                }

                var deleteMessagesAction = serviceMessage.Action as TLDecryptedMessageActionDeleteMessages;
                if (deleteMessagesAction != null)
                {
                    return false;
                }

                var readMessagesAction = serviceMessage.Action as TLDecryptedMessageActionReadMessages;
                if (readMessagesAction != null)
                {
                    return false;
                }

                var flushHistoryAction = serviceMessage.Action as TLDecryptedMessageActionFlushHistory;
                if (flushHistoryAction != null)
                {
                    return false;
                }

                var resendAction = serviceMessage.Action as TLDecryptedMessageActionResend;
                if (resendAction != null)
                {
                    return false;
                }

                var requestKey = serviceMessage.Action as TLDecryptedMessageActionRequestKey;
                if (requestKey != null)
                {
                    return false;
                }

                var commitKey = serviceMessage.Action as TLDecryptedMessageActionCommitKey;
                if (commitKey != null)
                {
                    return false;
                }

                var acceptKey = serviceMessage.Action as TLDecryptedMessageActionAcceptKey;
                if (acceptKey != null)
                {
                    return false;
                }

                var noop = serviceMessage.Action as TLDecryptedMessageActionNoop;
                if (noop != null)
                {
                    return false;
                }

                var abortKey = serviceMessage.Action as TLDecryptedMessageActionAbortKey;
                if (abortKey != null)
                {
                    return false;
                }
            }

            return true;
        }

        public static TLInt GetOutSeqNo(TLInt currentUserId, TLEncryptedChat17 chat)
        {
            var isAdmin = chat.AdminId.Value == currentUserId.Value;
            var seqNo = 2 * chat.RawOutSeqNo.Value + (isAdmin ? 1 : 0);

            return new TLInt(seqNo);
        }

        public static TLInt GetInSeqNo(TLInt currentUserId, TLEncryptedChat17 chat)
        {
            var isAdmin = chat.AdminId.Value == currentUserId.Value;
            var seqNo = 2 * chat.RawInSeqNo.Value + (isAdmin ? 0 : 1);

            return new TLInt(seqNo);
        }

        public static TLString EncryptMessage2(TLObject decryptedMessage, TLInt currentUserId, TLEncryptedChatCommon chat)
        {
            Debug.WriteLine("TLUtils.EncryptMessage2");

            var random = new Random();

            var key = chat.Key.Data;
            var keyHash = Utils.ComputeSHA1(key);
            var keyFingerprint = new TLLong(BitConverter.ToInt64(keyHash, 12));
            var decryptedBytes = decryptedMessage.ToBytes();
            var bytes = Combine(BitConverter.GetBytes(decryptedBytes.Length), decryptedBytes);

            var padding = 16 - bytes.Length % 16;
            if (padding < 12)
            {
                padding += 16;
            }
            var paddingBytes = new byte[padding];
            random.NextBytes(paddingBytes);
            var bytesWithPadding = Combine(bytes, paddingBytes);

            var x = chat.AdminId.Value == currentUserId.Value ? 0 : 8; //8;
            var msgKeyLarge = Utils.ComputeSHA256(Combine(key.SubArray(88 + x, 32), bytesWithPadding));
            var msgKey = msgKeyLarge.SubArray(8, 16);

            var sha256_a = Utils.ComputeSHA256(Combine(msgKey, key.SubArray(x, 36)));
            var sha256_b = Utils.ComputeSHA256(Combine(key.SubArray(40 + x, 36), msgKey));
            var aesKey = Combine(sha256_a.SubArray(0, 8), sha256_b.SubArray(8, 16), sha256_a.SubArray(24, 8));
            var aesIV = Combine(sha256_b.SubArray(0, 8), sha256_a.SubArray(8, 16), sha256_b.SubArray(24, 8));

            var encryptedBytes = Utils.AesIge(bytesWithPadding, aesKey, aesIV, true);

            var resultBytes = Combine(keyFingerprint.ToBytes(), msgKey, encryptedBytes);

            return TLString.FromBigEndianData(resultBytes);
        }

        public static TLDecryptedMessageBase DecryptMessage2(TLString data, TLInt currentUserId, TLEncryptedChat chat, out bool commitChat)
        {
            Debug.WriteLine("TLUtils.DecryptMessage2");

            commitChat = false;

            var bytes = data.Data;

            var keyFingerprint = BitConverter.ToInt64(bytes, 0);
            var msgKey = bytes.SubArray(8, 16);
            var key = chat.Key.Data;
            var keyHash = Utils.ComputeSHA1(key);
            var calculatedKeyFingerprint = BitConverter.ToInt64(keyHash, keyHash.Length - 8);

            if (keyFingerprint != calculatedKeyFingerprint)
            {
                var chat20 = chat as TLEncryptedChat20;
                if (chat20 != null && chat20.PFS_Key != null)
                {
                    var pfsKeyHash = Utils.ComputeSHA1(chat20.PFS_Key.Data);
                    var pfsKeyFingerprint = BitConverter.ToInt64(pfsKeyHash, pfsKeyHash.Length - 8);
                    if (pfsKeyFingerprint == keyFingerprint)
                    {
                        chat20.Key = chat20.PFS_Key;
                        chat20.PFS_Key = null;
                        chat20.PFS_KeyFingerprint = null;
                        chat20.PFS_A = null;
                        chat20.PFS_ExchangeId = null;
                        commitChat = true;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            var x = chat.AdminId.Value == currentUserId.Value ? 8 : 0; //0;
            var sha256_a = Utils.ComputeSHA256(Combine(msgKey, key.SubArray(x, 36)));
            var sha256_b = Utils.ComputeSHA256(Combine(key.SubArray(40 + x, 36), msgKey));
            var aesKey = Combine(sha256_a.SubArray(0, 8), sha256_b.SubArray(8, 16), sha256_a.SubArray(24, 8));
            var aesIV = Combine(sha256_b.SubArray(0, 8), sha256_a.SubArray(8, 16), sha256_b.SubArray(24, 8));

            var encryptedBytes = bytes.SubArray(24, bytes.Length - 24);
            var decryptedBytes = Utils.AesIge(encryptedBytes, aesKey, aesIV, false);

            var length = BitConverter.ToInt32(decryptedBytes, 0);
            if (length <= 0 || (4 + length) > decryptedBytes.Length)
            {
                Log.Write("TLUtils.DecryptMessage length <= 0 || (4 + length) > decryptedBytes.Length");
                return null;
            }

            var calculatedMsgKeyLarge = Utils.ComputeSHA256(Combine(key.SubArray(88 + x, 32), decryptedBytes));
            var calculatedMsgKey = calculatedMsgKeyLarge.SubArray(8, 16);
            for (var i = 0; i < 16; i++)
            {
                if (msgKey[i] != calculatedMsgKey[i])
                {
                    Log.Write("TLUtils.DecryptMessage msgKey != calculatedMsgKey");
                    return null;
                }
            }

            var position = 4;
            var decryptedObject = TLObject.GetObject<TLObject>(decryptedBytes, ref position);
            var decryptedMessageLayer = decryptedObject as TLDecryptedMessageLayer;
            var decryptedMessageLayer17 = decryptedObject as TLDecryptedMessageLayer17;
            TLDecryptedMessageBase decryptedMessage = null;

            if (decryptedMessageLayer17 != null)
            {
                var randomBytes = decryptedMessageLayer17.RandomBytes.Data;
                if (randomBytes == null || randomBytes.Length < Constants.MinRandomBytesLength)
                {
                    Log.Write("TLUtils.DecryptMessage randomBytes.Length<" + Constants.MinRandomBytesLength);
                    return null;
                }

                decryptedMessage = decryptedMessageLayer17.Message;
                var decryptedMessage17 = decryptedMessage as ISeqNo;
                if (decryptedMessage17 != null)
                {
                    decryptedMessage17.InSeqNo = decryptedMessageLayer17.InSeqNo;
                    decryptedMessage17.OutSeqNo = decryptedMessageLayer17.OutSeqNo;
                }
            }
            else if (decryptedMessageLayer != null)
            {
                decryptedMessage = decryptedMessageLayer.Message;
            }
            else if (decryptedObject is TLDecryptedMessageBase)
            {
                decryptedMessage = (TLDecryptedMessageBase)decryptedObject;
            }

            return decryptedMessage;
        }

        public static TLString EncryptMessage(TLObject decryptedMessage, TLInt currentUserId, TLEncryptedChatCommon chat)
        {
            Debug.WriteLine("TLUtils.EncryptMessage");
            var chat20 = chat as TLEncryptedChat20;
            if (chat20 != null && chat20.Layer.Value >= Constants.MinSecretChatWithMTProto2Layer)
            {
                return EncryptMessage2(decryptedMessage, currentUserId, chat);
            }

            var random = new Random();

            var key = chat.Key.Data;
            var keyHash = Utils.ComputeSHA1(key);
            var keyFingerprint = new TLLong(BitConverter.ToInt64(keyHash, 12));
            var decryptedBytes = decryptedMessage.ToBytes();
            var bytes = Combine(BitConverter.GetBytes(decryptedBytes.Length), decryptedBytes);
            var sha1Hash = Utils.ComputeSHA1(bytes);
            var msgKey = sha1Hash.SubArray(sha1Hash.Length - 16, 16);

            var padding = (bytes.Length % 16 == 0) ? 0 : (16 - (bytes.Length % 16));
            var paddingBytes = new byte[padding];
            random.NextBytes(paddingBytes);
            var bytesWithPadding = Combine(bytes, paddingBytes);

            var x = 0;
            var sha1_a = Utils.ComputeSHA1(Combine(msgKey, key.SubArray(x, 32)));
            var sha1_b = Utils.ComputeSHA1(Combine(key.SubArray(32 + x, 16), msgKey, key.SubArray(48 + x, 16)));
            var sha1_c = Utils.ComputeSHA1(Combine(key.SubArray(64 + x, 32), msgKey));
            var sha1_d = Utils.ComputeSHA1(Combine(msgKey, key.SubArray(96 + x, 32)));
            var aesKey = Combine(sha1_a.SubArray(0, 8), sha1_b.SubArray(8, 12), sha1_c.SubArray(4, 12));
            var aesIV = Combine(sha1_a.SubArray(8, 12), sha1_b.SubArray(0, 8), sha1_c.SubArray(16, 4), sha1_d.SubArray(0, 8));

            var encryptedBytes = Utils.AesIge(bytesWithPadding, aesKey, aesIV, true);

            var resultBytes = Combine(keyFingerprint.ToBytes(), msgKey, encryptedBytes);

            return TLString.FromBigEndianData(resultBytes);
        }

        public static TLDecryptedMessageBase DecryptMessage(TLString data, TLInt currentUserId, TLEncryptedChat chat, out bool commitChat)
        {
            Debug.WriteLine("TLUtils.DecryptMessage");

            commitChat = false;

            var bytes = data.Data;

            var keyFingerprint = BitConverter.ToInt64(bytes, 0);
            var msgKey = bytes.SubArray(8, 16);
            var key = chat.Key.Data;
            var keyHash = Utils.ComputeSHA1(key);
            var calculatedKeyFingerprint = BitConverter.ToInt64(keyHash, keyHash.Length - 8);

            if (keyFingerprint != calculatedKeyFingerprint)
            {
                var chat20 = chat as TLEncryptedChat20;
                if (chat20 != null && chat20.PFS_Key != null)
                {
                    var pfsKeyHash = Utils.ComputeSHA1(chat20.PFS_Key.Data);
                    var pfsKeyFingerprint = BitConverter.ToInt64(pfsKeyHash, pfsKeyHash.Length - 8);
                    if (pfsKeyFingerprint == keyFingerprint)
                    {
                        chat20.Key = chat20.PFS_Key;
                        chat20.PFS_Key = null;
                        chat20.PFS_KeyFingerprint = null;
                        chat20.PFS_A = null;
                        chat20.PFS_ExchangeId = null;
                        commitChat = true;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            var x = 0;
            var sha1_a = Utils.ComputeSHA1(Combine(msgKey, key.SubArray(x, 32)));
            var sha1_b = Utils.ComputeSHA1(Combine(key.SubArray(32 + x, 16), msgKey, key.SubArray(48 + x, 16)));
            var sha1_c = Utils.ComputeSHA1(Combine(key.SubArray(64 + x, 32), msgKey));
            var sha1_d = Utils.ComputeSHA1(Combine(msgKey, key.SubArray(96 + x, 32)));

            var aesKey = Combine(sha1_a.SubArray(0, 8), sha1_b.SubArray(8, 12), sha1_c.SubArray(4, 12));
            var aesIV = Combine(sha1_a.SubArray(8, 12), sha1_b.SubArray(0, 8), sha1_c.SubArray(16, 4), sha1_d.SubArray(0, 8));

            var encryptedBytes = bytes.SubArray(24, bytes.Length - 24);
            var decryptedBytes = Utils.AesIge(encryptedBytes, aesKey, aesIV, false);

            var msgKeyEquals = true;
            var length = BitConverter.ToInt32(decryptedBytes, 0);
            if (length <= 0 || 4 + length > decryptedBytes.Length)
            {
                Log.Write("TLUtils.DecryptMessage length <= 0 || (4 + length) > decryptedBytes.Length");
                msgKeyEquals = false;
            }
            var calculatedMsgKey = msgKeyEquals ? Utils.ComputeSHA1(decryptedBytes.SubArray(0, 4 + length)) : new byte[] { };
            for (var i = 0; i < 16; i++)
            {
                if (msgKeyEquals && msgKey[i] != calculatedMsgKey[i + 4])
                {
                    msgKeyEquals = false;
                }
            }
            if (!msgKeyEquals)
            {
                Log.Write("TLUtils.DecryptMessage msgKey != calculatedMsgKey");

                var result = DecryptMessage2(data, currentUserId, chat, out commitChat);
                return result;
            }

            var position = 4;
            var decryptedObject = TLObject.GetObject<TLObject>(decryptedBytes, ref position);
            var decryptedMessageLayer = decryptedObject as TLDecryptedMessageLayer;
            var decryptedMessageLayer17 = decryptedObject as TLDecryptedMessageLayer17;
            TLDecryptedMessageBase decryptedMessage = null;

            if (decryptedMessageLayer17 != null)
            {
                var randomBytes = decryptedMessageLayer17.RandomBytes.Data;
                if (randomBytes == null || randomBytes.Length < Constants.MinRandomBytesLength)
                {
                    Log.Write("TLUtils.DecryptMessage randomBytes.Length<" + Constants.MinRandomBytesLength);
                    return null;
                }

                decryptedMessage = decryptedMessageLayer17.Message;
                var decryptedMessage17 = decryptedMessage as ISeqNo;
                if (decryptedMessage17 != null)
                {
                    decryptedMessage17.InSeqNo = decryptedMessageLayer17.InSeqNo;
                    decryptedMessage17.OutSeqNo = decryptedMessageLayer17.OutSeqNo;
                }
            }
            else if (decryptedMessageLayer != null)
            {
                decryptedMessage = decryptedMessageLayer.Message;
            }
            else if (decryptedObject is TLDecryptedMessageBase)
            {
                decryptedMessage = (TLDecryptedMessageBase)decryptedObject;
            }

            return decryptedMessage;
        }

        public static T OpenObjectFromFile<T>(object syncRoot, string fileName)
            where T : class
        {
            try
            {
                Debug.WriteLine("::OpenFile " + fileName);

                lock (syncRoot)
                {
                    using (var fileStream = FileUtils.GetLocalFileStreamForRead(fileName))
                    {
                        if (fileStream.Length > 0)
                        {
                            var serializer = new DataContractSerializer(typeof(T));
                            return serializer.ReadObject(fileStream) as T;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var caption = String.Format("MTPROTO FILE ERROR: cannot read {0} from file {1}", typeof(T), fileName);
                WriteLine(caption, LogSeverity.Error);
                WriteException(caption, e);
            }
            return default(T);
        }


        public static T OpenObjectFromMTProtoFile<T>(object syncRoot, string fileName, out long length)
            where T : TLObject
        {
            length = 0;

            try
            {

                Debug.WriteLine("::OpenMTProtoFile " + fileName);

                lock (syncRoot)
                {
                    using (var fileStream = FileUtils.GetLocalFileStreamForRead(fileName))
                    {
                        if (fileStream.Length > 0)
                        {
                            length = fileStream.Length;
                            return TLObject.GetObject<T>(fileStream);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var caption = String.Format("MTPROTO FILE ERROR: cannot read {0} from file {1}", typeof(T), fileName);
                WriteLine(caption, LogSeverity.Error);
                WriteException(caption, e);
            }
            return default(T);
        }

        public static T OpenObjectFromMTProtoFile<T>(object syncRoot, string fileName)
            where T : TLObject
        {
            try
            {
                Debug.WriteLine("::OpenMTProtoFile " + fileName);

                lock (syncRoot)
                {
                    using (var fileStream = FileUtils.GetLocalFileStreamForRead(fileName))
                    {
                        if (fileStream.Length > 0)
                        {
                            return TLObject.GetObject<T>(fileStream);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var caption = String.Format("MTPROTO FILE ERROR: cannot read {0} from file {1}", typeof(T), fileName);
                WriteLine(caption, LogSeverity.Error);
                WriteException(caption, e);
            }
            return default(T);
        }

        public static void SaveObjectToFile<T>(object syncRoot, string fileName, T data)
        {
            try
            {
                lock (syncRoot)
                {
                    using (var fileStream = FileUtils.GetLocalFileStreamForWrite(fileName))
                    {
                        var dcs = new DataContractSerializer(typeof(T));
                        dcs.WriteObject(fileStream, data);
                    }
                }
            }
            catch (Exception e)
            {
                var caption = String.Format("FILE ERROR: cannot write {0} to file {1}", typeof(T), fileName);
                WriteLine(caption, LogSeverity.Error);
                WriteException(caption, e);
            }
        }

        public static void SaveObjectToMTProtoFile<T>(object syncRoot, string fileName, T data)
            where T : TLObject
        {
            try
            {
                lock (syncRoot)
                {
                    FileUtils.SaveWithTempFile(fileName, data);
                }
            }
            catch (Exception e)
            {
                var caption = String.Format("MTPROTO FILE ERROR: cannot write {0} to file {1}", typeof(T), fileName);
                WriteLine(caption, LogSeverity.Error);
                WriteException(caption, e);
            }
        }

        public static TLPeerBase GetPeerFromMessage(TLDecryptedMessageBase message)
        {
            TLPeerBase peer = null;
            var commonMessage = message;
            if (commonMessage != null)
            {
                if (commonMessage.ChatId != null)
                {
                    peer = new TLPeerEncryptedChat { Id = commonMessage.ChatId };
                }
            }
            else
            {
                WriteLine("Cannot get peer from non TLDecryptedMessage", LogSeverity.Error);
            }

            return peer;
        }

        public static TLPeerBase GetPeerFromMessage(TLMessageBase message)
        {
            TLPeerBase peer = null;
            var commonMessage = message as TLMessageCommon;
            if (commonMessage != null)
            {
                if (commonMessage.ToId is TLPeerChannel)
                {
                    peer = commonMessage.ToId;
                }
                else if (commonMessage.ToId is TLPeerChat)
                {
                    peer = commonMessage.ToId;
                }
                else
                {
                    if (commonMessage.Out.Value)
                    {
                        peer = commonMessage.ToId;
                    }
                    else
                    {
                        peer = new TLPeerUser { Id = commonMessage.FromId };
                    }
                }
            }
            else
            {
                WriteLine("Cannot get peer from non TLMessageCommon", LogSeverity.Error);
            }

            return peer;
        }

        public static bool IsChannelMessage(TLMessageBase message, out TLPeerChannel channel)
        {
            var isChannel = false;
            channel = null;

            var messageCommon = message as TLMessageCommon;
            if (messageCommon != null)
            {
                channel = messageCommon.ToId as TLPeerChannel;
                isChannel = channel != null;
            }

            return isChannel;
        }

        public static bool InsertItem<T>(IList<T> items, T item, Func<T, long> getField, Func<T, long> equalitysField = null)
            where T : TLObject
        {
            var fieldValue = getField(item);
            for (var i = 0; i < items.Count; i++)
            {
                if (getField(items[i]) > fieldValue)
                {
                    items.Insert(i, item);
                    return true;
                }
                if (getField(items[i]) == fieldValue
                    && equalitysField != null
                    && equalitysField(items[i]) == equalitysField(item))
                {
                    return false;
                }
            }

            items.Add(item);
            return true;
        }

        public static bool InsertItemByDesc<T>(IList<T> items, T item, Func<T, long> getField, Func<T, long> equalityField = null)
            where T : TLObject
        {
            var fieldValue = getField(item);
            for (var i = 0; i < items.Count; i++)
            {
                if (getField(items[i]) < fieldValue)
                {
                    items.Insert(i, item);
                    return true;
                }
                if (getField(items[i]) == fieldValue
                    && equalityField != null
                    && equalityField(items[i]) == equalityField(item))
                {
                    return false;
                }
            }

            items.Add(item);
            return true;
        }

        public static IEnumerable<T> FindInnerObjects<T>(TLTransportMessage obj)
            where T : TLObject
        {
            var result = obj.MessageData as T;
            if (result != null)
            {
                yield return (T)obj.MessageData;
            }
            else
            {
                var gzipData = obj.MessageData as TLGzipPacked;
                if (gzipData != null)
                {
                    result = gzipData.Data as T;
                    if (result != null)
                    {
                        yield return result;
                    }
                }

                var container = obj.MessageData as TLContainer;
                if (container != null)
                {
                    foreach (var message in container.Messages)
                    {
                        result = message.MessageData as T;
                        if (result != null)
                        {
                            yield return (T)message.MessageData;
                        }
                        gzipData = message.MessageData as TLGzipPacked;
                        if (gzipData != null)
                        {
                            result = gzipData.Data as T;
                            if (result != null)
                            {
                                yield return result;
                            }
                        }
                    }
                }
            }
        }

        public static int InputPeerToId(TLInputPeerBase inputPeer, TLInt selfId)
        {
            var chat = inputPeer as TLInputPeerChat;
            if (chat != null)
            {
                return chat.ChatId.Value;
            }

            var contact = inputPeer as TLInputPeerContact;
            if (contact != null)
            {
                return contact.UserId.Value;
            }

            var foreign = inputPeer as TLInputPeerForeign;
            if (foreign != null)
            {
                return foreign.UserId.Value;
            }

            var user = inputPeer as TLInputPeerUser;
            if (user != null)
            {
                return user.UserId.Value;
            }

            var self = inputPeer as TLInputPeerSelf;
            if (self != null)
            {
                return selfId.Value;
            }

            return -1;
        }

        public static TLPeerBase InputPeerToPeer(TLInputPeerBase inputPeer, int selfId)
        {
            var channel = inputPeer as TLInputPeerChannel;
            if (channel != null)
            {
                return new TLPeerChannel { Id = channel.ChatId };
            }

            var broadcast = inputPeer as TLInputPeerBroadcast;
            if (broadcast != null)
            {
                return new TLPeerBroadcast { Id = broadcast.ChatId };
            }

            var chat = inputPeer as TLInputPeerChat;
            if (chat != null)
            {
                return new TLPeerChat { Id = chat.ChatId };
            }

            var contact = inputPeer as TLInputPeerContact;
            if (contact != null)
            {
                return new TLPeerUser { Id = contact.UserId };
            }

            var foreign = inputPeer as TLInputPeerForeign;
            if (foreign != null)
            {
                return new TLPeerUser { Id = foreign.UserId };
            }

            var user = inputPeer as TLInputPeerUser;
            if (user != null)
            {
                return new TLPeerUser { Id = user.UserId };
            }

            var self = inputPeer as TLInputPeerSelf;
            if (self != null)
            {
                return new TLPeerUser { Id = new TLInt(selfId) };
            }

            return null;
        }


        public static int MergeItemsDesc<T>(Func<T, int> dateIndexFunc, IList<T> current, IList<T> updated, int offset, int maxId, int count, out IList<T> removedItems, Func<T, int> indexFunc, Func<T, bool> skipTailFunc)
        {
            removedItems = new List<T>();

            var currentIndex = 0;
            var updatedIndex = 0;

            var index = new Dictionary<int, int>();
            foreach (var item in current)
            {
                var id = indexFunc(item);
                if (id > 0)
                {
                    index[id] = id;
                }
            }


            //skip just added or sending items
            while (updatedIndex < updated.Count
                && currentIndex < current.Count
                && dateIndexFunc(updated[updatedIndex]) < dateIndexFunc(current[currentIndex]))
            {
                currentIndex++;
            }

            // insert before current items
            while (updatedIndex < updated.Count
                && (current.Count < currentIndex
                    || (currentIndex < current.Count && dateIndexFunc(updated[updatedIndex]) > dateIndexFunc(current[currentIndex]))))
            {
                if (dateIndexFunc(current[currentIndex]) == 0)
                {
                    currentIndex++;
                    continue;
                }
                if (index.ContainsKey(indexFunc(updated[updatedIndex])))
                {
                    updatedIndex++;
                    continue;
                }
                current.Insert(currentIndex, updated[updatedIndex]);
                updatedIndex++;
                currentIndex++;
            }

            // update existing items
            if (updatedIndex < updated.Count)
            {
                for (; currentIndex < current.Count; currentIndex++)
                {
                    if (indexFunc != null
                        && indexFunc(current[currentIndex]) == 0)
                    {
                        continue;
                    }

                    for (; updatedIndex < updated.Count; updatedIndex++)
                    {
                        // missing item at current list
                        if (dateIndexFunc(updated[updatedIndex]) > dateIndexFunc(current[currentIndex]))
                        {
                            current.Insert(currentIndex, updated[updatedIndex]);
                            updatedIndex++;
                            break;
                        }
                        // equal item
                        if (dateIndexFunc(updated[updatedIndex]) == dateIndexFunc(current[currentIndex]))
                        {
                            updatedIndex++;
                            break;
                        }
                        // deleted item
                        if (dateIndexFunc(updated[updatedIndex]) < dateIndexFunc(current[currentIndex]))
                        {
                            var removedItem = current[currentIndex];
                            removedItems.Add(removedItem);
                            current.RemoveAt(currentIndex);
                            currentIndex--;
                            break;
                        }
                    }

                    // at the end of updated list
                    if (updatedIndex == updated.Count)
                    {
                        currentIndex++;
                        break;
                    }
                }
            }


            // all other items were deleted
            if (updated.Count > 0 && updated.Count < count && current.Count != currentIndex)
            {
                for (var i = current.Count - 1; i >= updatedIndex; i--)
                {
                    if (skipTailFunc != null && skipTailFunc(current[i]))
                    {
                        continue;
                    }
                    current.RemoveAt(i);
                }
                return currentIndex - 1;
            }

            // add after current items
            while (updatedIndex < updated.Count)
            {
                current.Add(updated[updatedIndex]);
                updatedIndex++;
                currentIndex++;
            }

            return currentIndex - 1;
        }

        public static TLInt DateToUniversalTimeTLInt(long clientDelta, DateTime date)
        {
            clientDelta = MTProtoService.Instance.ClientTicksDelta;

            var unixTime = (int)Utils.DateTimeToUnixTimestamp(date) + clientDelta / 4294967296.0; //int * 2^32 + clientDelta

            return new TLInt((int)unixTime);
        }

        public static TLInt ToTLInt(DateTime date)
        {
            var unixTime = (int)Utils.DateTimeToUnixTimestamp(date); //int * 2^32 + clientDelta

            return new TLInt(unixTime);
        }

        public static byte[] GenerateAuthKeyId(byte[] authKey)
        {
            var authKeyHash = Utils.ComputeSHA1(authKey);
            var authKeyId = authKeyHash.SubArray(12, 8);

            return authKeyId;
        }

        public static long GenerateLongAuthKeyId(byte[] authKey)
        {
            var authKeyHash = Utils.ComputeSHA1(authKey);
            var authKeyId = authKeyHash.SubArray(12, 8);

            return BitConverter.ToInt64(authKeyId, 0);
        }

        private const int encryptXParam = 0;
        private const int decryptXParam = 8;

        public static WindowsPhone.Tuple<byte[], byte[]> GetEncryptKeyIV(byte[] authKey, byte[] msgKey)
        {
            return GetKeyIVCommon(authKey, msgKey, encryptXParam);
        }

        public static WindowsPhone.Tuple<byte[], byte[]> GetDecryptKeyIV(byte[] authKey, byte[] msgKey)
        {
            return GetKeyIVCommon(authKey, msgKey, decryptXParam);
        }

        private static WindowsPhone.Tuple<byte[], byte[]> GetKeyIVCommon(byte[] authKey, byte[] msgKey, int x)
        {
#if MTPROTO
            var sha256_a = Utils.ComputeSHA256(Combine(msgKey, authKey.SubArray(x, 36)));
            var sha256_b = Utils.ComputeSHA256(Combine(authKey.SubArray(40 + x, 36), msgKey));

            var aesKey = Combine(
                sha256_a.SubArray(0, 8),
                sha256_b.SubArray(8, 16),
                sha256_a.SubArray(24, 8));

            var aesIV = Combine(
                sha256_b.SubArray(0, 8),
                sha256_a.SubArray(8, 16),
                sha256_b.SubArray(24, 8));
#else
            var sha1_a = Utils.ComputeSHA1(Combine(msgKey, authKey.SubArray(x, 32)));
            var sha1_b = Utils.ComputeSHA1(Combine(authKey.SubArray(32 + x, 16), msgKey, authKey.SubArray(48 + x, 16)));
            var sha1_c = Utils.ComputeSHA1(Combine(authKey.SubArray(64 + x, 32), msgKey));
            var sha1_d = Utils.ComputeSHA1(Combine(msgKey, authKey.SubArray(96 + x, 32)));

            var aesKey = Combine( 
                sha1_a.SubArray(0, 8),
                sha1_b.SubArray(8, 12),
                sha1_c.SubArray(4, 12));

            var aesIV = Combine(
                sha1_a.SubArray(8, 12),
                sha1_b.SubArray(0, 8),
                sha1_c.SubArray(16, 4),
                sha1_d.SubArray(0, 8));
#endif

            return new WindowsPhone.Tuple<byte[], byte[]>(aesKey, aesIV);
        }

#if MTPROTO
        public static byte[] GetEncryptMsgKey(byte[] authKey, byte[] dataWithPadding)
        {
            return GetMsgKeyCommon(authKey, dataWithPadding, encryptXParam);
        }

        public static byte[] GetDecryptMsgKey(byte[] authKey, byte[] dataWithPadding)
        {
            return GetMsgKeyCommon(authKey, dataWithPadding, decryptXParam);
        }

        private static byte[] GetMsgKeyCommon(byte[] authKey, byte[] dataWithPadding, int x)
        {
            var bytes = Combine(authKey.SubArray(88 + x, 32), dataWithPadding);

            var msgKeyLarge = Utils.ComputeSHA256(bytes);
            var msgKey = msgKeyLarge.SubArray(8, 16);

            return msgKey;
        }
#else
        public static byte[] GetMsgKey(byte[] data)
        {
            var bytes = Utils.ComputeSHA1(data);
            var last16Bytes = bytes.SubArray(4, 16);

            return last16Bytes;
        }
#endif

        public static byte[] Combine(params byte[][] arrays)
        {
            var length = 0;
            for (var i = 0; i < arrays.Length; i++)
            {
                length += arrays[i].Length;
            }

            var result = new byte[length]; ////[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }
            return result;
        }

        public static string MessageIdString(byte[] bytes)
        {
            var ticks = BitConverter.ToInt64(bytes, 0);
            var date = Utils.UnixTimestampToDateTime(ticks >> 32);

            return BitConverter.ToString(bytes) + " "
                + ticks + "%4=" + ticks % 4 + " "
                + date;
        }

        public static string MessageIdString(TLLong messageId)
        {
            var bytes = BitConverter.GetBytes(messageId.Value);
            var ticks = BitConverter.ToInt64(bytes, 0);
            var date = Utils.UnixTimestampToDateTime(ticks >> 32);

            return BitConverter.ToString(bytes) + " "
                + ticks + "%4=" + ticks % 4 + " "
                + date;
        }

        public static string MessageIdString(TLInt messageId)
        {
            var ticks = messageId.Value;
            var date = Utils.UnixTimestampToDateTime(ticks >> 32);

            return BitConverter.ToString(BitConverter.GetBytes(messageId.Value)) + " "
                + ticks + "%4=" + ticks % 4 + " "
                + date;
        }

        public static DateTime ToDateTime(TLInt date)
        {
            var ticks = date.Value;
            return Utils.UnixTimestampToDateTime(ticks >> 32);
        }

        public static void ThrowNotSupportedException(this byte[] bytes, string objectType)
        {
            throw new NotSupportedException(String.Format("Not supported {0} signature: {1}", objectType, BitConverter.ToString(bytes.SubArray(0, 4))));
        }

        public static void ThrowNotSupportedException(this byte[] bytes, int position, string objectType)
        {
            throw new NotSupportedException(String.Format("Not supported {0} signature: {1}", objectType, BitConverter.ToString(bytes.SubArray(position, position + 4))));
        }

        public static void ThrowExceptionIfIncorrect(this byte[] bytes, ref int position, uint signature)
        {
            //if (!bytes.SubArray(position, 4).StartsWith(signature))
            //{
            //    throw new ArgumentException(String.Format("Incorrect signature: actual - {1}, expected - {0}", SignatureToBytesString(signature), BitConverter.ToString(bytes.SubArray(0, 4))));
            //}
            position += 4;
        }

        public static void ThrowExceptionIfIncorrect(this byte[] bytes, ref int position, string signature)
        {
            //if (!bytes.SubArray(position, 4).StartsWith(signature))
            //{
            //    throw new ArgumentException(String.Format("Incorrect signature: actual - {1}, expected - {0}", SignatureToBytesString(signature), BitConverter.ToString(bytes.SubArray(0, 4))));
            //}
            position += 4;
        }

        private static bool StartsWith(this byte[] array, byte[] startArray)
        {
            for (var i = 0; i < startArray.Length; i++)
            {
                if (array[i] != startArray[i]) return false;
            }
            return true;
        }

        private static bool StartsWith(this byte[] array, int position, byte[] startArray)
        {
            for (var i = 0; i < startArray.Length; i++)
            {
                if (array[position + i] != startArray[i]) return false;
            }
            return true;
        }

        public static bool StartsWith(this byte[] bytes, uint signature)
        {
            var sign = BitConverter.ToUInt32(bytes, 0);

            return sign == signature;
        }

        public static bool StartsWith(this Stream input, uint signature)
        {
            var bytes = new byte[4];
            input.Read(bytes, 0, 4);
            var sign = BitConverter.ToUInt32(bytes, 0);

            return sign == signature;
        }

        public static bool StartsWith(this byte[] bytes, string signature)
        {
            if (signature[0] != '#') throw new ArgumentException("Incorrect first symbol of signature: expexted - #, actual - " + signature[0]);

            var signatureBytes = SignatureToBytes(signature);

            return bytes.StartsWith(signatureBytes);
        }

        public static bool StartsWith(this byte[] bytes, int position, uint signature)
        {
            var sign = BitConverter.ToUInt32(bytes, position);

            return sign == signature;
        }

        public static bool StartsWith(this byte[] bytes, int position, string signature)
        {
            if (signature[0] != '#') throw new ArgumentException("Incorrect first symbol of signature: expexted - #, actual - " + signature[0]);

            var signatureBytes = SignatureToBytes(signature);

            return bytes.StartsWith(position, signatureBytes);
        }

        public static string SignatureToBytesString(string signature)
        {
            if (signature[0] != '#') throw new ArgumentException("Incorrect first symbol of signature: expexted - #, actual - " + signature[0]);

            return BitConverter.ToString(SignatureToBytes(signature));
        }

        public static byte[] SignatureToBytes(uint signature)
        {
            return BitConverter.GetBytes(signature);
        }

        public static byte[] SignatureToBytes(string signature)
        {
            if (signature[0] != '#') throw new ArgumentException("Incorrect first symbol of signature: expexted - #, actual - " + signature[0]);

            var bytesString =
                signature.Length % 2 == 0 ?
                new string(signature.Replace("#", "0").ToArray()) :
                new string(signature.Replace("#", String.Empty).ToArray());

            var bytes = Utils.StringToByteArray(bytesString);
            Array.Reverse(bytes);
            return bytes;
        }


        public static TLDecryptedMessageLayer GetDecryptedMessageLayer(TLInt layer, TLInt inSeqNo, TLInt outSeqNo, TLDecryptedMessageBase message)
        {
            var randomBytes = new byte[15];
            var random = new SecureRandom();
            random.NextBytes(randomBytes);

            var decryptedMessageLayer17 = new TLDecryptedMessageLayer17();
            decryptedMessageLayer17.Layer = layer;
            decryptedMessageLayer17.InSeqNo = inSeqNo;
            decryptedMessageLayer17.OutSeqNo = outSeqNo;
            decryptedMessageLayer17.RandomBytes = TLString.FromBigEndianData(randomBytes);
            decryptedMessageLayer17.Message = message;

            return decryptedMessageLayer17;
        }

    }
}

