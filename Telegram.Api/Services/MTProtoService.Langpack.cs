// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Langpack;

namespace Telegram.Api.Services
{
    public partial class MTProtoService
    {
        public void GetLangPackAsync(TLString langCode, Action<TLLangPackDifference> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetLangPack { LangCode = langCode };

            SendInformativeMessage("langpack.getLangPack", obj, callback, faultCallback);
        }

        public void GetStringsAsync(TLString langCode, TLVector<TLString> keys, Action<TLVector<TLLangPackStringBase>> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetStrings { LangCode = langCode, Keys = keys };

            SendInformativeMessage("langpack.getStrings", obj, callback, faultCallback);
        }

        public void GetDifferenceAsync(TLInt fromVersion, Action<TLLangPackDifference> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetDifference { FromVersion = fromVersion };

            SendInformativeMessage("langpack.getDifference", obj, callback, faultCallback);
        }

        public void GetLanguagesAsync(Action<TLVector<TLLangPackLanguage>> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetLanguages();

            SendInformativeMessage("langpack.getLanguages", obj, callback, faultCallback);
        }
    }
}
