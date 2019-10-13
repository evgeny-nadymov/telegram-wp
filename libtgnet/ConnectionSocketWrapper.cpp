#include "pch.h"

#include <sstream>
#include <chrono>
#include <iostream>
#include "ConnectionSocketWrapper.h"
#include "MicrosoftCryptoImpl.h"

using namespace libtgnet;


Platform::Array<uint8_t>^ ConnectionSocketWrapper::AesCtr(const Platform::Array<uint8_t>^ data, TCPO2StateWrapper^ state)
{
	MicrosoftCryptoImpl::Init();

	auto result = ref new Platform::Array<uint8_t>(data);
	auto key = ref new Platform::Array<uint8_t>(state->Key);
	auto iv = ref new Platform::Array<uint8_t>(state->IV);
	auto ecount = ref new Platform::Array<uint8_t>(state->Ecount);
	uint32_t num = state->Num;
	MicrosoftCryptoImpl::AesCtrEncrypt(result->Data, result->Length, key->Data, iv->Data, ecount->Data, &num);

	return result;
}

ConnectionSocketWrapper::ConnectionSocketWrapper(ConnectionSettings^ connectionSettings, ProxySettings^ proxySettings)
{
	_socket = new ConnectionSocket(connectionSettings, proxySettings);
	_host = connectionSettings->Host;
}

ConnectionSocketWrapper::~ConnectionSocketWrapper()
{
	delete _socket;
	_socket = nullptr;
}

void ConnectionSocketWrapper::LOG(std::wstring str)
{
	std::wostringstream stream;
	stream << L"[libtgnet::ConnectionSocketWrapper " + std::wstring(_host->Begin()) + L"] => " << _socket << L" " << str << L"\n";

	::OutputDebugString(stream.str().c_str());
}

int ConnectionSocketWrapper::Connect()
{
	if (_socket == nullptr) return -2;

	//LOG(L"Connect start");
	auto result = _socket->Connect();
	//LOG(L"Connect end");

	return result;
}

int ConnectionSocketWrapper::SendPacket(const Platform::Array<uint8>^ data)
{
	if (_sendTime == 0)
	{
		_sendTime = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
	}

	if (_socket == nullptr) return -2;

	//LOG(L"SendPacket begin");
	auto result = _socket->SendPacket(data);
	//LOG(L"SendPacket end");

	return result;
}

void ConnectionSocketWrapper::StartReceive()
{
	LOG(L"StartReceive");
	while (true)
	{
		if (_socket == nullptr) return;

		//LOG(L"Receive begin");
		Platform::Array<uint8_t>^ result = _socket->Receive();
		//LOG(L"Receive end");
		if (result == nullptr)
		{
			Closed(this);
			return;
		}
		else
		{
			if (_receiveTime == 0)
			{
				_receiveTime = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
			}
			PacketReceived(this, result);
		}
	}
}

uint64_t ConnectionSocketWrapper::GetPing()
{
	if (_sendTime > 0 && _sendTime < _receiveTime)
	{
		return _receiveTime - _sendTime;
	}

	return 0;
}

void ConnectionSocketWrapper::Close()
{
	if (_socket == nullptr) return;

	//LOG(L"Close begin");
	_socket->Close();
	//LOG(L"Close end");
}
