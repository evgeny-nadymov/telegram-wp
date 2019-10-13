// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once

namespace TelegramClient_Native
{
	ref class ConnectionSocket;
	public delegate void PacketReceivedEventHandler(ConnectionSocket^ sender, const Platform::Array<uint8>^ s);

	public ref class ConnectionSocket sealed
	{
	private:
		SOCKET _socket;

	public:
		ConnectionSocket(Platform::String^ host, int port);

		event PacketReceivedEventHandler^ PacketReceived;
		void SendPacket(Platform::String^ caption, const Platform::Array<uint8>^ data);
		void Close();

		int GetReceiveBufferSize();

		int GetSendBufferSize();
	};
}

