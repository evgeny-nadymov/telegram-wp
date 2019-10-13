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
	public ref class ImageUtils sealed
	{
	public:
		ImageUtils();

		Platform::Array<uint8>^ FastBlur(int width, int height, int stride, const Platform::Array<uint8>^ pixels);
		Platform::Array<uint8>^ FastSecretBlur(int width, int height, int stride, const Platform::Array<uint8>^ pixels);
	};
}