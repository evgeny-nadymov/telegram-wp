// (c) Copyright Morten Nielsen.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#if SILVERLIGHT
using System;
using System.Net;
using System.Security;
using System.IO;
using System.Linq;

namespace SharpGIS
{
    /// <summary>
    /// This is an explicit web client class for doing webrequests.
    /// If you want to opt in for gzip support on all existing WebClients, consider
    /// using the <see cref="WebRequestCreator"/>
    /// </summary>
    public class GZipWebClient : WebClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GZipWebClient"/> class.
        /// </summary>
        [SecuritySafeCritical]
        public GZipWebClient()
        {
        }
        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri"/> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip"; //Set GZIP header
            return req;
        }
        /// <summary>
        /// Returns the <see cref="T:System.Net.WebResponse"/> for the specified <see cref="T:System.Net.WebRequest"/> using the specified <see cref="T:System.IAsyncResult"/>.
        /// </summary>
        /// <param name="request">A <see cref="T:System.Net.WebRequest"/> that is used to obtain the response.</param>
        /// <param name="result">An <see cref="T:System.IAsyncResult"/> object obtained from a previous call to <see cref="M:System.Net.WebRequest.BeginGetResponse(System.AsyncCallback,System.Object)"/> .</param>
        /// <returns>
        /// A <see cref="T:System.Net.WebResponse"/> containing the response for the specified <see cref="T:System.Net.WebRequest"/>.
        /// </returns>
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            try
            {
                WebResponse response = base.GetWebResponse(request, result);
                if (!(response is GZipWebResponse) && //this would be the case if WebRequestCreator was also used
                 (response.Headers[HttpRequestHeader.ContentEncoding] == "gzip") && response is HttpWebResponse)
                    return new GZipWebResponse(response as HttpWebResponse); //If gzipped response, uncompress
                else
                    return response;
            }
            catch
            {
                return null;
            }
        }
        internal sealed class GZipWebResponse : HttpWebResponse
        {
            private readonly HttpWebResponse _response;
            private readonly SharpGIS.GZipInflateStream _stream;

            internal GZipWebResponse(HttpWebResponse resp)
            {
                _response = resp;
                _stream = new GZipInflateStream(_response.GetResponseStream());
            }
            public override System.IO.Stream GetResponseStream()
            {
                return _stream;
            }
            public override void Close()
            {
                _response.Close();
                _stream.Close();
            }
            public override long ContentLength
            {
                get
                {
                    return _stream.Length;
                }
            }
            public override string ContentType
            {
                get { return _response.ContentType; }
            }
            public override WebHeaderCollection Headers
            {
                get { return _response.Headers; }
            }
            public override Uri ResponseUri
            {
                get { return _response.ResponseUri; }
            }
            public override bool SupportsHeaders
            {
                get { return _response.SupportsHeaders; }
            }
            public override string Method
            {
                get
                {
                    return _response.Method;
                }
            }
            public override HttpStatusCode StatusCode
            {
                get
                {
                    return _response.StatusCode;
                }
            }
            public override string StatusDescription
            {
                get
                {
                    return _response.StatusDescription;
                }
            }
            public override CookieCollection Cookies
            {
                get
                {
                    return _response.Cookies;
                }
            }
        }
    }
}
#endif