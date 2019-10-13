// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "BackEndTransport.h"
#include "BackEndNativeBuffer.h"
#include <ppltasks.h>

using namespace PhoneVoIPApp::BackEnd;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Windows::System::Threading;
using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Details;

BackEndTransport::BackEndTransport()
{
}


void BackEndTransport::WriteAudio(BYTE* bytes, int byteCount)
{
    Write(bytes, byteCount, TransportMessageType::Audio, 0, 0);
}

void BackEndTransport::WriteVideo(BYTE* bytes, int byteCount, UINT64 hnsPresenationTime, UINT64 hnsSampleDuration)

{
    Write(bytes, byteCount, TransportMessageType::Video, hnsPresenationTime, hnsSampleDuration);
}

void BackEndTransport::Write(BYTE* bytes, int byteCount, TransportMessageType::Value dataType, UINT64 hnsPresenationTime, UINT64 hnsSampleDuration)
{

    static const int MaxPacketSize = 10*1024*1024;

    int bytesToSend = byteCount;

    while (bytesToSend)
    {
        int chunkSize = bytesToSend > MaxPacketSize ? MaxPacketSize : bytesToSend;
        ComPtr<NativeBuffer> spNativeBuffer = NULL;
        if (dataType == TransportMessageType::Audio)
        {
            MakeAndInitialize<NativeBuffer>(&spNativeBuffer, bytes, chunkSize, FALSE);
            AudioMessageReceived(NativeBuffer::GetIBufferFromNativeBuffer(spNativeBuffer), hnsPresenationTime, hnsSampleDuration);
        }
        else
        {
            // Temporarily duplicating this for sample so that MSS can own this
            // buffer, and will be released when the stream itself is released
            BYTE* pMem = new BYTE[chunkSize];
                
            memcpy((void*) pMem, (void*) bytes, chunkSize);

            MakeAndInitialize<NativeBuffer>(&spNativeBuffer, pMem, chunkSize, TRUE);
            VideoMessageReceived(NativeBuffer::GetIBufferFromNativeBuffer(spNativeBuffer), hnsPresenationTime, hnsSampleDuration);
        }

        // Increment byte position
        bytes += chunkSize;
        bytesToSend -= chunkSize;            
    }
    return;

}

BackEndTransport::~BackEndTransport()
{
}
