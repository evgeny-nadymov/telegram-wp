// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once
#include "windows.h"

#define MAX_RAW_BUFFER_SIZE 1024*128

#include <synchapi.h>
#include <audioclient.h>
#include <phoneaudioclient.h>

#include "BackEndTransport.h"

namespace PhoneVoIPApp
{
    namespace BackEnd
    {
        //public ref class BackEndAudio sealed
        //{
        //public:
        //    // Constructor
        //    BackEndAudio();

        //    // Destructor
        //    virtual ~BackEndAudio();

        //    void SetTransport(BackEndTransport^ transport);
        //    
        //    void Start();
        //    void Stop();

        //private:
        //    HRESULT InitRender();
        //    HRESULT InitCapture();
        //    HRESULT StartAudioThreads();
        //    void CaptureThread(Windows::Foundation::IAsyncAction^ operation);
        //    void OnTransportMessageReceived(Windows::Storage::Streams::IBuffer^ stream, UINT64, UINT64);
        //    
        //    BackEndTransport^ transportController;

        //    PhoneVoIPApp::BackEnd::MessageReceivedEventHandler^ onTransportMessageReceivedHandler;
        //    Windows::Foundation::EventRegistrationToken onTransportMessageReceivedHandlerToken;

        //    int m_sourceFrameSizeInBytes;

        //    WAVEFORMATEX* m_pwfx;

        //    // Devices
        //    IAudioClient2* m_pDefaultRenderDevice;
        //    IAudioClient2* m_pDefaultCaptureDevice;

        //    // Actual render and capture objects
        //    IAudioRenderClient* m_pRenderClient;
        //    IAudioCaptureClient* m_pCaptureClient;

        //    // Misc interfaces
        //    IAudioClock* m_pClock; 
        //    ISimpleAudioVolume* m_pVolume;

        //    // Audio buffer size
        //    UINT32 m_nMaxFrameCount;
        //    HANDLE hCaptureEvent;

        //    // Event for stopping audio capture/render
        //    HANDLE hShutdownEvent;

        //    Windows::Foundation::IAsyncAction^ m_CaptureThread;

        //    // Has audio started?
        //    bool started;
        //};
    }
}
