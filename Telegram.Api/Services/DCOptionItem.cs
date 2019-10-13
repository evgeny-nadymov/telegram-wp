// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.Services
{
    public class ConnectionParams
    {
        public byte[] Salt { get; set; }

        public byte[] SessionId { get; set; }

        public byte[] AuthKey { get; set; }
    }

    public class DCOptionItem
    {
        public ConnectionParams Params { get; set; }

        public int Id { get; set; }

        public string Hostname { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }
    }
}
