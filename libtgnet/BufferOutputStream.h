// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once
#include <stdlib.h>
#include <stdint.h>

namespace libtgnet
{
	class BufferOutputStream{

	public:
		BufferOutputStream(size_t size);
		BufferOutputStream(unsigned char* buf, size_t size);
		~BufferOutputStream();
		void WriteByte(unsigned char byte);
		void WriteInt64(int64_t i);
		void WriteInt32(int32_t i);
		void WriteInt16(int16_t i);
		void WriteBytes(unsigned char* bytes, size_t count);
		unsigned char* GetBuffer();
		size_t GetLength();
		void Reset();

	private:
		void ExpandBufferIfNeeded(size_t need);
		unsigned char* buffer;
		size_t size;
		size_t offset;
	};
}