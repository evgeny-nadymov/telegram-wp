/*
Copyright (c) 2012 Microsoft Corporation.  All rights reserved.
Use of this sample source code is subject to the terms of the Microsoft license
agreement under which you licensed this sample source code and is provided AS-IS.
If you did not accept the terms of the license agreement, you are not authorized
to use this sample source code.  For the terms of the license, please see the
license agreement between you and Microsoft.

To see all Code Samples for Windows Phone, visit http://go.microsoft.com/fwlink/?LinkID=219604

*/
#pragma once
#include "BackEndCapture.h"
#include <windows.phone.networking.voip.h>
#include "ICallControllerStatusListener.h"
#include "IConfig.h"
#include "ApiLock.h"

namespace PhoneVoIPApp
{
	namespace BackEnd
	{
		// Forward declaration
		ref class Globals;

		// A method that is called back when the incoming call dialog has been dismissed.
		// This callback is used to complete the incoming call agent.
		public delegate void IncomingCallDialogDismissedCallback(int64 callId, int64 callAccessHash, bool rejected);

		ref class LibTgVoipStateListener sealed {
		private:
			ICallControllerStatusListener^ statusListener;

		public:
			LibTgVoipStateListener();
			void SetStatusCallback(ICallControllerStatusListener^ statusListener);
			virtual void OnCallStateChanged(libtgvoip::VoIPControllerWrapper^ sender, libtgvoip::CallState newState);
			virtual void OnSignalBarsChanged(libtgvoip::VoIPControllerWrapper^ sender, int signal);
		};

		public value struct Config{
			double InitTimeout;
			double RecvTimeout;
			DataSavingMode DataSavingMode;
			bool EnableAEC;
			bool EnableNS;
			bool EnableAGC;
			Platform::String^ LogFilePath;
			Platform::String^ StatsDumpFilePath;
		};

		// A class that provides methods and properties related to VoIP calls.
		// It wraps Windows.Phone.Networking.Voip.VoipCallCoordinator, and provides app-specific call functionality.
		public ref class CallController sealed
		{
		public:

			// The public methods below are just for illustration purposes - add your own methods here
			// mtproto
			void HandleUpdatePhoneCall();
			void StartMTProtoUpdater();
			void StopMTProtoUpdater();

			// libtgvoip
			void CreateVoIPControllerWrapper();
			void DeleteVoIPControllerWrapper();
			void SetConfig(Config config);
			void SetEncryptionKey(const Platform::Array<uint8>^ key, bool isOutgoing);
			void SetPublicEndpoints(const Platform::Array<EndpointStruct>^ endpoints, bool allowP2P, int connectionMaxLayer);
			void SetProxy(ProxyStruct proxy);
			void Start();
			void Connect();

			void SetMicMute(bool mute);
			void SwitchSpeaker(bool external);
			void UpdateServerConfig(Platform::String^ json);
			int64 GetPreferredRelayID();
			Error GetLastError();
			Platform::String^ GetDebugLog();
			Platform::String^ GetDebugString();
			Platform::String^ GetVersion();
			int GetSignalBarsCount();

			// Provide an inteface that can be used to get call controller status change notifications
			void SetStatusCallback(ICallControllerStatusListener^ statusListener);

			// Initiate an outgoing call. Called by the UI process.
			// Returns true if the outgoing call processing was started, false otherwise.
			bool InitiateOutgoingCall(Platform::String^ recepientName, int64 recepientId, int64 callId, int64 callAccessHash);
			bool InitiateOutgoingCall(Platform::String^ recepientName, int64 recepientId, int64 callId, int64 callAccessHash, 
				Config config, const Platform::Array<uint8>^ key, bool outgoing, 
				const Platform::Array<Platform::String^>^ emojis, const Platform::Array<EndpointStruct>^ endpoints, 
				bool allowP2P, int connectionMaxLayer,
				ProxyStruct proxy);

			// Start processing an incoming call. Called by managed code in this process (the VoIP agent host process).
			// Returns true if the incoming call processing was started, false otherwise.
			bool OnIncomingCallReceived(Platform::String^ contactName, int64 contactId, Platform::String^ contactImage, int64 callId, int64 callAccessHash, IncomingCallDialogDismissedCallback^ incomingCallDialogDismissedCallback);

			// Hold a call. Called by the UI process.
			bool HoldCall();

			// Resume a call. Called by the UI process.
			bool ResumeCall();

			// End a call. Called by the UI process.
			bool EndCall();

			// Toggle the camera location. Called by the UI process.
			bool ToggleCamera();

			// Get the call state
			property PhoneVoIPApp::BackEnd::CallStatus CallStatus
			{
				PhoneVoIPApp::BackEnd::CallStatus get();
			};

			// Get or set the media operations that are allowed for a call.
			property PhoneVoIPApp::BackEnd::MediaOperations MediaOperations
			{
				PhoneVoIPApp::BackEnd::MediaOperations get();
			}

			// Indicates if video is currently being displayed or not
			property bool IsShowingVideo
			{
				bool get();
				void set(bool value);
			}

			// Indicates whether the video is being rendered or not.
			property bool IsRenderingVideo
			{
				bool get();
				void set(bool value);
			}

			// Get the current camera location
			property PhoneVoIPApp::BackEnd::CameraLocation CameraLocation
			{
				PhoneVoIPApp::BackEnd::CameraLocation get();
			}

			// Get the possible routes for audio
			property CallAudioRoute AvailableAudioRoutes
			{
				CallAudioRoute get();
			}

			// Get or set the current route for audio
			property CallAudioRoute AudioRoute
			{
				CallAudioRoute get();
				void set(CallAudioRoute newRoute);
			}

			// Get the name of the other party in the most recent call.
			// Can return nullptr if there hasn't been a call yet.
			property Platform::String^ OtherPartyName
			{
				Platform::String^ get();
			}

			// Get the name of the other party in the most recent call.
			// Can return nullptr if there hasn't been a call yet.
			property int64 OtherPartyId
			{
				int64 get();
			}

			// Get call start time
			property Windows::Foundation::DateTime CallStartTime
			{
				Windows::Foundation::DateTime get();
			}

			property int64 CallId
			{
				int64 get();
			}

			property int64 CallAccessHash
			{
				int64 get();
			}

			property int64 AcceptedCallId
			{
				int64 get();
				void set(int64 value);
			}

			property Platform::Array<uint8>^ Key
			{
				Platform::Array<uint8>^ get();
			}

			property bool Outgoing
			{
				bool get();
			}

			property Platform::Array<Platform::String^>^ Emojis
			{
				Platform::Array<Platform::String^>^ get();
			}

		private:
			libtgvoip::VoIPControllerWrapper^ wrapper;
			LibTgVoipStateListener^ stateListener;

			// Only the server can create an instance of this object
			friend ref class PhoneVoIPApp::BackEnd::Globals;

			// Constructor and destructor
			CallController();
			~CallController();

			// Set the call status
			void SetCallStatus(PhoneVoIPApp::BackEnd::CallStatus newStatus);

			// Indicates that a call is now active 
			void SetActiveCall(Windows::Phone::Networking::Voip::VoipPhoneCall^ call, int64 contactId, int64 callId, int64 callAccessHash, const Platform::Array<uint8_t>^ key, bool outgoing, const Platform::Array<Platform::String^>^ emojis, Windows::Phone::Networking::Voip::VoipCallMedia callMedia);

			// Called by the VoipCallCoordinator when the user accepts an incoming call.
			void OnAcceptCallRequested(Windows::Phone::Networking::Voip::VoipPhoneCall^ sender, Windows::Phone::Networking::Voip::CallAnswerEventArgs^ args);

			// Called by the VoipCallCoordinator when the user rejects an incoming call.
			void OnRejectCallRequested(Windows::Phone::Networking::Voip::VoipPhoneCall^ sender, Windows::Phone::Networking::Voip::CallRejectEventArgs^ args);

			// Called by the VoipCallCoordinator when a call is to be put on hold.
			void OnHoldCallRequested(Windows::Phone::Networking::Voip::VoipPhoneCall^ sender, Windows::Phone::Networking::Voip::CallStateChangeEventArgs^ args);

			// Called by the VoipCallCoordinator when a call that was previously put on hold is to be resumed.
			void OnResumeCallRequested(Windows::Phone::Networking::Voip::VoipPhoneCall^ sender, Windows::Phone::Networking::Voip::CallStateChangeEventArgs^ args);

			// Called by the VoipCallCoordinator when a call is to be ended.
			void OnEndCallRequested(Windows::Phone::Networking::Voip::VoipPhoneCall^ sender, Windows::Phone::Networking::Voip::CallStateChangeEventArgs^ args);

			// Called by the AudioRoutingManager when call audio routing changes.
			void OnAudioEndpointChanged(Windows::Phone::Media::Devices::AudioRoutingManager^ sender, Platform::Object^ args);

			// Called by the BackEndCapture when the camera is toggled
			void OnCameraLocationChanged(PhoneVoIPApp::BackEnd::CameraLocation newCameraLocation);

			// Set a value that indicates if video/audio capture/render is enabled for a call or not.
			void SetMediaOperations(PhoneVoIPApp::BackEnd::MediaOperations value);

			// Start/stop video/audio capture/playback based on the current state.
			void UpdateMediaOperations();

			// The relative URI to the call-in-progress page
			Platform::String ^callInProgressPageUri;

			// The name of this service provider
			Platform::String^ voipServiceName;

			// The URI to the default contact image
			Windows::Foundation::Uri^ defaultContactImageUri;

			// The URI to the branding image
			Windows::Foundation::Uri^ brandingImageUri;

			// The URI to the ringtone file
			Windows::Foundation::Uri^ ringtoneUri;

			// Interface used to deliver status callbacks
			ICallControllerStatusListener^ statusListener;

			// A VoIP call that is in progress
			Windows::Phone::Networking::Voip::VoipPhoneCall^ activeCall;

			// The status of a call, if any
			PhoneVoIPApp::BackEnd::CallStatus callStatus;

			// The name of the other party, if any
			Platform::String^ otherPartyName;

			// The id of the other party, if any
			int64 otherPartyId;

			int64 callId;

			int64 callAccessHash;

			// The id of the caller for the latest incoming call
			int64 incomingId;

			int64 incomingCallId;

			int64 incomingCallAccessHash;

			int64 acceptedCallId;

			Platform::Array<uint8>^ key;

			bool outgoing;

			Platform::Array<Platform::String^>^ emojis;

			// Indicates if video/audio capture/render is enabled for a call or not.
			PhoneVoIPApp::BackEnd::MediaOperations mediaOperations;

			PhoneVoIPApp::BackEnd::CameraLocation cameraLocation;

			// Indicates if video is currently being displayed or not.
			bool isShowingVideo;

			bool isRenderingVideo;

			// The method to be called when the incoming call dialog box is dismissed
			IncomingCallDialogDismissedCallback^ onIncomingCallDialogDismissed;

			// The VoIP call coordinator
			Windows::Phone::Networking::Voip::VoipCallCoordinator^ callCoordinator;

			// The phone audio routing manager
			Windows::Phone::Media::Devices::AudioRoutingManager^ audioRoutingManager;

			// Phone call related event handlers
			Windows::Foundation::TypedEventHandler<Windows::Phone::Networking::Voip::VoipPhoneCall^, Windows::Phone::Networking::Voip::CallAnswerEventArgs^>^ acceptCallRequestedHandler;
			Windows::Foundation::TypedEventHandler<Windows::Phone::Networking::Voip::VoipPhoneCall^, Windows::Phone::Networking::Voip::CallRejectEventArgs^>^ rejectCallRequestedHandler;
			Windows::Foundation::TypedEventHandler<Windows::Phone::Networking::Voip::VoipPhoneCall^, Windows::Phone::Networking::Voip::CallStateChangeEventArgs^>^ holdCallRequestedHandler;
			Windows::Foundation::TypedEventHandler<Windows::Phone::Networking::Voip::VoipPhoneCall^, Windows::Phone::Networking::Voip::CallStateChangeEventArgs^>^ resumeCallRequestedHandler;
			Windows::Foundation::TypedEventHandler<Windows::Phone::Networking::Voip::VoipPhoneCall^, Windows::Phone::Networking::Voip::CallStateChangeEventArgs^>^ endCallRequestedHandler;

			// Audio related event handlers
			Windows::Foundation::TypedEventHandler<Windows::Phone::Media::Devices::AudioRoutingManager^, Platform::Object^>^ audioEndpointChangedHandler;

			// A cookie used to un-register the audio endpoint changed handler
			Windows::Foundation::EventRegistrationToken audioEndpointChangedHandlercookie;

			// Camera location related event handlers
			CameraLocationChangedEventHandler^ cameraLocationChangedHandler;
		};
	}
}
