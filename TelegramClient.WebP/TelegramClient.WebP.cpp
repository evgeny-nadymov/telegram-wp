// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"
#include "TelegramClient.WebP.h"
#include <windows.h>
#include <malloc.h>

using namespace TelegramClient_WebP::LibWebP;
using namespace Platform;

int(*WebPGetDecoderVersion) ();
int (*WebPGetInfo) (void* data, unsigned int data_size, int* width, int* height);
void* (*WebPDecodeRGBInto) (void* data, unsigned int data_size, void* output_buffer, int output_buffer_size, int output_stride);
void* (*WebPDecodeBGRInto) (void* data, unsigned int data_size, void* output_buffer, int output_buffer_size, int output_stride);
void* (*WebPDecodeBGRAInto) (void* data, unsigned int data_size, void* output_buffer, int output_buffer_size, int output_stride);
void* (*WebPDecodeRgbAInto) (void* data, unsigned int data_size, void* output_buffer, int output_buffer_size, int output_stride);
void* (*WebPDecodeRGBAInto) (void* data, unsigned int data_size, void* output_buffer, int output_buffer_size, int output_stride);
bool loaded = false;

WebPDecoder::WebPDecoder()
{
	if (!loaded)
	{
#if _M_ARM
		auto lib = LoadPackagedLibrary(L"ARM\\libwebp", 0);
#else
		auto lib = LoadPackagedLibrary(L"x86\\libwebp", 0);
#endif
		if (!lib)
			throw ref new Platform::FailureException();
		WebPGetDecoderVersion = (int(*)()) GetProcAddress(lib, "WebPGetDecoderVersion");
		WebPGetInfo =
			(int(*)(
			void* data,
			unsigned int data_size,
			int* width,
			int* height))
			GetProcAddress(lib, "WebPGetInfo");

		WebPDecodeRGBInto =
			(void*(*)(
			void* data,
			unsigned int data_size,
			void* output_buffer,
			int output_buffer_size,
			int output_stride))
			GetProcAddress(lib, "WebPDecodeRGBInto");

		WebPDecodeBGRInto =
			(void*(*) (
			void* data,
			unsigned int data_size,
			void* output_buffer,
			int output_buffer_size,
			int output_stride))
			GetProcAddress(lib, "WebPDecodeBGRInto");

		WebPDecodeBGRAInto =
			(void* (*) (
			void* data,
			unsigned int data_size,
			void* output_buffer,
			int output_buffer_size,
			int output_stride))
			GetProcAddress(lib, "WebPDecodeBGRAInto");

		WebPDecodeRgbAInto =
			(void* (*) (
			void* data,
			unsigned int data_size,
			void* output_buffer,
			int output_buffer_size,
			int output_stride))
			GetProcAddress(lib, "WebPDecodeRgbAInto");

		WebPDecodeRGBAInto =
			(void* (*) (
			void* data,
			unsigned int data_size,
			void* output_buffer,
			int output_buffer_size,
			int output_stride))
			GetProcAddress(lib, "WebPDecodeRGBAInto");

		if (WebPGetDecoderVersion&&
			WebPGetInfo&&
			WebPDecodeRGBInto&&
			WebPDecodeBGRInto&&
			WebPDecodeBGRAInto&&
			WebPDecodeRgbAInto&&
			WebPDecodeRGBAInto)
			loaded = true;
		else
			throw ref new Platform::FailureException();
	}
}


Platform::String^ WebPDecoder::GetDecoderVersion()
{
	return GetDecoderVersion()->ToString();
}
bool WebPDecoder::GetInfo(const Platform::Array<unsigned char>^ Data, int* Width, int* Height)
{
	auto nData = (unsigned char*) malloc(Data->Length*sizeof(unsigned char));
	memcpy(nData, Data->Data, Data->Length);
	auto result = WebPGetInfo(nData, Data->Length, Width, Height);
	free(nData);
	return result;
}

Platform::Array<unsigned char>^ WebPDecoder::Decode(DecodeType type, const Platform::Array<unsigned char>^ Data, int* Width, int* Height)
{
	auto nData = (unsigned char*) malloc(Data->Length*sizeof(unsigned char));
	memcpy(nData, Data->Data, Data->Length);
	int retDataLength = 0;
	unsigned char* retData = NULL;
	if (!GetInfo(Data, Width, Height))
		throw ref new Platform::FailureException();
	switch (type)
	{
	case DecodeType::rgbA:
		retDataLength = (*Width) * (*Height) * 4 * sizeof(unsigned char);
		retData = (unsigned char*)malloc(retDataLength);
		retData = (unsigned char*)WebPDecodeRgbAInto(nData, Data->Length, retData, (*Width) * (*Height) * 4, (*Width) * 4);
		free(nData);
		break;
	case DecodeType::BGR:
		retDataLength = (*Width) * (*Height) * 3 * sizeof(unsigned char);
		retData = (unsigned char*) malloc(retDataLength);
		retData = (unsigned char*) WebPDecodeBGRInto(nData, Data->Length, retData, (*Width) * (*Height) * 3, (*Width) * 3);
		free(nData);
		break;
	case DecodeType::RGB:
		retDataLength = (*Width) * (*Height) * 3 * sizeof(unsigned char);
		retData = (unsigned char*) malloc(retDataLength);
		retData = (unsigned char*) WebPDecodeRGBInto(nData, Data->Length, retData, (*Width) * (*Height) * 3, (*Width) * 3);
		free(nData);
		break;
	case DecodeType::RGBA:
		retDataLength = (*Width) * (*Height) * 4 * sizeof(unsigned char);
		retData = (unsigned char*) malloc(retDataLength);
		retData = (unsigned char*) WebPDecodeRGBAInto(nData, Data->Length, retData, (*Width) * (*Height) * 4, (*Width) * 4);
		free(nData);
		break;
	case DecodeType::BGRA:
		retDataLength = (*Width) * (*Height) * 4 * sizeof(unsigned char);
		retData = (unsigned char*) malloc(retDataLength);
		retData = (unsigned char*) WebPDecodeBGRAInto(nData, Data->Length, retData, (*Width) * (*Height) * 4, (*Width) * 4);
		free(nData);
		break;
	default:
		free(nData);
		throw ref new Platform::NotImplementedException();
	}
	if (retData == NULL)
		throw ref new Platform::FailureException();

	auto retArr = ref new Platform::Array<unsigned char>(retData, retDataLength);
	//for (auto i = 0; i < retDataLength; i++)
	//{
	//	retArr[i] = retData[i];
	//}
	free(retData);
	return retArr;
}

Platform::Array<unsigned char>^ WebPDecoder::DecodeRgbA(const Platform::Array<unsigned char>^ Data, int* Width, int* Height)
{
	auto nData = (unsigned char*)malloc(Data->Length*sizeof(unsigned char));
	memcpy(nData, Data->Data, Data->Length);
	int retDataLength = 0;
	unsigned char* retData = NULL;
	if (!GetInfo(Data, Width, Height))
		throw ref new Platform::FailureException();
	retDataLength = (*Width)* (*Height)* 4 * sizeof(unsigned char);
	retData = (unsigned char*)malloc(retDataLength);
	retData = (unsigned char*)WebPDecodeRgbAInto(nData, Data->Length, retData, (*Width)* (*Height)* 4, (*Width)* 4);
	free(nData);

	if (retData == NULL)
		throw ref new Platform::FailureException();

	auto retArr = ref new Platform::Array<unsigned char>(retData, retDataLength);
	//for (auto i = 0; i < retDataLength; i++)
	//{
	//	retArr[i] = retData[i];
	//}
	free(retData);
	return retArr;
}
