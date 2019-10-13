// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Storage.Streams;
using Telegram.Api.Helpers;
using Windows.Networking.Sockets;
#if WIN_RT
#else
using System.Net.Sockets;
using Microsoft.Phone.Net.NetworkInformation;
#endif

namespace Telegram.Api.Transport
{
    /// <summary>
    /// Provides sock5 functionality to clients (Connect only).
    /// </summary>
    public class SocksProxy
    {
        private SocksProxy() { }

        #region ErrorMessages
        private static string[] errorMsgs =    {
                                        "Operation completed successfully.",
                                        "General SOCKS server failure.",
                                        "Connection not allowed by ruleset.",
                                        "Network unreachable.",
                                        "Host unreachable.",
                                        "Connection refused.",
                                        "TTL expired.",
                                        "Command not supported.",
                                        "Address type not supported.",
                                        "Unknown error."
                                    };
        #endregion

        private static bool Send(DataWriter dataWriter, byte[] buffer, int offset, int length)
        {
            var sendBuffer = buffer.SubArray(offset, length);
            dataWriter.WriteBytes(sendBuffer);
            var resul = dataWriter.StoreAsync().AsTask().Result;

            return true;
        }

        private static int Receive(DataReader dataReader, byte[] buffer, int offset, int length)
        {
            var bytesTransferred = dataReader.LoadAsync((uint)buffer.Length).AsTask().Result;
            var receiveBuffer = new byte[bytesTransferred];
            dataReader.ReadBytes(receiveBuffer);

            Array.Copy(receiveBuffer, buffer, receiveBuffer.Length);

            return (int) bytesTransferred;
        }


        public static async Task<StreamSocket> ConnectToSocks5Proxy(double timeout, StreamSocket s, DataWriter _dataWriter, DataReader _dataReader, string proxyAddress, ushort proxyPort, string destAddress, ushort destPort, string userName, string password)
        {
            var destIP = new HostName(destAddress);
            var proxyIP = new HostName(proxyAddress);
            var request = new byte[256];
            var response = new byte[256];
            ushort nIndex;

            // open a TCP connection to SOCKS server...
            //Connect(s, proxyEndPoint);
            await s.ConnectAsync(proxyIP, proxyPort.ToString(CultureInfo.InvariantCulture)).WithTimeout(timeout);

            nIndex = 0;
            request[nIndex++] = 0x05; // Version 5.
            request[nIndex++] = 0x02; // 2 Authentication methods are in packet...
            request[nIndex++] = 0x00; // NO AUTHENTICATION REQUIRED
            request[nIndex++] = 0x02; // USERNAME/PASSWORD

            Send(_dataWriter, request, 0, nIndex);
            var nGot = Receive(_dataReader, response, 0, response.Length);

            // Receive 2 byte response...
            if (nGot != 2)
                throw new ConnectionException("Bad response received from proxy server.");

            if (response[1] == 0xFF)
            {    // No authentication method was accepted close the socket.
                s.Dispose();
                throw new ConnectionException("None of the authentication method was accepted by proxy server.");
            }

            byte[] rawBytes;
            //Username/Password Authentication protocol
            if (response[1] == 0x02)
            {
                nIndex = 0;
                request[nIndex++] = 0x01; // Version 5.

                // add user name
                request[nIndex++] = (byte)userName.Length;
                rawBytes = Encoding.UTF8.GetBytes(userName);
                rawBytes.CopyTo(request, nIndex);
                nIndex += (ushort)rawBytes.Length;

                // add password
                request[nIndex++] = (byte)password.Length;
                rawBytes = Encoding.UTF8.GetBytes(password);
                rawBytes.CopyTo(request, nIndex);
                nIndex += (ushort)rawBytes.Length;

                // Send the Username/Password request
                Send(_dataWriter, request, 0, nIndex);
                nGot = Receive(_dataReader, response, 0, response.Length);

                if (nGot != 2)
                    throw new ConnectionException("Bad response received from proxy server.");
                if (response[1] != 0x00)
                    throw new ConnectionException("Bad Usernaem/Password.");
            }
            //// This version only supports connect command. 
            //// UDP and Bind are not supported.

            // Send connect request now...
            nIndex = 0;
            request[nIndex++] = 0x05;    // version 5.
            request[nIndex++] = 0x01;    // command = connect.
            request[nIndex++] = 0x00;    // Reserve = must be 0x00

            // Destination adress in an IP.
            switch (destIP.Type)
            {
                case HostNameType.Ipv4:
                    // Address is IPV4 format
                    request[nIndex++] = 0x01;
                    rawBytes = destIP.GetAddressBytes();
                    rawBytes.CopyTo(request, nIndex);
                    nIndex += (ushort)rawBytes.Length;
                    break;
                case HostNameType.Ipv6:
                    // Address is IPV6 format
                    request[nIndex++] = 0x04;
                    rawBytes = destIP.GetAddressBytes();
                    rawBytes.CopyTo(request, nIndex);
                    nIndex += (ushort)rawBytes.Length;
                    break;
                //case HostNameType.DomainName:
                //    // Dest. address is domain name.
                //    request[nIndex++] = 0x03;    // Address is full-qualified domain name.
                //    request[nIndex++] = Convert.ToByte(destAddress.Length); // length of address.
                //    rawBytes = Encoding.UTF8.GetBytes(destAddress);
                //    rawBytes.CopyTo(request, nIndex);
                //    nIndex += (ushort)rawBytes.Length;
                //    break;
            }

            // using big-edian byte order
            byte[] portBytes = BitConverter.GetBytes(destPort);
            for (int i = portBytes.Length - 1; i >= 0; i--)
                request[nIndex++] = portBytes[i];

            // send connect request.
            Send(_dataWriter, request, 0, nIndex);
            nGot = Receive(_dataReader, response, 0, response.Length);

            if (response[1] != 0x00)
                throw new ConnectionException(errorMsgs[response[1]]);
            
            // Success Connected...
            return s;
        }
#if !WIN_RT
        private static bool Send(Socket s, byte[] buffer, int offset, int length)
        {
            var send1Event = new AutoResetEvent(false);
            var sendArgs = new SocketAsyncEventArgs();
            sendArgs.SetBuffer(buffer, offset, length);
            sendArgs.Completed += (sender, eventArgs) =>
            {
                send1Event.Set();
            };

            var result = s.SendAsync(sendArgs);
            send1Event.WaitOne();

            return result;
        }

        private static int Receive(Socket s, byte[] buffer, int offset, int length)
        {
            var receive1Event = new AutoResetEvent(false);
            var receive1Args = new SocketAsyncEventArgs();
            receive1Args.SetBuffer(buffer, offset, length);
            receive1Args.Completed += (sender, eventArgs) =>
            {
                receive1Event.Set();
            };
            var result = s.ReceiveAsync(receive1Args);
            receive1Event.WaitOne();

            return receive1Args.BytesTransferred;
        }

        private static bool Connect(Socket s, IPEndPoint remoteEndpoit)
        {
            var connectEvent = new AutoResetEvent(false);
            var connectArgs = new SocketAsyncEventArgs();
            connectArgs.RemoteEndPoint = remoteEndpoit;
            connectArgs.Completed += (sender, eventArgs) =>
            {
                connectEvent.Set();
            };
            s.ConnectAsync(connectArgs);
            connectEvent.WaitOne();

            return connectArgs.SocketError == System.Net.Sockets.SocketError.Success;
        }

        public static Socket ConnectToSocks5Proxy(Socket s, string proxyAdress, ushort proxyPort, string destAddress, ushort destPort, string userName, string password)
        {
            IPAddress destIP = null;
            IPAddress proxyIP = null;
            var request = new byte[256];
            var response = new byte[256];
            ushort nIndex;

            try
            {
                proxyIP = IPAddress.Parse(proxyAdress);
            }
            catch (FormatException)
            {    // get the IP address
                NameResolutionResult resolutionResult = null;
                var dnsResolveEvent = new AutoResetEvent(false);
                var endpoint = new DnsEndPoint(proxyAdress, 0);
                DeviceNetworkInformation.ResolveHostNameAsync(endpoint, result =>
                {
                    resolutionResult = result;
                    dnsResolveEvent.Set();
                }, null);
                dnsResolveEvent.WaitOne();
                proxyIP = resolutionResult.IPEndPoints[0].Address;
            }

            // Parse destAddress (assume it in string dotted format "212.116.65.112" )
            try
            {
                destIP = IPAddress.Parse(destAddress);
            }
            catch (FormatException)
            {
                // wrong assumption its in domain name format "www.microsoft.com"
            }

            var proxyEndPoint = new IPEndPoint(proxyIP, proxyPort);

            // open a TCP connection to SOCKS server...
            var connected = Connect(s, proxyEndPoint);
            if (!connected)
            {
                throw new ConnectionException("Can't connect to proxy server.");
            }

            nIndex = 0;
            request[nIndex++] = 0x05; // Version 5.
            request[nIndex++] = 0x02; // 2 Authentication methods are in packet...
            request[nIndex++] = 0x00; // NO AUTHENTICATION REQUIRED
            request[nIndex++] = 0x02; // USERNAME/PASSWORD

            Send(s, request, 0, nIndex);
            var nGot = Receive(s, response, 0, response.Length);

            // Receive 2 byte response...
            if (nGot != 2)
                throw new ConnectionException("Bad response received from proxy server.");

            if (response[1] == 0xFF)
            {    // No authentication method was accepted close the socket.
                s.Close();
                throw new ConnectionException("None of the authentication method was accepted by proxy server.");
            }

            byte[] rawBytes;
            //Username/Password Authentication protocol
            if (response[1] == 0x02)
            {
                nIndex = 0;
                request[nIndex++] = 0x01; // Version 5.

                // add user name
                request[nIndex++] = (byte)userName.Length;
                rawBytes = Encoding.UTF8.GetBytes(userName);
                rawBytes.CopyTo(request, nIndex);
                nIndex += (ushort)rawBytes.Length;

                // add password
                request[nIndex++] = (byte)password.Length;
                rawBytes = Encoding.UTF8.GetBytes(password);
                rawBytes.CopyTo(request, nIndex);
                nIndex += (ushort)rawBytes.Length;

                // Send the Username/Password request
                Send(s, request, 0, nIndex);
                nGot = Receive(s, response, 0, response.Length);

                if (nGot != 2)
                    throw new ConnectionException("Bad response received from proxy server.");
                if (response[1] != 0x00)
                    throw new ConnectionException("Bad Usernaem/Password.");
            }
            //// This version only supports connect command. 
            //// UDP and Bind are not supported.

            // Send connect request now...
            nIndex = 0;
            request[nIndex++] = 0x05;    // version 5.
            request[nIndex++] = 0x01;    // command = connect.
            request[nIndex++] = 0x00;    // Reserve = must be 0x00

            if (destIP != null)
            {
                // Destination adress in an IP.
                switch (destIP.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        // Address is IPV4 format
                        request[nIndex++] = 0x01;
                        rawBytes = destIP.GetAddressBytes();
                        rawBytes.CopyTo(request, nIndex);
                        nIndex += (ushort)rawBytes.Length;
                        break;
                    case AddressFamily.InterNetworkV6:
                        // Address is IPV6 format
                        request[nIndex++] = 0x04;
                        rawBytes = destIP.GetAddressBytes();
                        rawBytes.CopyTo(request, nIndex);
                        nIndex += (ushort)rawBytes.Length;
                        break;
                }
            }
            else
            {
                // Dest. address is domain name.
                request[nIndex++] = 0x03;    // Address is full-qualified domain name.
                request[nIndex++] = Convert.ToByte(destAddress.Length); // length of address.
                rawBytes = Encoding.UTF8.GetBytes(destAddress);
                rawBytes.CopyTo(request, nIndex);
                nIndex += (ushort)rawBytes.Length;
            }

            // using big-edian byte order
            byte[] portBytes = BitConverter.GetBytes(destPort);
            for (int i = portBytes.Length - 1; i >= 0; i--)
                request[nIndex++] = portBytes[i];

            // send connect request.
            Send(s, request, 0, nIndex);
            nGot = Receive(s, response, 0, response.Length);

            if (response[1] != 0x00)
                throw new ConnectionException(errorMsgs[response[1]]);
            // Success Connected...
            return s;
        }
#endif
    }

    public class ConnectionException : Exception
    {
        public ConnectionException(string message)
            : base(message)
        {
        }
    }

    public static class NetworkConverter
    {
        public static bool IsLoopBackForIPv4(string ipv4)
        {
            var data = GetBytesForIPv4(ipv4);
            return data[3] == 1 && data[0] == 127 && data[1] == 0 && data[2] == 0;
        }

        public static bool IsLoopBackForIPv6(string ipv6)
        {
            var data = GetWordsForIPv6(ipv6);
            return IsLoopBackForIPv6(data);
        }

        public static bool IsLoopBack(this HostName hostName)
        {
            switch (hostName.Type)
            {
                case HostNameType.Ipv4:
                    return IsLoopBackForIPv4(hostName.CanonicalName);
                case HostNameType.Ipv6:
                    return IsLoopBackForIPv6(hostName.CanonicalName);
            }

            throw new NotSupportedException();
        }

        public static bool IsLoopBackForIPv6(ushort[] data)
        {
            for (var i = 0; i != 5; ++i)
                if (data[i] != 0)
                    return false;
            if (data[5] == 0)
                return data[6] == 0 && data[7] == 1;
            if (data[5] != 0xFFFF)
                return false;
            return data[6] == 0x7F00 && data[7] == 1;
        }

        public static byte[] GetPortBytes(int port)
        {
            var portBytes = BitConverter.GetBytes((ushort)port);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(portBytes);
            return portBytes;
        }

        public static int ToPort(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public static byte[] GetAddressBytes(this HostName hostName)
        {
            switch (hostName.Type)
            {
                case HostNameType.Ipv4:
                    return GetBytesForIPv4(hostName.CanonicalName);
                case HostNameType.Ipv6:
                    return GetBytesForIPv6(hostName.CanonicalName);
            }
            throw new NotSupportedException();
        }

        public static byte[] GetBytesForIPv6(string ipv6)
        {
            var result = new byte[16];
            var idxDst = 0;
            var words = GetWordsForIPv6(ipv6);
            for (var idxSrc = 0; idxSrc != words.Length; ++idxSrc)
            {
                var v = words[idxSrc];
                result[idxDst++] = (byte)((v >> 8) & 0xFF);
                result[idxDst++] = (byte)(v & 0xFF);
            }

            return result;
        }

        public static byte[] GetBytesForIPv4(string ipv4)
        {
            var result = ipv4.Split('.').Select(byte.Parse)
                .ToArray();
            return result;
        }

        public static ushort[] GetWordsForIPv6(string ipv6)
        {
            var data = new ushort[8];
            ipv6 = ipv6.Replace(" ", string.Empty);
            if (ipv6.StartsWith("::ffff:"))
            {
                data[5] = 0xFFFF;
                var ipv4 = ipv6.Substring(7);
                if (ipv4.IndexOf(':') != -1)
                {
                    var parts = ipv4.Split(':')
                        .Select(x => ushort.Parse(x, System.Globalization.NumberStyles.HexNumber))
                        .ToArray();
                    data[6] = parts[0];
                    data[7] = parts[1];
                }
                else
                {
                    var d = GetBytesForIPv4(ipv4);
                    data[6] = (ushort)((d[0] << 8) + d[1]);
                    data[7] = (ushort)((d[2] << 8) + d[3]);
                }
            }
            else
            {
                var parts = ipv6.Split(':')
                    .Select(x => string.IsNullOrWhiteSpace(x) ? -1 : int.Parse(x, System.Globalization.NumberStyles.HexNumber))
                    .ToArray();
                var prefixSize = Array.IndexOf(parts, -1);
                if (prefixSize == -1)
                {
                    if (parts.Length != 8)
                        throw new ArgumentOutOfRangeException();
                    data = parts.Select(x => (ushort)x).ToArray();
                }
                else
                {
                    var nonEmptyIndex = prefixSize;
                    while (nonEmptyIndex < (parts.Length - 1) && parts[nonEmptyIndex + 1] == -1)
                        nonEmptyIndex += 1;
                    var suffixSize = parts.Length - nonEmptyIndex - 1;
                    for (var i = 0; i != prefixSize; ++i)
                        data[i] = (ushort)parts[i];
                    var suffixIndexSrc = parts.Length - suffixSize;
                    var suffixIndexDst = data.Length - suffixSize;
                    for (var i = 0; i != suffixSize; ++i)
                        data[suffixIndexDst++] = (ushort)parts[suffixIndexSrc++];
                }
            }

            return data;
        }

        public static string ToIPv4(byte[] data)
        {
            return string.Join(".", data.Reverse().Select(x => x.ToString()));
        }

        public static string ToIPv6(byte[] data)
        {
            var words = new ushort[8];
            var idxDst = 0;
            for (var idxSrc = 0; idxSrc != data.Length; idxSrc += 2)
                words[idxDst++] = (ushort)((data[idxSrc] << 8) + data[idxSrc + 1]);
            return ToIPv6(words);
        }

        public static string ToIPv6(ushort[] data)
        {
            var zeroRanges = new List<Tuple<int, int>>();
            var startIndex = -1;
            var indexCount = 0;
            for (var i = 0; i != 8; ++i)
            {
                var v = data[i];
                if (v == 0)
                {
                    if (startIndex == -1)
                    {
                        startIndex = i;
                        indexCount = 1;
                    }
                    else
                        indexCount += 1;
                }
                else if (v != 0 && startIndex != -1)
                {
                    zeroRanges.Add(Tuple.Create(startIndex, indexCount));
                    startIndex = -1;
                }
            }

            if (startIndex != -1)
                zeroRanges.Add(Tuple.Create(startIndex, indexCount));

            if (zeroRanges.Count != 0)
            {
                var largestRange = zeroRanges.OrderByDescending(x => x.Item2).First();
                startIndex = largestRange.Item1;
                indexCount = largestRange.Item2;
            }

            ushort[] wordsPrefix, wordsSuffix;
            if (startIndex != -1)
            {
                wordsPrefix = data.Take(startIndex).ToArray();
                wordsSuffix = data.Skip(startIndex + indexCount).ToArray();
            }
            else
            {
                wordsPrefix = data;
                wordsSuffix = null;
            }

            var result = new StringBuilder();
            if (wordsPrefix.Length != 0)
                result.Append(string.Join(":", wordsPrefix.Select(x => x.ToString("x"))));
            if (wordsSuffix != null)
                result
                    .Append("::")
                    .Append(string.Join(":", wordsSuffix.Select(x => x.ToString("x"))));
            return result.ToString();
        }
    }
}
