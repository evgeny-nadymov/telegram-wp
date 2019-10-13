// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include <hstring.h>
#include <memory>
#include <activation.h>
#include <wrl\module.h>
#include <crtdbg.h>
#include "Globals.h"
#include "ApiLock.h"
#include "CallController.h"
#include "BackEndAudio.h"
#include "BackEndCapture.h"

using namespace PhoneVoIPApp::BackEnd;
using namespace Windows::Foundation;
using namespace Windows::Phone::Media::Capture;

HRESULT __declspec(dllexport) MyGetActivationFactory(_In_ HSTRING activatableClassId, _COM_Outptr_ IInspectable **factory)
{
    *factory = nullptr;

    Microsoft::WRL::ComPtr<IActivationFactory> activationFactory;
    auto &module = Microsoft::WRL::Module<Microsoft::WRL::InProcDisableCaching>::GetModule();

    HRESULT hr = module.GetActivationFactory(activatableClassId, &activationFactory);
    if (SUCCEEDED(hr))
    {
        *factory = activationFactory.Detach();

        if (*factory == nullptr)
        {
            return E_OUTOFMEMORY;
        }
    }

    return hr;
}

// Maximum number of characters required to contain the string version of an unsigned integer
#define MAX_CHARS_IN_UINT_AS_STRING ((sizeof(unsigned int) * 4) + 1)

const LPCWSTR Globals::noOtherBackgroundProcessEventName = L"PhoneVoIPApp.noOtherBackgroundProcess";
const LPCWSTR Globals::uiDisconnectedEventName = L"PhoneVoIPApp.uiDisconnected.";
const LPCWSTR Globals::backgroundProcessReadyEventName = L"PhoneVoIPApp.backgroundProcessReady.";
Globals^ Globals::singleton = nullptr;

void Globals::StartServer(const Platform::Array<Platform::String^>^ outOfProcServerClassNames)
{
    HRESULT hr = S_OK;

    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (outOfProcServerClassNames == nullptr)
    {
        throw ref new Platform::InvalidArgumentException(L"outOfProcServerClassNames cannot be null");
    }

    if (this->started)
    {
        return; // Nothing more to be done
    }

    // Set an event that indicates that the background process is ready.
    this->backgroundReadyEvent = ::CreateEventEx(
        NULL,
        Globals::GetBackgroundProcessReadyEventName(Globals::GetCurrentProcessId())->Data(),
        CREATE_EVENT_INITIAL_SET | CREATE_EVENT_MANUAL_RESET,
        EVENT_ALL_ACCESS);
    if (this->backgroundReadyEvent == NULL)
    {
        // Something went wrong
        DWORD dwErr = ::GetLastError();
        hr = HRESULT_FROM_WIN32(dwErr);
        throw ref new Platform::COMException(hr, L"An error occurred trying to create an event that indicates that the background process is ready");
    }

    // Set the event
    BOOL success = ::SetEvent(this->backgroundReadyEvent);
    if (success == FALSE)
    {
        DWORD dwErr = ::GetLastError();
        hr = HRESULT_FROM_WIN32(dwErr);
        throw ref new Platform::COMException(hr, L"An error occurred trying to set an event that indicates that the background process is ready");
    }

    this->started = true;
}

void Globals::DoPeriodicKeepAlive()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // TODO: Do stuff here - refresh tokens, get new certs from server, etc.
}

unsigned int Globals::GetCurrentProcessId()
{
    return ::GetCurrentProcessId();
}

Platform::String^ Globals::GetUiDisconnectedEventName(unsigned int backgroundProcessId)
{
    WCHAR backgroundProcessIdString[MAX_CHARS_IN_UINT_AS_STRING];
    if (swprintf_s<_countof(backgroundProcessIdString)>(backgroundProcessIdString, L"%u", backgroundProcessId) < 0)
        throw ref new Platform::FailureException(L"Could not create string version of background process id");

    auto eventName = ref new Platform::String(Globals::uiDisconnectedEventName) + ref new Platform::String(backgroundProcessIdString);
    return eventName;
}

Platform::String^ Globals::GetBackgroundProcessReadyEventName(unsigned int backgroundProcessId)
{
    WCHAR backgroundProcessIdString[MAX_CHARS_IN_UINT_AS_STRING];
    if (swprintf_s<_countof(backgroundProcessIdString)>(backgroundProcessIdString, L"%u", backgroundProcessId) < 0)
        throw ref new Platform::FailureException(L"Could not create string version of background process id");

    auto eventName = ref new Platform::String(Globals::backgroundProcessReadyEventName) + ref new Platform::String(backgroundProcessIdString);
    return eventName;
}

Globals^ Globals::Instance::get()
{
    if (Globals::singleton == nullptr)
    {
        // Make sure only one API call is in progress at a time
        std::lock_guard<std::recursive_mutex> lock(g_apiLock);

        if (Globals::singleton == nullptr)
        {
            Globals::singleton = ref new Globals();
        }
        // else: some other thread has created an instance of the call controller
    }

    return Globals::singleton;
}

CallController^ Globals::CallController::get()
{
    if (this->callController == nullptr)
    {
        // Make sure only one API call is in progress at a time
        std::lock_guard<std::recursive_mutex> lock(g_apiLock);

        if (this->callController == nullptr)
        {
            this->callController = ref new PhoneVoIPApp::BackEnd::CallController();
        }
        // else: some other thread has created an instance of the call controller
    }

    return this->callController;
}

IVideoRenderer^ Globals::VideoRenderer::get()
{
    // No need to lock - this get is idempotent
    return this->videoRenderer;
}

void Globals::VideoRenderer::set(IVideoRenderer^ value)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    this->videoRenderer = value;
}

IMTProtoUpdater^ Globals::MTProtoUpdater::get()
{
	// No need to lock - this get is idempotent
	return this->mtProtoUpdater;
}

void Globals::MTProtoUpdater::set(IMTProtoUpdater^ value)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	this->mtProtoUpdater = value;
}

//BackEndAudio^ Globals::AudioController::get()
//{
//    // No need to lock - this get is idempotent
//    return this->audioController;
//}

BackEndCapture^ Globals::CaptureController::get()
{
    // No need to lock - this get is idempotent
    return this->captureController;
}

BackEndTransport^ Globals::TransportController::get()
{
    // No need to lock - this get is idempotent
    return this->transportController;
}

Globals::Globals() :
    started(false),
    serverRegistrationCookie(NULL),
    callController(nullptr),
    videoRenderer(nullptr),
    noOtherBackgroundProcessEvent(NULL),
    backgroundReadyEvent(NULL),
    //audioController(nullptr),
    transportController(nullptr),
    captureController(nullptr)
{
    {
        WCHAR szBuffer[256];
        swprintf_s<ARRAYSIZE(szBuffer)>(szBuffer, L"[Globals::Globals] => VoIP background process with id %d starting up\n", this->GetCurrentProcessId());
        ::OutputDebugString(szBuffer);
    }

    // Create an event that indicates if any other VoIP background exits or not
    this->noOtherBackgroundProcessEvent = ::CreateEventEx(NULL, Globals::noOtherBackgroundProcessEventName, CREATE_EVENT_INITIAL_SET | CREATE_EVENT_MANUAL_RESET, EVENT_ALL_ACCESS);
    if (this->noOtherBackgroundProcessEvent == NULL)
    {
        // Something went wrong
        DWORD dwErr = ::GetLastError();
        HRESULT hr = HRESULT_FROM_WIN32(dwErr);
        throw ref new Platform::COMException(hr, L"An error occurred trying to create an event that indicates if the background process exists or not");
    }

    // Wait for up to 30 seconds for the event to become set - if another instance of this process exists, this event would be in the reset state
    DWORD reason = ::WaitForSingleObjectEx(this->noOtherBackgroundProcessEvent, 30 * 1000, FALSE);
    _ASSERT(reason != WAIT_FAILED); // We don't care about any of the other reasons why WaitForSingleObjectEx returned
    if (reason == WAIT_TIMEOUT)
    {
        throw ref new Platform::FailureException(L"Another instance of the VoIP background process exists and that process did not exit within 30 seconds. Cannot continue.");
    }

    // Reset the event to indicate that there is a VoIP background process
    BOOL success = ::ResetEvent(this->noOtherBackgroundProcessEvent);
    if (success == FALSE)
    {
        // Something went wrong
        DWORD dwErr = ::GetLastError();
        HRESULT hr = HRESULT_FROM_WIN32(dwErr);
        throw ref new Platform::COMException(hr, L"An error occurred trying to reset the event that indicates if the background process exists or not");
    }
    
    // Initialize transport
    this->transportController = ref new BackEndTransport(); // local

    // Initialize audio controller
    //this->audioController = ref new BackEndAudio();

    // Set the transport for audio
    //this->audioController->SetTransport(this->transportController);

    // Initialize capture controller
    this->captureController = ref new BackEndCapture();

    // Set the transport on the controller
    this->captureController->SetTransport(this->transportController);
    
    // Initialize the call controller
    this->callController = ref new PhoneVoIPApp::BackEnd::CallController();    
}

Globals::~Globals()
{
    // The destructor of this singleton object is called when the process is shutting down.

    // Before shutting down, make sure the UI process is not connected
    HANDLE uiDisconnectedEvent = ::OpenEvent(EVENT_ALL_ACCESS, FALSE, Globals::GetUiDisconnectedEventName(Globals::GetCurrentProcessId())->Data());
    if (uiDisconnectedEvent != NULL)
    {
        // The event exists - wait for it to get signaled (for a maximum of 30 seconds)
        DWORD reason = ::WaitForSingleObjectEx(uiDisconnectedEvent, 30 * 1000, FALSE);
        _ASSERT(reason != WAIT_FAILED); // We don't care about any of the other reasons why WaitForSingleObjectEx returned
    }

    // At this point, the UI is no longer connected to the background process.
    // It is possible that the UI now reconnects to the background process - this would be a bug,
    // and we should exit the background process anyway.

    // Unset the event that indicates that the background process is ready
    BOOL success;
    if (this->backgroundReadyEvent != NULL)
    {
        success = ::ResetEvent(this->backgroundReadyEvent);
        _ASSERT(success);

        ::CloseHandle(this->backgroundReadyEvent);
        this->backgroundReadyEvent = NULL;
    }

    // Unregister the activation factories for out-of-process objects hosted in this process
    if (this->started)
    {
        RoRevokeActivationFactories(this->serverRegistrationCookie);
    }

    // Set the event that indicates that no instance of the VoIP background process exists
    if (this->noOtherBackgroundProcessEvent != NULL)
    {
        success = ::SetEvent(this->noOtherBackgroundProcessEvent);
        _ASSERT(success);

        ::CloseHandle(this->noOtherBackgroundProcessEvent);
        this->noOtherBackgroundProcessEvent = NULL;
    }

    {
        WCHAR szBuffer[256];
        swprintf_s<ARRAYSIZE(szBuffer)>(szBuffer, L"[Globals::~Globals] => VoIP background process with id %d shutting down\n", this->GetCurrentProcessId());
        ::OutputDebugString(szBuffer);
    }
}
