// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "CallController.h"
#include "BackEndAudio.h"
#include "BackEndCapture.h"
#include "Server.h"

using namespace PhoneVoIPApp::BackEnd;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Phone::Media::Devices;
using namespace Windows::Phone::Networking::Voip;

LibTgVoipStateListener::LibTgVoipStateListener(){

}

void LibTgVoipStateListener::OnCallStateChanged(libtgvoip::VoIPControllerWrapper^ sender, libtgvoip::CallState newState)
{
	if (this->statusListener != nullptr){
		this->statusListener->OnCallStateChanged((CallState)newState);
	}
}

void LibTgVoipStateListener::OnSignalBarsChanged(libtgvoip::VoIPControllerWrapper^ sender, int newSignal)
{
	if (this->statusListener != nullptr){
		this->statusListener->OnSignalBarsChanged(newSignal);
	}
}

void LibTgVoipStateListener::SetStatusCallback(ICallControllerStatusListener^ statusListener)
{
	this->statusListener = statusListener;
}

void CallController::StartMTProtoUpdater()
{
	if (Globals::Instance->MTProtoUpdater != nullptr)
	{
		Globals::Instance->MTProtoUpdater->Start(0, 0, 0);
	}
}

void CallController::StopMTProtoUpdater()
{
	if (Globals::Instance->MTProtoUpdater != nullptr)
	{
		Globals::Instance->MTProtoUpdater->Stop();
	}
}

void CallController::HandleUpdatePhoneCall()
{

	::OutputDebugString(L"[CallController::HandleUpdatePhoneCall]\n");
}

void CallController::CreateVoIPControllerWrapper()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	wrapper = ref new libtgvoip::VoIPControllerWrapper();
	
	//wrapper->SetStateCallback(stateListener);
	wrapper->CallStateChanged += ref new libtgvoip::CallStateChangedEventHandler(
		stateListener,
		&LibTgVoipStateListener::OnCallStateChanged
	);
	wrapper->SignalBarsChanged += ref new libtgvoip::SignalBarsChangedEventHandler(
		stateListener,
		&LibTgVoipStateListener::OnSignalBarsChanged
	);
}

void CallController::DeleteVoIPControllerWrapper()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr){
		delete wrapper;
		wrapper = nullptr;
	}
}

void CallController::SetConfig(Config config)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr)
	{
		wrapper->SetConfig(config.InitTimeout, config.RecvTimeout, (libtgvoip::DataSavingMode)config.DataSavingMode, config.EnableAEC, config.EnableNS, config.EnableAGC, config.LogFilePath, config.StatsDumpFilePath);
	}
}

void CallController::SetEncryptionKey(const Platform::Array<uint8>^ key, bool isOutgoing)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr)
	{
		wrapper->SetEncryptionKey(key, isOutgoing);
	}
}

void CallController::SetPublicEndpoints(const Platform::Array<EndpointStruct>^ endpoints, bool allowP2P, int connectionMaxLayer)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr)
	{
		auto points = ref new Platform::Array<libtgvoip::Endpoint^>(endpoints->Length);

		for (int i = 0; i < endpoints->Length; i++)
		{
			auto point = ref new libtgvoip::Endpoint();
			point->id = endpoints[i].id;
			point->port = endpoints[i].port;
			point->ipv4 = endpoints[i].ipv4;
			point->ipv6 = endpoints[i].ipv6;

			auto length = endpoints[i].peerTag->Length();
			auto it = endpoints[i].peerTag->Begin();
			auto peerTag = ref new Platform::Array<byte>(length);
			for (size_t i = 0; i < length; i++)
			{
				peerTag[i] = (byte)it[i];
			}
			point->peerTag = peerTag;

			points[i] = point;
		}

		wrapper->SetPublicEndpoints(points, allowP2P, connectionMaxLayer);
	}
}

void CallController::SetProxy(ProxyStruct proxy)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr)
	{
		wrapper->SetProxy((libtgvoip::ProxyProtocol)proxy.protocol, proxy.address, proxy.port, proxy.username, proxy.password);
	}
}

void CallController::Start()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr)
	{
		wrapper->Start();
	}
}

void Handler(Windows::System::Threading::ThreadPoolTimer^ timer){
	::OutputDebugString(L"PeriodicTimer.Handler");
}

void CallController::Connect()
{
	::OutputDebugString(L"[CallController::Connect]\n");

	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr)
	{
		wrapper->Connect();
	}
}

void CallController::SetMicMute(bool mute)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr){
		wrapper->SetMicMute(mute);
	}
}
void CallController::SwitchSpeaker(bool external)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	libtgvoip::VoIPControllerWrapper::SwitchSpeaker(external);
}

void CallController::SetStatusCallback(ICallControllerStatusListener^ statusListener)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    this->statusListener = statusListener;
	this->stateListener->SetStatusCallback(statusListener);
}

void CallController::UpdateServerConfig(Platform::String^ json)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	libtgvoip::VoIPControllerWrapper::UpdateServerConfig(json);
}

int64 CallController::GetPreferredRelayID()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr){
		return wrapper->GetPreferredRelayID();
	}

	return 0;
}

Error CallController::GetLastError()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr){
		return (Error)wrapper->GetLastError();
	}

	return Error::Unknown;
}

Platform::String^ CallController::GetVersion()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	return libtgvoip::VoIPControllerWrapper::GetVersion();
}

int CallController::GetSignalBarsCount()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr){
		return wrapper->GetSignalBarsCount();
	}

	return 0;
}

Platform::String^ CallController::GetDebugLog()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr){
		return wrapper->GetDebugLog();
	}

	return nullptr;
}

Platform::String^ CallController::GetDebugString()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	if (wrapper != nullptr){
		return wrapper->GetDebugString();
	}

	return nullptr;
}

bool CallController::InitiateOutgoingCall(Platform::String^ recepientName, int64 recepientId, int64 id, int64 accessHash, 
	Config config, const Platform::Array<uint8>^ key, bool outgoing, const Platform::Array<Platform::String^>^ emojis, 
	const Platform::Array<EndpointStruct>^ endpoints, bool allowP2P, int connectionMaxLayer,
	ProxyStruct proxy)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	VoipPhoneCall^ outgoingCall = nullptr;

	// In this sample, we allow only one call at a time.
	if (this->activeCall != nullptr)
	{
		::OutputDebugString(L"[CallController::InitiateOutgoingCall] => Only one active call allowed in this sample at a time\n");

		this->activeCall->NotifyCallEnded();

		// If we receive a request to initiate an outgoing call when another call is in progress,
		// we just ignore it. 
		//return false;
	}

	::OutputDebugString(L"[CallController::InitiateOutgoingCall] => Starting outgoing call\n");

	// Start a new outgoing call.
	this->callCoordinator->RequestNewOutgoingCall(this->callInProgressPageUri, recepientName, "Telegram", VoipCallMedia::Audio
		//| VoipCallMedia::Video
		, &outgoingCall);

	// Tell the phone service that this call is active.
	// Normally, we do this only when the remote party has accepted the call.
	outgoingCall->NotifyCallActive();

	// Store it as the active call - assume we support both audio and video
	this->SetActiveCall(outgoingCall, recepientId, id, accessHash, key, outgoing, emojis, VoipCallMedia::Audio
		//| VoipCallMedia::Video
		);

	this->DeleteVoIPControllerWrapper();
	this->CreateVoIPControllerWrapper();
	this->SetConfig(config);
	this->SetEncryptionKey(key, outgoing);
	this->SetPublicEndpoints(endpoints, allowP2P, connectionMaxLayer);
	this->SetProxy(proxy);
	this->Start();
	//UpdateNetworkType(null);
	this->Connect();


	return true;
}

bool CallController::InitiateOutgoingCall(Platform::String^ recepientName, int64 recepientId, int64 callId, int64 callAccessHash)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    VoipPhoneCall^ outgoingCall = nullptr;

    // In this sample, we allow only one call at a time.
    if (this->activeCall != nullptr)
    {
        ::OutputDebugString(L"[CallController::InitiateOutgoingCall] => Only one active call allowed in this sample at a time\n");

        // If we receive a request to initiate an outgoing call when another call is in progress,
        // we just ignore it. 
        return false;
    }

    ::OutputDebugString(L"[CallController::InitiateOutgoingCall] => Starting outgoing call\n");

    // Start a new outgoing call.
    this->callCoordinator->RequestNewOutgoingCall(this->callInProgressPageUri, recepientName, "Telegram", VoipCallMedia::Audio
		//| VoipCallMedia::Video
		, &outgoingCall);

    // Tell the phone service that this call is active.
    // Normally, we do this only when the remote party has accepted the call.
    outgoingCall->NotifyCallActive();

    // Store it as the active call - assume we support both audio and video
	this->SetActiveCall(outgoingCall, recepientId, callId, callAccessHash, nullptr, true, nullptr, VoipCallMedia::Audio
		//| VoipCallMedia::Video
		);

    return true;
}

bool CallController::OnIncomingCallReceived(Platform::String^ contactName, int64 contactId, Platform::String^ contactImage, int64 callId, int64 callAccessHash, IncomingCallDialogDismissedCallback^ incomingCallDialogDismissedCallback)
{
    VoipPhoneCall^ incomingCall = nullptr;

    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // TODO: If required, contact your cloud service here for more information about the incoming call.

    try
    {
        TimeSpan timeout;
        timeout.Duration = 25 * 10 * 1000 * 1000; // in 100ns units

        ::OutputDebugString(L"[CallController::OnIncomingCallReceived] => Will time out in 90 seconds\n");

        // Store the caller number of this incoming call
        this->incomingId = contactId;
		this->incomingCallId = callId;
		this->incomingCallAccessHash = callAccessHash;

        // Store the callback that needs to be called when the incoming call dialog has been dismissed,
        // either because the call was accepted or rejected by the user.
        this->onIncomingCallDialogDismissed = incomingCallDialogDismissedCallback;
		//Windows::ApplicationModel::Package::Current->InstalledLocation->Path
		Uri^ contactImageUri = nullptr;
		if (contactImage != nullptr)
		{
			String^ localFolder = String::Concat(Windows::Storage::ApplicationData::Current->LocalFolder->Path, "\\");
			contactImageUri = ref new Uri(localFolder, contactImage);
		}

        // Ask the Phone Service to start a new incoming call
        this->callCoordinator->RequestNewIncomingCall(
            this->callInProgressPageUri,
            contactName,
            "Telegram Call",
            contactImageUri,
            this->voipServiceName,
            this->brandingImageUri,
            "",                      // Was this call forwarded/delegated to this user on behalf of someone else? At this time, we won't use this field
            this->ringtoneUri,
            VoipCallMedia::Audio,	// | VoipCallMedia::Video,
            timeout,                // Maximum amount of time to ring for
            &incomingCall);
    }
    catch(...)
    {
        // Requesting an incoming call can fail if there is already an incoming call in progress.
        // This is rare, but possible. Treat this case like a missed call.
        ::OutputDebugString(L"[CallController::OnIncomingCallReceived] => An exception has occurred\n");
        return false;
    }

    // Register for events about this incoming call.
    incomingCall->AnswerRequested += this->acceptCallRequestedHandler;
    incomingCall->RejectRequested += this->rejectCallRequestedHandler;

    return true;
}

bool CallController::HoldCall()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->activeCall == nullptr)
    {
        // Nothing to do - there is no call to put on hold
        return false;
    }

    ::OutputDebugString(L"[CallController::HoldCall] => Trying to put call on hold\n");

    // Change the call status before notifying that the call is held because
    // access to the camera will be removed once NotifyCallHeld is called
    this->SetCallStatus(PhoneVoIPApp::BackEnd::CallStatus::Held);

    // Hold the active call
    this->activeCall->NotifyCallHeld();

    // TODO: Contact your cloud service and let it know that the active call has been put on hold.

    return true;
}

bool CallController::ResumeCall()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->activeCall == nullptr)
    {
        // Nothing to do - there is no call to resume
        return false;
    }

    ::OutputDebugString(L"[CallController::ResumeCall] => Trying to resume a call\n");

    // Resume the active call
    this->activeCall->NotifyCallActive();

    // TODO: Contact your cloud service and let it know that the active call has been resumed.

    // Change the call status after notifying that the call is active
    // if it is done before access to the camera will not have been granted yet
    this->SetCallStatus(PhoneVoIPApp::BackEnd::CallStatus::InProgress);

    return true;
}

bool CallController::EndCall()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->activeCall == nullptr)
    {
        // Nothing to do - there is no call to end
        return false;
    }

    ::OutputDebugString(L"[CallController::EndCall] => Trying to end a call\n");

    // Unregister from audio endpoint changes
    this->audioRoutingManager->AudioEndpointChanged -= this->audioEndpointChangedHandlercookie;

    // TODO: Contact your cloud service and let it know that the active call has ended.

    // End the active call.
    this->activeCall->NotifyCallEnded();
    this->activeCall = nullptr;

	// Reset libtgvoip call params
	this->key = nullptr;
	this->callId = -1;
	this->callAccessHash = 0;
	this->otherPartyId = 0;
	this->emojis = nullptr;
    
    // Reset camera choice to front facing for next call
    this->cameraLocation = PhoneVoIPApp::BackEnd::CameraLocation::Front;

	// Change the call status
	this->SetCallStatus(PhoneVoIPApp::BackEnd::CallStatus::None);

    return true;
}

bool CallController::ToggleCamera()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->activeCall == nullptr)
    {
        // Nothing to do - there is no call to end
        return false;
    }
    ::OutputDebugString(L"[CallController::ToggleCamera] => Trying to toggle the camera\n");

    Globals::Instance->CaptureController->ToggleCamera();

    return true;
}

PhoneVoIPApp::BackEnd::CallStatus CallController::CallStatus::get()
{
    // No need to lock - this get is idempotent
    return this->callStatus;
}

PhoneVoIPApp::BackEnd::CameraLocation CallController::CameraLocation::get()
{
    // No need to lock - this get is idempotent
    return this->cameraLocation;
}

PhoneVoIPApp::BackEnd::MediaOperations CallController::MediaOperations::get()
{
    // No need to lock - this get is idempotent
    return this->mediaOperations;
}

bool CallController::IsShowingVideo::get()
{
    // No need to lock - this get is idempotent
    return this->isShowingVideo;
}

void CallController::IsShowingVideo::set(bool value)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // Has anything changed?
    if (this->isShowingVideo == value)
        return; // No

    // Update the value
    this->isShowingVideo = value;

    // Start/stop video capture/render based on this change
    this->UpdateMediaOperations();
}

bool CallController::IsRenderingVideo::get()
{
    // No need to lock - this get is idempotent
    return this->isRenderingVideo;
}

void CallController::IsRenderingVideo::set(bool value)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // Has anything changed?
    if (this->isRenderingVideo == value)
        return; // No

    // Update the value
    this->isRenderingVideo = value;

    // Start/stop video capture/render based on this change
    this->UpdateMediaOperations();
}

CallAudioRoute CallController::AvailableAudioRoutes::get()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->activeCall == nullptr)
    {
        // There is no call in progress
        return CallAudioRoute::None;
    }

    return (CallAudioRoute)(this->audioRoutingManager->AvailableAudioEndpoints);
}

CallAudioRoute CallController::AudioRoute::get()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->activeCall == nullptr)
    {
        // There is no call in progress
        return CallAudioRoute::None;
    }

    auto audioEndpoint = this->audioRoutingManager->GetAudioEndpoint();
    switch(audioEndpoint)
    {
    case AudioRoutingEndpoint::Earpiece:
    case AudioRoutingEndpoint::WiredHeadset:
    case AudioRoutingEndpoint::WiredHeadsetSpeakerOnly:
        return CallAudioRoute::Earpiece;

    case AudioRoutingEndpoint::Default:
    case AudioRoutingEndpoint::Speakerphone:
        return CallAudioRoute::Speakerphone;

    case AudioRoutingEndpoint::Bluetooth:
    case AudioRoutingEndpoint::BluetoothWithNoiseAndEchoCancellation:
        return CallAudioRoute::Bluetooth;

    default:
        throw ref new FailureException("Unexpected audio routing endpoint");
    }

}

void CallController::AudioRoute::set(CallAudioRoute newRoute)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->callStatus != PhoneVoIPApp::BackEnd::CallStatus::InProgress)
    {
        // There is no call in progress - do nothing
        return;
    }

    switch(newRoute)
    {
    case CallAudioRoute::Earpiece:
        this->audioRoutingManager->SetAudioEndpoint(AudioRoutingEndpoint::Earpiece);
        break;

    case CallAudioRoute::Speakerphone:
        this->audioRoutingManager->SetAudioEndpoint(AudioRoutingEndpoint::Speakerphone);
        break;

    case CallAudioRoute::Bluetooth:
        this->audioRoutingManager->SetAudioEndpoint(AudioRoutingEndpoint::Bluetooth);
        break;

    case CallAudioRoute::None:
    default:
        throw ref new FailureException("Cannot set audio route to CallAudioRoute::None");
    }
}

Platform::String^ CallController::OtherPartyName::get()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);
	return this->otherPartyName;
}

int64 CallController::OtherPartyId::get()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);
    return this->otherPartyId;
}

Windows::Foundation::DateTime CallController::CallStartTime::get()
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if (this->activeCall != nullptr)
    {
        // There is a call in progress
        return this->activeCall->StartTime;
    }
    else
    {
        // There is no call in progress
        Windows::Foundation::DateTime minValue;
        minValue.UniversalTime = 0;
        return minValue;
    }
}

int64 CallController::CallId::get()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);
	return this->callId;
}

int64 CallController::CallAccessHash::get()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);
	return this->callAccessHash;
}

int64 CallController::AcceptedCallId::get()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	return this->acceptedCallId;
}

void CallController::AcceptedCallId::set(int64 value)
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	this->acceptedCallId = value;
}

Platform::Array<uint8>^ CallController::Key::get()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	return this->key;
}

bool CallController::Outgoing::get()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	return this->outgoing;
}

Platform::Array<Platform::String^>^ CallController::Emojis::get()
{
	// Make sure only one API call is in progress at a time
	std::lock_guard<std::recursive_mutex> lock(g_apiLock);

	return this->emojis;
}

CallController::CallController() :
	wrapper(nullptr),
    callInProgressPageUri(L"/Views/ShellView.xaml"),
    voipServiceName(nullptr),
    defaultContactImageUri(nullptr),
    brandingImageUri(nullptr),
    ringtoneUri(nullptr),
    statusListener(nullptr),
    callStatus(PhoneVoIPApp::BackEnd::CallStatus::None),
    otherPartyName(nullptr),
    otherPartyId(-1),
    incomingId(-1),
	acceptedCallId(-1),
	key(nullptr),
	emojis(nullptr),
    mediaOperations(PhoneVoIPApp::BackEnd::MediaOperations::None),
    onIncomingCallDialogDismissed(nullptr),
    activeCall(nullptr),
    cameraLocation(PhoneVoIPApp::BackEnd::CameraLocation::Front)
{
	this->stateListener = ref new LibTgVoipStateListener();
    this->callCoordinator = VoipCallCoordinator::GetDefault();
    this->audioRoutingManager = AudioRoutingManager::GetDefault();

    // URIs required for interactions with the VoipCallCoordinator
    String^ installFolder = String::Concat(Windows::ApplicationModel::Package::Current->InstalledLocation->Path, "\\");
    //this->defaultContactImageUri = ref new Uri(installFolder, "Assets\\DefaultContactImage.png");
    this->voipServiceName = ref new String(L"Telegram");
    this->brandingImageUri = ref new Uri(installFolder, "SquareTile150x150.png");
    //this->ringtoneUri = ref new Uri(installFolder, "Assets\\Ringtone.wma");

    // Event handler delegates - creating them once and storing them as member variables
    // avoids having to create new delegate objects for each phone call.
    this->acceptCallRequestedHandler = ref new TypedEventHandler<VoipPhoneCall^, CallAnswerEventArgs^>(this, &CallController::OnAcceptCallRequested);
    this->rejectCallRequestedHandler = ref new TypedEventHandler<VoipPhoneCall^, CallRejectEventArgs^>(this, &CallController::OnRejectCallRequested);
    this->holdCallRequestedHandler = ref new TypedEventHandler<VoipPhoneCall^, CallStateChangeEventArgs^>(this, &CallController::OnHoldCallRequested);
    this->resumeCallRequestedHandler = ref new TypedEventHandler<VoipPhoneCall^, CallStateChangeEventArgs^>(this, &CallController::OnResumeCallRequested);
    this->endCallRequestedHandler = ref new TypedEventHandler<VoipPhoneCall^, CallStateChangeEventArgs^>(this, &CallController::OnEndCallRequested);
    this->audioEndpointChangedHandler = ref new TypedEventHandler<AudioRoutingManager^, Object^>(this, &CallController::OnAudioEndpointChanged);
    this->cameraLocationChangedHandler = ref new CameraLocationChangedEventHandler(this, &CallController::OnCameraLocationChanged);
}

CallController::~CallController()
{
}

void CallController::SetCallStatus(PhoneVoIPApp::BackEnd::CallStatus newStatus)
{
    // No need to lock - private method

    if (newStatus == this->callStatus)
        return; // Nothing more to do

    // Change the call status
    this->callStatus = newStatus;

    // Update audio/video capture/render status, if required.
    this->UpdateMediaOperations();

    // If required, let the UI know.
    if (this->statusListener != nullptr)
    {
        this->statusListener->OnCallStatusChanged(this->callStatus);
    }
}

void CallController::SetActiveCall(VoipPhoneCall^ call, int64 contactId, int64 callId, int64 callAccessHash, const Platform::Array<uint8_t>^ key, bool outgoing, const Platform::Array<Platform::String^>^ emojis, VoipCallMedia callMedia)
{
    // No need to lock - private method

    // The specified call is now active.
    // For an incoming call, this means that the local party has accepted the call.
    // For an outoing call, this means that the remote party has accepted the call.
    this->activeCall = call;

    // Listen to state changes of the active call.
    call->HoldRequested += this->holdCallRequestedHandler;
    call->ResumeRequested += this->resumeCallRequestedHandler;
    call->EndRequested += this->endCallRequestedHandler;

    // Register for audio endpoint changes
    this->audioEndpointChangedHandlercookie = this->audioRoutingManager->AudioEndpointChanged += this->audioEndpointChangedHandler;

    // Store information about the other party in the call
    this->otherPartyName = this->activeCall->ContactName;
	this->otherPartyId = contactId;
	this->callId = callId;
	this->callAccessHash = callAccessHash;

	if (key != nullptr)
	{
		this->key = ref new Platform::Array<uint8_t>(key->Data, key->Length);
	}
	else
	{
		this->key = nullptr;
	}
	if (emojis != nullptr)
	{
		this->emojis = ref new Platform::Array<Platform::String^>(emojis->Data, emojis->Length);
	}
	else
	{
		this->emojis = nullptr;
	}

	this->outgoing = outgoing;

    // Change the call status
    this->SetCallStatus(PhoneVoIPApp::BackEnd::CallStatus::InProgress);

    // Figure out if video/audio capture/render are all allowed
    PhoneVoIPApp::BackEnd::MediaOperations newOperations = PhoneVoIPApp::BackEnd::MediaOperations::None;
    if ((callMedia & VoipCallMedia::Audio) != VoipCallMedia::None)
    {
        // Enable both audio capture and render by default
        newOperations = PhoneVoIPApp::BackEnd::MediaOperations::AudioCapture | PhoneVoIPApp::BackEnd::MediaOperations::AudioRender;
    }
    if ((callMedia & VoipCallMedia::Video) != VoipCallMedia::None)
    {
        // Enable both video capture and render by default
        newOperations = newOperations | PhoneVoIPApp::BackEnd::MediaOperations::VideoCapture | PhoneVoIPApp::BackEnd::MediaOperations::VideoRender;

    }
    this->SetMediaOperations(newOperations);
}

void CallController::OnAcceptCallRequested(VoipPhoneCall^ sender, CallAnswerEventArgs^ args)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    ::OutputDebugString(L"[CallController::OnAcceptCallRequested] => Incoming call accepted\n");

    // The local user has accepted an incoming call.
    VoipPhoneCall^ incomingCall = (VoipPhoneCall^)sender;

    // If there is was a call already in progress, end it
    // As of now, we support only one call at a time in this application
    this->EndCall();

    // Reset camera choice to front facing for next call
    this->cameraLocation = PhoneVoIPApp::BackEnd::CameraLocation::Front;

    // The incoming call is the new active call.
    incomingCall->NotifyCallActive();

	this->acceptedCallId = this->incomingCallId;

    // Let the incoming call agent know that incoming call processing is now complete
    if (this->onIncomingCallDialogDismissed != nullptr)
        this->onIncomingCallDialogDismissed(this->incomingCallId, this->incomingCallAccessHash, false);

    // Store it as the active call.
    this->SetActiveCall(incomingCall, this->incomingId, this->incomingCallId, this->incomingCallAccessHash, nullptr, false, nullptr, args->AcceptedMedia);
}

void CallController::OnRejectCallRequested(VoipPhoneCall^ sender, CallRejectEventArgs^ args)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    ::OutputDebugString(L"[CallController::OnRejectCallRequested] => Incoming call rejected\n");

    // The local user has rejected an incoming call.
    VoipPhoneCall^ incomingCall = (VoipPhoneCall^)sender;

    // End it.
    incomingCall->NotifyCallEnded();

    // TODO: Contact your cloud service and let it know that the incoming call was rejected.
	
    // Finally, let the incoming call agent know that incoming call processing is now complete
	this->onIncomingCallDialogDismissed(this->incomingCallId, this->incomingCallAccessHash, true);
}

void CallController::OnHoldCallRequested(VoipPhoneCall^ sender, CallStateChangeEventArgs^ args)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // A request to put the active call on hold has been received.
    VoipPhoneCall^ callToPutOnHold = (VoipPhoneCall^)sender;

    // Sanity test.
    if (callToPutOnHold != this->activeCall)
    {
        throw ref new Platform::FailureException(L"Something is wrong. The call to put on hold is not the active call");
    }

    // Put the active call on hold.
    this->HoldCall();
}

void CallController::OnResumeCallRequested(VoipPhoneCall^ sender, CallStateChangeEventArgs^ args)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // A request to resumed the active call has been received.
    VoipPhoneCall^ callToResume = (VoipPhoneCall^)sender;

    // Sanity test.
    if (callToResume != this->activeCall)
    {
        throw ref new Platform::FailureException(L"Something is wrong. The call to resume is not the active call");
    }

    // Resume the active call
    this->ResumeCall();
}

void CallController::OnEndCallRequested(VoipPhoneCall^ sender, CallStateChangeEventArgs^ args)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // A request to end the active call has been received.
    VoipPhoneCall^ callToEnd = (VoipPhoneCall^)sender;

    // Sanity test.
    if (callToEnd != this->activeCall)
    {
        throw ref new Platform::FailureException(L"Something is wrong. The call to end is not the active call");
    }

    // End the active call
    this->EndCall();
}

void CallController::OnAudioEndpointChanged(AudioRoutingManager^ sender, Object^ args)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    // If required, let the UI know.
    if ((this->activeCall != nullptr) && (this->statusListener != nullptr))
    {
        this->statusListener->OnCallAudioRouteChanged(this->AudioRoute);
    }
}

void CallController::OnCameraLocationChanged(PhoneVoIPApp::BackEnd::CameraLocation newCameraLocation)
{
    // Make sure only one API call is in progress at a time
    std::lock_guard<std::recursive_mutex> lock(g_apiLock);

    if(this->cameraLocation == newCameraLocation || this->activeCall == nullptr)
    {
        // nothing to do
        return;
    }

    this->cameraLocation = newCameraLocation;

    // If required, let the UI know.
    if ((this->statusListener != nullptr))
    {
        this->statusListener->OnCameraLocationChanged(this->cameraLocation);
    }

}

void CallController::SetMediaOperations(PhoneVoIPApp::BackEnd::MediaOperations value)
{
    // No need to lock - private method

    // Has anything changed?
    if (this->mediaOperations == value)
        return; // No

    // Update the value
    this->mediaOperations = value;

    // Start/stop video/audio capture based on this change
    this->UpdateMediaOperations();
}

void CallController::UpdateMediaOperations()
{
    // No need to lock - private method

    bool captureAudio = false, captureVideo = false, renderAudio = false, renderVideo = false;

    if (this->callStatus == PhoneVoIPApp::BackEnd::CallStatus::InProgress)
    {
        // A call is in progress

        // Start audio capture/render, if enabled
        captureAudio = ((this->mediaOperations & PhoneVoIPApp::BackEnd::MediaOperations::AudioCapture) != PhoneVoIPApp::BackEnd::MediaOperations::None);
        renderAudio = ((this->mediaOperations & PhoneVoIPApp::BackEnd::MediaOperations::AudioRender) != PhoneVoIPApp::BackEnd::MediaOperations::None);

        // Start video capture/render if enabled *and* the UI is showing video.
        if (this->isShowingVideo)
        {
            if(isRenderingVideo)
            {
                renderVideo = ((this->mediaOperations & PhoneVoIPApp::BackEnd::MediaOperations::VideoRender) != PhoneVoIPApp::BackEnd::MediaOperations::None);
            }

            // Does this phone have a camera?
            auto availableCameras = Windows::Phone::Media::Capture::AudioVideoCaptureDevice::AvailableSensorLocations;
            bool isCameraPresent = ((availableCameras != nullptr) && (availableCameras->Size > 0));

            // Start capture only if there is a camera present
            if (isCameraPresent)
            {
                captureVideo = ((this->mediaOperations & PhoneVoIPApp::BackEnd::MediaOperations::VideoCapture) != PhoneVoIPApp::BackEnd::MediaOperations::None);
            }
        }
    }
    // else: call is not in progress - all capture/rendering should stop

    // What are the new media operations?
    PhoneVoIPApp::BackEnd::MediaOperations newOperations = PhoneVoIPApp::BackEnd::MediaOperations::None;

    // Start/stop audio capture and render
    if (captureAudio || renderAudio)
    {
        ::OutputDebugString(L"[CallController::UpdateMediaOperations] => Starting audio\n");
        newOperations = newOperations | (PhoneVoIPApp::BackEnd::MediaOperations::AudioCapture | PhoneVoIPApp::BackEnd::MediaOperations::AudioRender);
        //Globals::Instance->AudioController->Start();
    }
    else
    {
        ::OutputDebugString(L"[CallController::UpdateMediaOperations] => Stopping audio\n");
        //Globals::Instance->AudioController->Stop();
    }

    // Start/stop video render
    if (Globals::Instance->VideoRenderer != nullptr)
    {
        if (renderVideo)
        {
            ::OutputDebugString(L"[CallController::UpdateMediaOperations] => Starting video render\n");
            newOperations = newOperations | PhoneVoIPApp::BackEnd::MediaOperations::VideoRender;
            Globals::Instance->VideoRenderer->Start();
        }
        else
        {
            ::OutputDebugString(L"[CallController::UpdateMediaOperations] => Stopping video render\n");
            Globals::Instance->VideoRenderer->Stop();
        }

        if (captureVideo)
        {
            ::OutputDebugString(L"[CallController::UpdateMediaOperations] => Starting video capture\n");
            newOperations = newOperations | PhoneVoIPApp::BackEnd::MediaOperations::VideoCapture;
            Globals::Instance->CaptureController->Start(cameraLocation);
            Globals::Instance->CaptureController->CameraLocationChanged += cameraLocationChangedHandler;
        }
        else
        {
            ::OutputDebugString(L"[CallController::UpdateMediaOperations] => Stopping video capture\n");
            Globals::Instance->CaptureController->Stop();
        }
    }

    // Let the listener know that the allowed media operation state has changed
    if (this->statusListener != nullptr)
    {
        this->statusListener->OnMediaOperationsChanged(newOperations);
    }
}
