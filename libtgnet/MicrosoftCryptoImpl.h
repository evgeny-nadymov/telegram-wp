// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once
#include <stdint.h>
#include <wrl.h>
#include <wrl/implements.h>
#include <windows.storage.streams.h>
#include <robuffer.h>
#include <vector>

ref class MicrosoftCryptoImpl{
public:
	static void AesIgeEncrypt(uint8_t* in, uint8_t* out, size_t len, uint8_t* key, uint8_t* iv);
	static void AesIgeDecrypt(uint8_t* in, uint8_t* out, size_t len, uint8_t* key, uint8_t* iv);
	static void AesCtrEncrypt(uint8_t* inout, size_t len, uint8_t* key, uint8_t* iv, uint8_t* ecount, uint32_t* num);
	static void SHA1(uint8_t* msg, size_t len, uint8_t* out);
	static void SHA256(uint8_t* msg, size_t len, uint8_t* out);
	static void RandBytes(uint8_t* buffer, size_t len);
	static void Init();
private:
	static inline void XorInt128(uint8_t* a, uint8_t* b, uint8_t* out);
	static void IBufferToPtr(Windows::Storage::Streams::IBuffer^ buffer, size_t len, uint8_t* out);
	static Windows::Storage::Streams::IBuffer^ IBufferFromPtr(uint8_t* msg, size_t len);
	/*static Windows::Security::Cryptography::Core::CryptographicHash^ sha1Hash;
	static Windows::Security::Cryptography::Core::CryptographicHash^ sha256Hash;*/
	static Windows::Security::Cryptography::Core::HashAlgorithmProvider^ sha1Provider;
	static Windows::Security::Cryptography::Core::HashAlgorithmProvider^ sha256Provider;
	static Windows::Security::Cryptography::Core::SymmetricKeyAlgorithmProvider^ aesKeyProvider;
};

class NativeBuffer :
	public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::RuntimeClassType::WinRtClassicComMix>,
	ABI::Windows::Storage::Streams::IBuffer,
	Windows::Storage::Streams::IBufferByteAccess>
{
public:
	NativeBuffer(byte *buffer, UINT totalSize)
	{
		m_length = totalSize;
		m_buffer = buffer;
	}

	virtual ~NativeBuffer()
	{
	}

	STDMETHODIMP RuntimeClassInitialize(byte *buffer, UINT totalSize)
	{
		m_length = totalSize;
		m_buffer = buffer;
		return S_OK;
	}

	STDMETHODIMP Buffer(byte **value)
	{
		*value = m_buffer;
		return S_OK;
	}

	STDMETHODIMP get_Capacity(UINT32 *value)
	{
		*value = m_length;
		return S_OK;
	}

	STDMETHODIMP get_Length(UINT32 *value)
	{
		*value = m_length;
		return S_OK;
	}

	STDMETHODIMP put_Length(UINT32 value)
	{
		m_length = value;
		return S_OK;
	}

private:
	UINT32 m_length;
	byte *m_buffer;
};

