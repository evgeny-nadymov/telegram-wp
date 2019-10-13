// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Telegram.Api.Services.Cache;

namespace Telegram.Api.TL
{
    public class TLWebAuthorization : TLObject
    {
        public const uint Signature = TLConstructors.TLWebAuthorization;

        public TLLong Hash { get; set; }

        public TLInt BotId { get; set; }

        public TLString Domain { get; set; }

        public TLString Browser { get; set; }

        public TLString Platform { get; set; }

        public TLInt DateCreated { get; set; }

        public TLInt DateActive { get; set; }

        public TLString Ip { get; set; }

        public TLString Region { get; set; }

        public string Caption
        {
            get
            {
                var domain =
                    Domain.ToString()
                        .Replace("https://", string.Empty)
                        .Replace("http://", string.Empty)
                        .Replace("www.", string.Empty);
                var index = domain.IndexOf('.');
                if (index > 0)
                {
                    var result = domain.Substring(0, index);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return char.ToUpper(result[0]) + result.Substring(1);
                    }
                }

                return Domain.ToString();
            }
        }

        public string Location
        {
            get { return string.Format("{0} – {1}", Ip, Region); }
        }

        public TLUserBase Bot
        {
            get
            {
                var cacheService = InMemoryCacheService.Instance;

                return cacheService.GetUser(BotId);
            }
        }

        public string ParamsString
        {
            get { return string.Join(", ", Params); }
        }

        public IEnumerable<string> Params
        {
            get
            {
                if (Bot != null) yield return Bot.FullName;
                if (!TLString.IsNullOrEmpty(Browser)) yield return Browser.ToString();
                if (!TLString.IsNullOrEmpty(Platform)) yield return Platform.ToString();
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLLong>(bytes, ref position);
            BotId = GetObject<TLInt>(bytes, ref position);
            Domain = GetObject<TLString>(bytes, ref position);
            Browser = GetObject<TLString>(bytes, ref position);
            Platform = GetObject<TLString>(bytes, ref position);
            DateCreated = GetObject<TLInt>(bytes, ref position);
            DateActive = GetObject<TLInt>(bytes, ref position);
            Ip = GetObject<TLString>(bytes, ref position);
            Region = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public void Update(TLWebAuthorization authorization)
        {
            Hash = authorization.Hash;
            BotId = authorization.BotId;
            Domain = authorization.Domain;
            Browser = authorization.Browser;
            Platform = authorization.Platform;
            DateCreated = authorization.DateCreated;
            DateActive = authorization.DateActive;
            Ip = authorization.Ip;
            Region = authorization.Region;
        }
    }
}
