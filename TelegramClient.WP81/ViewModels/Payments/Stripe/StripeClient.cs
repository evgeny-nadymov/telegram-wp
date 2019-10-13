// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using TelegramClient.ViewModels.Payments.Stripe.JSON;

namespace TelegramClient.ViewModels.Payments.Stripe
{
    public class StripeClient : IDisposable
    {
        private readonly string _publishableKey;

        private HttpClient _client;

        public StripeClient(string publishableKey)
        {
            _publishableKey = publishableKey;
            _client = new HttpClient();
        }

        public async Task<StripeToken> CreateTokenAsync(Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException("card");
            }

            if (_client != null)
            {
                try
                {
                    var parameters = StripeNetworkUtils.HashMapFromCard(card);

                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/tokens");
                    var requestContent = new FormUrlEncodedContent(parameters);

                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", GetAuthorizationHeaderValue(_publishableKey));
                    request.Content = requestContent;

                    var response = await _client.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    var json = GetRootObject(content);
                    var token = new StripeToken {Id = json.id, Type = json.type, Error = json.error, Content = content};

                    return token;
                }
                catch
                {

                }
            }

            return null;
        }

        public static RootObject GetRootObject(string payload)
        {
            var serializer = new DataContractJsonSerializer(typeof(RootObject));
            RootObject rootObject;
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(payload)))
            {
                rootObject = serializer.ReadObject(stream) as RootObject;
            }

            return rootObject;
        }

        private string GetAuthorizationHeaderValue(string apiKey)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:", apiKey)));
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
    }
}
