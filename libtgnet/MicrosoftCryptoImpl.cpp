// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"
#include "MicrosoftCryptoImpl.h"
#include <stdlib.h>
#include <string.h>
#include <windows.h>
#include <vector>
#include <string>
#include <collection.h>
#include <wrl.h>  
#include <robuffer.h> 

using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Security::Cryptography;
using namespace Windows::Security::Cryptography::Core;
using namespace Windows::Storage::Streams;
using namespace Windows::Data::Json;
using namespace Windows::Phone::Media::Devices;

HashAlgorithmProvider^ MicrosoftCryptoImpl::sha1Provider;
HashAlgorithmProvider^ MicrosoftCryptoImpl::sha256Provider;
SymmetricKeyAlgorithmProvider^ MicrosoftCryptoImpl::aesKeyProvider;

void MicrosoftCryptoImpl::AesIgeEncrypt(uint8_t* in, uint8_t* out, size_t len, uint8_t* key, uint8_t* iv){
	IBuffer^ keybuf = IBufferFromPtr(key, 32);
	CryptographicKey^ _key = aesKeyProvider->CreateSymmetricKey(keybuf);
	uint8_t tmpOut[16];
	uint8_t* xPrev = iv + 16;
	uint8_t* yPrev = iv;
	uint8_t x[16];
	uint8_t y[16];
	for (size_t offset = 0; offset<len; offset += 16){
		for (size_t i = 0; i<16; i++){
			if (offset + i < len){
				x[i] = in[offset + i];
			}
			else{
				x[i] = 0;
			}
		}
		XorInt128(x, yPrev, y);
		IBuffer^ inbuf = IBufferFromPtr(y, 16);
		IBuffer^ outbuf = CryptographicEngine::Encrypt(_key, inbuf, nullptr);
		IBufferToPtr(outbuf, 16, tmpOut);
		XorInt128(tmpOut, xPrev, y);
		memcpy(xPrev, x, 16);
		memcpy(yPrev, y, 16);
		memcpy(out + offset, y, 16);
	}
}

void MicrosoftCryptoImpl::AesIgeDecrypt(uint8_t* in, uint8_t* out, size_t len, uint8_t* key, uint8_t* iv){
	IBuffer^ keybuf = IBufferFromPtr(key, 32);
	CryptographicKey^ _key = aesKeyProvider->CreateSymmetricKey(keybuf);
	uint8_t tmpOut[16];
	uint8_t* xPrev = iv;
	uint8_t* yPrev = iv + 16;
	uint8_t x[16];
	uint8_t y[16];
	for (size_t offset = 0; offset<len; offset += 16){
		for (size_t i = 0; i<16; i++){
			if (offset + i < len){
				x[i] = in[offset + i];
			}
			else{
				x[i] = 0;
			}
		}
		XorInt128(x, yPrev, y);
		IBuffer^ inbuf = IBufferFromPtr(y, 16);
		IBuffer^ outbuf = CryptographicEngine::Decrypt(_key, inbuf, nullptr);
		IBufferToPtr(outbuf, 16, tmpOut);
		XorInt128(tmpOut, xPrev, y);
		memcpy(xPrev, x, 16);
		memcpy(yPrev, y, 16);
		memcpy(out + offset, y, 16);
	}
}

#define GETU32(pt) (((uint32_t)(pt)[0] << 24) ^ ((uint32_t)(pt)[1] << 16) ^ ((uint32_t)(pt)[2] <<  8) ^ ((uint32_t)(pt)[3]))
#define PUTU32(ct, st) { (ct)[0] = (u8)((st) >> 24); (ct)[1] = (u8)((st) >> 16); (ct)[2] = (u8)((st) >> 8); (ct)[3] = (u8)(st); }

typedef  uint8_t u8;

/* increment counter (128-bit int) by 1 */
static void AES_ctr128_inc(unsigned char *counter) {
	unsigned long c;

	/* Grab bottom dword of counter and increment */
	c = GETU32(counter + 12);
	c++;	c &= 0xFFFFFFFF;
	PUTU32(counter + 12, c);

	/* if no overflow, we're done */
	if (c)
		return;

	/* Grab 1st dword of counter and increment */
	c = GETU32(counter + 8);
	c++;	c &= 0xFFFFFFFF;
	PUTU32(counter + 8, c);

	/* if no overflow, we're done */
	if (c)
		return;

	/* Grab 2nd dword of counter and increment */
	c = GETU32(counter + 4);
	c++;	c &= 0xFFFFFFFF;
	PUTU32(counter + 4, c);

	/* if no overflow, we're done */
	if (c)
		return;

	/* Grab top dword of counter and increment */
	c = GETU32(counter + 0);
	c++;	c &= 0xFFFFFFFF;
	PUTU32(counter + 0, c);
}

void MicrosoftCryptoImpl::AesCtrEncrypt(uint8_t* inout, size_t len, uint8_t* key, uint8_t* counter, uint8_t* ecount_buf, uint32_t* num){
	unsigned int n;
	unsigned long l = len;

	//assert(in && out && key && counter && num);
	//assert(*num < AES_BLOCK_SIZE);

	IBuffer^ keybuf = IBufferFromPtr(key, 32);
	CryptographicKey^ _key = aesKeyProvider->CreateSymmetricKey(keybuf);

	n = *num;

	while (l--) {
		if (n == 0) {
			IBuffer^ inbuf = IBufferFromPtr(counter, 16);
			IBuffer^ outbuf = CryptographicEngine::Encrypt(_key, inbuf, nullptr);
			IBufferToPtr(outbuf, 16, ecount_buf);
			//AES_encrypt(counter, ecount_buf, key);
			AES_ctr128_inc(counter);
		}
		*inout = *(inout++) ^ ecount_buf[n];
		n = (n + 1) % 16;
	}

	*num = n;
}

void MicrosoftCryptoImpl::SHA1(uint8_t* msg, size_t len, uint8_t* out){
	//EnterCriticalSection(&hashMutex);

	IBuffer^ arr = IBufferFromPtr(msg, len);
	CryptographicHash^ hash = sha1Provider->CreateHash();
	hash->Append(arr);
	IBuffer^ res = hash->GetValueAndReset();
	IBufferToPtr(res, 20, out);

	//LeaveCriticalSection(&hashMutex);
}

void MicrosoftCryptoImpl::SHA256(uint8_t* msg, size_t len, uint8_t* out){
	//EnterCriticalSection(&hashMutex);

	IBuffer^ arr = IBufferFromPtr(msg, len);
	CryptographicHash^ hash = sha256Provider->CreateHash();
	hash->Append(arr);
	IBuffer^ res = hash->GetValueAndReset();
	IBufferToPtr(res, 32, out);
	//LeaveCriticalSection(&hashMutex);
}

void MicrosoftCryptoImpl::RandBytes(uint8_t* buffer, size_t len){
	IBuffer^ res = CryptographicBuffer::GenerateRandom(len);
	IBufferToPtr(res, len, buffer);
}

void MicrosoftCryptoImpl::Init(){
	/*sha1Hash=HashAlgorithmProvider::OpenAlgorithm(HashAlgorithmNames::Sha1)->CreateHash();
	sha256Hash=HashAlgorithmProvider::OpenAlgorithm(HashAlgorithmNames::Sha256)->CreateHash();*/
	sha1Provider = HashAlgorithmProvider::OpenAlgorithm(HashAlgorithmNames::Sha1);
	sha256Provider = HashAlgorithmProvider::OpenAlgorithm(HashAlgorithmNames::Sha256);
	aesKeyProvider = SymmetricKeyAlgorithmProvider::OpenAlgorithm(SymmetricAlgorithmNames::AesEcb);
}

void MicrosoftCryptoImpl::XorInt128(uint8_t* a, uint8_t* b, uint8_t* out){
	uint64_t* _a = reinterpret_cast<uint64_t*>(a);
	uint64_t* _b = reinterpret_cast<uint64_t*>(b);
	uint64_t* _out = reinterpret_cast<uint64_t*>(out);
	_out[0] = _a[0] ^ _b[0];
	_out[1] = _a[1] ^ _b[1];
}

void MicrosoftCryptoImpl::IBufferToPtr(IBuffer^ buffer, size_t len, uint8_t* out)
{
	ComPtr<IBufferByteAccess> bufferByteAccess;
	reinterpret_cast<IInspectable*>(buffer)->QueryInterface(IID_PPV_ARGS(&bufferByteAccess));

	byte* hashBuffer;
	bufferByteAccess->Buffer(&hashBuffer);
	CopyMemory(out, hashBuffer, len);
}

IBuffer^ MicrosoftCryptoImpl::IBufferFromPtr(uint8_t* msg, size_t len)
{
	ComPtr<NativeBuffer> nativeBuffer = Make<NativeBuffer>((byte *)msg, len);
	return reinterpret_cast<IBuffer^>(nativeBuffer.Get());
}
