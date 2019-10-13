#pragma once
#include "ConnectionSocket.h"
namespace libtgnet
{
	ref class ConnectionSocketWrapper;
	public delegate void PacketReceivedEventHandler(ConnectionSocketWrapper^ sender, const Platform::Array<uint8_t>^ s);
	public delegate void ConnectionClosedEventHandler(ConnectionSocketWrapper^ sender);

	public ref class TCPO2StateWrapper sealed
	{
	public:
		property Platform::Array<uint8_t>^ Key;
		property Platform::Array<uint8_t>^ IV;
		property Platform::Array<uint8_t>^ Ecount;
		property uint32_t Num;
	};

	public ref class ConnectionSocketWrapper sealed
	{
	private:
		ConnectionSocket* _socket;

		Platform::String^ _host;
		void LOG(std::wstring str);

		uint64_t _sendTime;
		uint64_t _receiveTime;
	public:
		ConnectionSocketWrapper(ConnectionSettings^ connectionSettings, ProxySettings^ proxySettings);
		virtual ~ConnectionSocketWrapper();
		int SendPacket(const Platform::Array<uint8_t>^ data);
		event PacketReceivedEventHandler^ PacketReceived;
		event ConnectionClosedEventHandler^ Closed;
		int Connect();
		void StartReceive();
		void Close();

		uint64_t GetPing();

		static Platform::Array<uint8_t>^ AesCtr(const Platform::Array<uint8_t>^ data, TCPO2StateWrapper^ state);
	};
}

