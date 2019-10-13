#pragma once

namespace FFmpegInterop
{
	public ref class FFmpegGifDecoder sealed
	{
	public:
		static int FFmpegGifDecoder::CreateDecoder(Platform::String^ src, Platform::WriteOnlyArray<int>^ data);
		static void FFmpegGifDecoder::DestroyDecoder(int ptr);
		static Platform::Array<uint8_t>^ FFmpegGifDecoder::GetVideoFrame(int ptr, Platform::WriteOnlyArray<int>^ data);
	};
}

