// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once

namespace TelegramClient_WebP
{
    namespace LibWebP
	{
		public enum class DecodeType
		{
			RGB,
			RGBA,
			rgbA,
			BGR,
			BGRA,
			YUV
		};

		public ref class WebPDecoder sealed
		{
		public:
			WebPDecoder();
			Platform::String^ GetDecoderVersion();
			bool GetInfo(const Platform::Array<unsigned char>^ Data, int* Width, int* Height);
			Platform::Array<unsigned char>^ Decode(DecodeType type, const Platform::Array<unsigned char>^ Data, int* Width, int* Height); 
			Platform::Array<unsigned char>^ DecodeRgbA(const Platform::Array<unsigned char>^ Data, int* Width, int* Height);
			//Platform::Array<unsigned int>^ WebPDecoder::DecodeToWritableBitmap(const Platform::Array<unsigned char>^ Data, int* Width, int* Height);
			//Platform::Array<int>^ Decode
		};
	}
}