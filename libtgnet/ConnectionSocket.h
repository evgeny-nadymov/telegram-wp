// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once
#define TCP_OBFUSCATED2
//#define TCP_OBFUSCATED
#include <vector>
#include <stdint.h>
#include <string>
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>

namespace libtgnet
{
	struct voip_crypto_functions_t{
		void(*rand_bytes)(uint8_t* buffer, size_t length);
		void(*sha1)(uint8_t* msg, size_t length, uint8_t* output);
		void(*sha256)(uint8_t* msg, size_t length, uint8_t* output);
		void(*aes_ige_encrypt)(uint8_t* in, uint8_t* out, size_t length, uint8_t* key, uint8_t* iv);
		void(*aes_ige_decrypt)(uint8_t* in, uint8_t* out, size_t length, uint8_t* key, uint8_t* iv);
		void(*aes_ctr_encrypt)(uint8_t* inout, size_t length, uint8_t* key, uint8_t* iv, uint8_t* ecount, uint32_t* num);
	};
	typedef struct voip_crypto_functions_t voip_crypto_functions_t;

	struct TCPO2State{
		unsigned char key[32];
		unsigned char iv[16];
		unsigned char ecount[16];
		uint32_t num;
	};

	public enum class ProxyType : int{
		Socks5 = 0,
		MTProto
	};

	public ref class ProxySettings sealed {
	public:
		property ProxyType Type;
		property Platform::String^ Host;
		property int Port;
		property bool IPv4;
		property Platform::String^ Username;
		property Platform::String^ Password;
		property Platform::Array<uint8_t>^ Secret;
	};

	public ref class ConnectionSettings sealed{
	public:
		property Platform::String^ Host;
		property int Port;
		property bool IPv4;
		property int16 ProtocolDCId;
		property Platform::Array<uint8_t>^ ProtocolSecret;
	};

	class ConnectionSocket
	{
	private:
		int16 _protocolDCId;
		Platform::Array<uint8_t>^ _protocolSecret = nullptr;
		Platform::String^ _host;
		int _port;
		bool _ipv4;
		ProxySettings^ _proxySettings = nullptr;
		bool _closed;

		unsigned char _recvbuf[64];
		SOCKET _socket = SOCKET_ERROR;
		TCPO2State _recvState;
		TCPO2State _sendState;

		void GenerateTCPO2States(unsigned char* buffer, TCPO2State* recvState, TCPO2State* sendState);
		void EncryptForTCPO2(unsigned char *buffer, size_t len, TCPO2State *state);
		uint32_t StringToV4Address(std::string address);
		void StringToV6Address(std::string address, unsigned char *out);
		int Connect(Platform::String^ address, int port, bool ipv4);
		int Connect(const addrinfo *info);

		void LOG(std::wstring str);
	public:
		static voip_crypto_functions_t crypto;

		ConnectionSocket(ConnectionSettings^ connectionSettings, ProxySettings^ proxySettings);
		~ConnectionSocket();
		int Connect();
		int SendPacket(const Platform::Array<uint8>^ data);
		Platform::Array<uint8>^ Receive();
		void Close();
	};
}
