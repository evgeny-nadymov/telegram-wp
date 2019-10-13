// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"

#include <sstream>
#include "ConnectionSocket.h"
#include "MicrosoftCryptoImpl.h"
#include "BufferOutputStream.h"

#pragma comment(lib,"WS2_32")

using namespace libtgnet;

voip_crypto_functions_t ConnectionSocket::crypto; // set it yourself upon initialization

ConnectionSocket::ConnectionSocket(ConnectionSettings^ connectionSettings, ProxySettings^ proxySettings)
{
	ConnectionSocket::crypto.aes_ige_decrypt = MicrosoftCryptoImpl::AesIgeDecrypt;
	ConnectionSocket::crypto.aes_ige_encrypt = MicrosoftCryptoImpl::AesIgeEncrypt;
	ConnectionSocket::crypto.aes_ctr_encrypt = MicrosoftCryptoImpl::AesCtrEncrypt;
	ConnectionSocket::crypto.sha1 = MicrosoftCryptoImpl::SHA1;
	ConnectionSocket::crypto.sha256 = MicrosoftCryptoImpl::SHA256;
	ConnectionSocket::crypto.rand_bytes = MicrosoftCryptoImpl::RandBytes;
	MicrosoftCryptoImpl::Init();

	WORD wVersionRequested;
	WSADATA wsaData;
	int err;

	/* Use the MAKEWORD(lowbyte, highbyte) macro declared in Windef.h */
	wVersionRequested = MAKEWORD(2, 2);

	err = WSAStartup(wVersionRequested, &wsaData);
	if (err != 0) {
		/* Tell the user that we could not find a usable */
		/* Winsock DLL.                                  */
		printf("WSAStartup failed with error: %d\n", err);
		return;
	}

	_host = connectionSettings->Host;
	_port = connectionSettings->Port;
	_ipv4 = connectionSettings->IPv4;
	_protocolDCId = connectionSettings->ProtocolDCId;
	_protocolSecret = connectionSettings->ProtocolSecret;

	_proxySettings = proxySettings;
}

ConnectionSocket::~ConnectionSocket()
{

}

int ConnectionSocket::SendPacket(const Platform::Array<uint8>^ data)
{
	BufferOutputStream os(data->Length + 4);
	size_t len = data->Length / 4;
	if (len < 0x7F)
	{
		os.WriteByte((unsigned char)len);
	}
	else
	{
		os.WriteByte(0x7F);
		os.WriteByte((unsigned char)(len & 0xFF));
		os.WriteByte((unsigned char)((len >> 8) & 0xFF));
		os.WriteByte((unsigned char)((len >> 16) & 0xFF));
	}
	os.WriteBytes(data->begin(), data->Length);

#ifdef TCP_OBFUSCATED2
	EncryptForTCPO2(os.GetBuffer(), os.GetLength(), &_sendState);
#endif

	int res = send(_socket, (const char*)os.GetBuffer(), os.GetLength(), 0);
	if (res < 0)
	{
		printf("send failed with error: %d\n", res);
	}

	return res;
}

Platform::Array<uint8>^ ConnectionSocket::Receive()
{
	int length = 0;
	int offset = 0;

	int res = recv(_socket, (char*)_recvbuf, 1, 0);
	if (res <= 0)
	{
		printf("recv failed with error: %d\n", res);
		return nullptr;
	}

#ifdef TCP_OBFUSCATED2
	EncryptForTCPO2(_recvbuf, 1, &_recvState);
#endif

	if (_recvbuf[0] < 0x7F)
	{
		length = _recvbuf[0] * 4;
	}
	else
	{
		int res = recv(_socket, (char*)_recvbuf, 3, 0);
		if (res <= 0)
		{
			printf("recv failed with error: %d\n", res);
			return nullptr;
		}
		if (res < 3)
		{
			printf("recv failed with incorrect length size: %d\n", res);
			return nullptr;
		}

#ifdef TCP_OBFUSCATED2
		EncryptForTCPO2(_recvbuf, 3, &_recvState);
#endif

		length = ((size_t)_recvbuf[0] | ((size_t)_recvbuf[1] << 8) | ((size_t)_recvbuf[2] << 16)) * 4;
	}

	if (length <= 0)
	{
		printf("recv failed with incorrect length: %d\n", length);
		return nullptr;
	}

	Platform::Array<uint8>^ buffer = ref new Platform::Array<uint8_t>(length);

	while (offset < length)
	{
		int res = recv(_socket, (char*)buffer->begin() + offset, length - offset, 0);
		if (res <= 0)
		{
			printf("recv failed with error: %d\n", res);
			return nullptr;
		}
		offset += res;
	}

#ifdef TCP_OBFUSCATED2
	EncryptForTCPO2(buffer->begin(), length, &_recvState);
#endif

	return buffer;
}

void ConnectionSocket::LOG(std::wstring str)
{
	std::wostringstream stream;
	stream << L"[libtvnet::ConnectionSocket " + std::wstring(_host->Begin()) + L"] => " << _socket << L" " << str << L"\n";

	::OutputDebugString(stream.str().c_str());
}

void ConnectionSocket::Close()
{
	LOG(L"Close");
	if (_socket == SOCKET_ERROR) return;

	_closed = true;
	int err = closesocket(_socket);
	if (err < 0)
	{
		printf("closesocket failed with error: %d\n", err);
		return;
	}
}

int ConnectionSocket::Connect()
{
	int err = -1;
	BufferOutputStream p(1024);

	// init socket
	if (_proxySettings != nullptr)
	{
		struct addrinfo hints;
		struct addrinfo *info;
		memset(&hints, 0, sizeof(hints));

		std::wstring addressW(_proxySettings->Host->Begin());
		std::string addressA(addressW.begin(), addressW.end());

		std::string portA = std::to_string(_proxySettings->Port);

		err = getaddrinfo(addressA.c_str(), portA.c_str(), &hints, &info);
		if (err < 0)
		{
			printf("getaddrinfo failed with error: %d\n", err);
			return -2;
		}
		if (info == nullptr)
		{
			printf("getaddrinfo info is null");
			return -2;	// IMPORTANT! err=11001 here
		}

		err = Connect(info);
		if (err < 0)
		{
			printf("connect failed with error: %d\n", err);
			return -4;
		}

		if (_proxySettings->Type == ProxyType::MTProto)
		{
			_protocolSecret = _proxySettings->Secret;
		}
		else
		{
			p.Reset();
			if (_proxySettings->Username->IsEmpty())
			{
				p.WriteByte(0x05); // VER
				p.WriteByte(0x01); // NMETHODS
				p.WriteByte(0x00); // no auth
			}
			else
			{
				p.WriteByte(0x05); // VER
				p.WriteByte(0x02); // NMETHODS
				p.WriteByte(0x00); // no auth
				p.WriteByte(0x02); // user/pass
			}

			err = send(_socket, (const char*)p.GetBuffer(), p.GetLength(), 0);
			if (err < 0)
			{
				//closesocket(_socket);
				printf("send to proxy failed with error: %d\n", err);
				return -8;
			}

			err = recv(_socket, (char*)_recvbuf, sizeof(_recvbuf), 0);
			if (err != 2)
			{
				//closesocket(_socket);
				printf("recv from proxy failed with error: %d\n", err);
				return -8;
			}

			if (_recvbuf[1] == 0xFF)
			{
				//closesocket(_socket);
				printf("none of the authentication method was accepted by proxy server");
				return -8;
			}

			//Username/Password Authentication protocol
			if (_recvbuf[1] == 0x02)
			{
				p.Reset();
				p.WriteByte(0x01);	// Version 5.

				// add user name
				std::wstring usernameW(_proxySettings->Username->Begin());
				std::string usernameA(usernameW.begin(), usernameW.end());
				const char* srcUsername = usernameA.c_str();

				unsigned char usernameLength = _proxySettings->Username->Length() > 255 ? 255 : _proxySettings->Username->Length();
				p.WriteByte(usernameLength);								// ULEN
				p.WriteBytes((unsigned char*)srcUsername, usernameLength);	// UNAME

				// add password
				std::wstring passwordW(_proxySettings->Password->Begin());
				std::string passwordA(passwordW.begin(), passwordW.end());
				const char* srcPassword = passwordA.c_str();

				unsigned char passwordLength = _proxySettings->Password->Length() > 255 ? 255 : _proxySettings->Password->Length();
				p.WriteByte(passwordLength);								// PLEN
				p.WriteBytes((unsigned char*)srcPassword, passwordLength);	// PASSWD

				// Send the Username/Password request
				err = send(_socket, (const char*)p.GetBuffer(), p.GetLength(), 0);
				if (err < 0)
				{
					printf("send to proxy failed with error: %d\n", err);
				}

				err = recv(_socket, (char*)_recvbuf, sizeof(_recvbuf), 0);
				if (err != 2)
				{
					printf("recv from proxy failed with error: %d\n", err);
					return -16;
				}

				if (_recvbuf[1] != 0x00)
				{
					printf("bad username/password: %d\n", err);
					return -16;
				}
			}

			//// This version only supports connect command. 
			//// UDP and Bind are not supported.

			// Send connect request now...
			p.Reset();
			p.WriteByte(0x05);	// version 5.
			p.WriteByte(0x01);	// command = connect.
			p.WriteByte(0x00);	// Reserve = must be 0x00

			// Destination adress in an IP.
			if (_ipv4)
			{
				// Address is IPV4 format
				std::wstring hostW(_host->Begin());
				std::string hostA(hostW.begin(), hostW.end());

				p.WriteByte(0x01);
				p.WriteInt32(StringToV4Address(hostA));
			}
			else
			{
				// Address is IPV6 format
				std::wstring hostW(_host->Begin());
				std::string hostA(hostW.begin(), hostW.end());
				uint8_t address[16];
				StringToV6Address(hostA, address);

				p.WriteByte(0x04);
				p.WriteBytes(address, 16);
			}

			p.WriteInt16(htons(_port));

			// send connect request.
			err = send(_socket, (const char*)p.GetBuffer(), p.GetLength(), 0);
			if (err < 0)
			{
				printf("send to proxy failed with error: %d\n", err);
			}

			err = recv(_socket, (char*)_recvbuf, sizeof(_recvbuf), 0);
			if (err <= 0)
			{
				printf("recv from proxy failed with error: %d\n", err);
				return -32;
			}
			if (_recvbuf[1] != 0x00)
			{
				printf("recv from proxy failed with proxy error: %d\n", _recvbuf[0]);
				return -32;
			}
			// Success Connected...
		}
	}
	else
	{
		err = Connect(_host, _port, _ipv4);
		if (err < 0)
		{
			printf("connect failed with error: %d\n", err);
			return -64;
		}
	}

	p.Reset();
#if defined(TCP_OBFUSCATED2) || defined(TCP_OBFUSCATED)
	unsigned char buf[64];
	GenerateTCPO2States(buf, &_recvState, &_sendState);
	p.WriteBytes(buf, 64);
#else
	p.WriteByte(0xef);
#endif

	err = send(_socket, (const char*)p.GetBuffer(), p.GetLength(), 0);
	if (err <= 0)
	{
		printf("send failed with error: %d\n", err);
		return -128;
	}

	return err;
}

int ConnectionSocket::Connect(const addrinfo *info)
{
	int err = -2;
	LOG(L"Socket begin");
	_socket = socket(info->ai_addr->sa_family, info->ai_socktype, info->ai_protocol);
	LOG(L"Socket end");

	LOG(L"Connect");
	err = connect(_socket, info->ai_addr, info->ai_addrlen);

	return err;
}

int ConnectionSocket::Connect(Platform::String^ address, int port, bool ipv4)
{
	int err = -2;
	std::wstring addressW(address->Begin());
	std::string addressA(addressW.begin(), addressW.end());

	if (ipv4)
	{
		LOG(L"Socket begin");
		_socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
		LOG(L"Socket end");


		sockaddr_in addr;
		ZeroMemory(&addr, 0, sizeof(addr));
		addr.sin_family = AF_INET;
		addr.sin_addr.s_addr = inet_addr(addressA.c_str());
		addr.sin_port = htons(port);

		LOG(L"Connect");
		err = connect(_socket, (sockaddr*)&addr, sizeof(addr));
	}
	else
	{
		_socket = socket(AF_INET6, SOCK_STREAM, IPPROTO_TCP);

		sockaddr_in6 addr;
		ZeroMemory(&addr, sizeof(addr));
		addr.sin6_family = AF_INET6;
		int size = sizeof(addr);
#if WINAPI_FAMILY==WINAPI_FAMILY_PHONE_APP
		WSAStringToAddressW((wchar_t*)addressW.c_str(), AF_INET6, NULL, (sockaddr*)&addr, &size);
#else
		WSAStringToAddressA((char*)address.c_str(), AF_INET6, NULL, (sockaddr*)&addr, &size);
#endif
		addr.sin6_flowinfo = 0;
		addr.sin6_scope_id = 0;
		addr.sin6_port = htons(port);

		err = connect(_socket, (sockaddr*)&addr, sizeof(addr));
	}

	return err;
}

void ConnectionSocket::GenerateTCPO2States(unsigned char* buffer, TCPO2State* recvState, TCPO2State* sendState)
{
	memset(recvState, 0, sizeof(TCPO2State));
	memset(sendState, 0, sizeof(TCPO2State));
	unsigned char nonce[64];
	unsigned char* zero = reinterpret_cast<unsigned char*>(nonce);
	uint32_t *first = reinterpret_cast<uint32_t*>(nonce);
	uint32_t *second = first + 1;
	uint32_t reserved11 = 0x44414548U;
	uint32_t reserved12 = 0x54534f50U;
	uint32_t reserved13 = 0x20544547U;
	uint32_t reserved14 = 0xEEEEEEEEU;
	uint32_t reserved21 = 0x00000000U;
	do {
		ConnectionSocket::crypto.rand_bytes(nonce, sizeof(nonce));
	} while (*zero == 0xef
		|| *first == reserved11
		|| *first == reserved12
		|| *first == reserved13
		|| *first == reserved14
		|| *second == reserved21);

#if defined(TCP_OBFUSCATED2)
	// prepare encryption key/iv
	if (_protocolSecret != nullptr
		&& _protocolSecret->Length == 16)
	{
		memcpy(sendState->key, nonce + 8, 32);
		memcpy(sendState->iv, nonce + 8 + 32, 16);

		byte buffer[32];
		memcpy(buffer, sendState->key, 32);

		BufferOutputStream payload(64);
		payload.WriteBytes(buffer, 32);
		payload.WriteBytes(_protocolSecret->Data, _protocolSecret->Length);

		ConnectionSocket::crypto.sha256(payload.GetBuffer(), payload.GetLength(), sendState->key);
	}
	else
	{
		memcpy(sendState->key, nonce + 8, 32);
		memcpy(sendState->iv, nonce + 8 + 32, 16);
	}

	// prepare decryption key/iv
	char reversed[48];
	memcpy(reversed, nonce + 8, sizeof(reversed));
	std::reverse(reversed, reversed + sizeof(reversed));
	if (_protocolSecret != nullptr
		&& _protocolSecret->Length == 16)
	{
		memcpy(recvState->key, reversed, 32);
		memcpy(recvState->iv, reversed + 32, 16);

		byte buffer[32];
		memcpy(buffer, recvState->key, 32);

		BufferOutputStream payload(64);
		payload.WriteBytes(buffer, 32);
		payload.WriteBytes(_protocolSecret->Data, _protocolSecret->Length);

		ConnectionSocket::crypto.sha256(payload.GetBuffer(), payload.GetLength(), recvState->key);
	}
	else
	{
		memcpy(recvState->key, reversed, 32);
		memcpy(recvState->iv, reversed + 32, 16);
	}

	// write protocol identifier
	*reinterpret_cast<uint32_t*>(nonce + 56) = 0xEFEFEFEFU;

	// write dc identifier
	*reinterpret_cast<int16*>(nonce + 60) = _protocolDCId;

	memcpy(buffer, nonce, 56);
	EncryptForTCPO2(nonce, sizeof(nonce), sendState);
	memcpy(buffer + 56, nonce + 56, 8);
#else
	// write protocol identifier
	*reinterpret_cast<uint32_t*>(nonce + 56) = 0xEFEFEFEFU;
	memcpy(buffer, nonce, 64);
#endif
}

void ConnectionSocket::EncryptForTCPO2(unsigned char *buffer, size_t len, TCPO2State *state)
{
	ConnectionSocket::crypto.aes_ctr_encrypt(buffer, len, state->key, state->iv, state->ecount, &state->num);
}

uint32_t ConnectionSocket::StringToV4Address(std::string address)
{
	sockaddr_in addr;
	ZeroMemory(&addr, sizeof(addr));
	addr.sin_family = AF_INET;
	int size = sizeof(addr);
#if WINAPI_FAMILY==WINAPI_FAMILY_PHONE_APP
	wchar_t buf[INET_ADDRSTRLEN];
	MultiByteToWideChar(CP_UTF8, 0, address.c_str(), -1, buf, INET_ADDRSTRLEN);
	WSAStringToAddressW(buf, AF_INET, NULL, (sockaddr*)&addr, &size);
#else
	WSAStringToAddressA((char*)address.c_str(), AF_INET, NULL, (sockaddr*)&addr, &size);
#endif
	return addr.sin_addr.s_addr;
}

void ConnectionSocket::StringToV6Address(std::string address, unsigned char *out)
{
	sockaddr_in6 addr;
	ZeroMemory(&addr, sizeof(addr));
	addr.sin6_family = AF_INET6;
	int size = sizeof(addr);
#if WINAPI_FAMILY==WINAPI_FAMILY_PHONE_APP
	wchar_t buf[INET6_ADDRSTRLEN];
	MultiByteToWideChar(CP_UTF8, 0, address.c_str(), -1, buf, INET6_ADDRSTRLEN);
	WSAStringToAddressW(buf, AF_INET6, NULL, (sockaddr*)&addr, &size);
#else
	WSAStringToAddressA((char*)address.c_str(), AF_INET, NULL, (sockaddr*)&addr, &size);
#endif
	memcpy(out, addr.sin6_addr.s6_addr, 16);
}
