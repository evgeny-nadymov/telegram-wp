// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"
//#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>

#include "ConnectionSocket.h"

#pragma comment(lib,"WS2_32")

using namespace TelegramClient_Native;

void ConnectionSocket::SendPacket(Platform::String^ caption, const Platform::Array<uint8>^ data)
{
	
}

void ConnectionSocket::Close()
{
	int err = closesocket(_socket);
	if (err < 0)
	{
	printf("closesocket failed with error: %d\n", err);
	return;
	}
}

ConnectionSocket::ConnectionSocket(Platform::String^ host, int port)
{
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

	_socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	sockaddr_in serverAddr;
	memset(&serverAddr, 0, sizeof(serverAddr));
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_addr.s_addr = inet_addr("149.154.175.50");
	serverAddr.sin_port = htons(443);

	err = connect(_socket, (sockaddr*)&serverAddr, sizeof(serverAddr));
	if (err < 0)
	{
		printf("connect failed with error: %d\n", err);
		return;
	}

	/*err = closesocket(_socket);
	if (err < 0)
	{
		printf("closesocket failed with error: %d\n", err);
		return;
	}*/
	//int sockbufsize = 0, size = sizeof(int);
	//err = getsockopt(skt, SOL_SOCKET, SO_RCVBUF, (char *)&sockbufsize, &size);

	//WSACleanup();
}

int ConnectionSocket::GetReceiveBufferSize()
{
	int sockbufsize = 0, size = sizeof(int);
	int err = getsockopt(_socket, SOL_SOCKET, SO_RCVBUF, (char *)&sockbufsize, &size);

	return sockbufsize;
}

int ConnectionSocket::GetSendBufferSize()
{
	int sockbufsize = 0, size = sizeof(int);
	int err = getsockopt(_socket, SOL_SOCKET, SO_SNDBUF, (char *)&sockbufsize, &size);

	return sockbufsize;
}
