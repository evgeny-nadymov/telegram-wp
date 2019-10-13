// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once

#include <windows.phone.networking.voip.h>

namespace PhoneVoIPApp
{
    namespace BackEnd
	{
		// libtgvoip Endpoint
		public ref class Endpoint sealed{
		public:
			property int64 id;
			property uint16 port;
			property Platform::String^ ipv4;
			property Platform::String^ ipv6;
			property Platform::Array<uint8>^ peerTag;
		};

		public value struct EndpointStruct{
			int64 id;
			uint16 port;
			Platform::String^ ipv4;
			Platform::String^ ipv6;
			Platform::String^ peerTag;
		};

		// libtgvoip ProxyProtocol
		public enum class ProxyProtocol : int{
			None = (int)libtgvoip::ProxyProtocol::None,
			SOCKS5 = (int)libtgvoip::ProxyProtocol::SOCKS5
		};

		public value struct ProxyStruct{
			ProxyProtocol protocol;
			Platform::String^ address;
			uint16 port;
			Platform::String^ username;
			Platform::String^ password;
		};

		// libtgvoip CallState
		public enum class CallState : int{
			WaitInit		= (int)libtgvoip::CallState::WaitInit,
			WaitInitAck		= (int)libtgvoip::CallState::WaitInitAck,
			Established		= (int)libtgvoip::CallState::Established,
			Failed			= (int)libtgvoip::CallState::Failed
		};

		// libtgvoip Error
		public enum class Error : int{
			Unknown			= (int)libtgvoip::Error::Unknown,
			Incompatible	= (int)libtgvoip::Error::Incompatible,
			Timeout			= (int)libtgvoip::Error::Timeout,
			AudioIO			= (int)libtgvoip::Error::AudioIO
		};

		// libtgvoip NetworkType
		public enum class NetworkType : int{
			Unknown			= (int)libtgvoip::NetworkType::Unknown,
			GPRS			= (int)libtgvoip::NetworkType::GPRS,
			EDGE			= (int)libtgvoip::NetworkType::EDGE,
			UMTS			= (int)libtgvoip::NetworkType::UMTS,
			HSPA			= (int)libtgvoip::NetworkType::HSPA,
			LTE				= (int)libtgvoip::NetworkType::LTE,
			WiFi			= (int)libtgvoip::NetworkType::WiFi,
			Ethernet		= (int)libtgvoip::NetworkType::Ethernet,
			OtherHighSpeed	= (int)libtgvoip::NetworkType::OtherHighSpeed,
			OtherLowSpeed	= (int)libtgvoip::NetworkType::OtherLowSpeed,
			Dialup			= (int)libtgvoip::NetworkType::Dialup,
			OtherMobile		= (int)libtgvoip::NetworkType::OtherMobile,
		};

		// libtgvoip DataSavingMode
		public enum class DataSavingMode{
			Never			= (int)libtgvoip::DataSavingMode::Never,
			MobileOnly		= (int)libtgvoip::DataSavingMode::MobileOnly,
			Always			= (int)libtgvoip::DataSavingMode::Always
		};

        // The status of a call
        public enum class CallStatus
        {
            None = 0x00,
            InProgress,
            Held
        };

        // Where is the call audio going?
        public enum class CallAudioRoute
        {
            None            = (int)Windows::Phone::Media::Devices::AvailableAudioRoutingEndpoints::None,
            Earpiece        = (int)Windows::Phone::Media::Devices::AvailableAudioRoutingEndpoints::Earpiece,
            Speakerphone    = (int)Windows::Phone::Media::Devices::AvailableAudioRoutingEndpoints::Speakerphone,
            Bluetooth       = (int)Windows::Phone::Media::Devices::AvailableAudioRoutingEndpoints::Bluetooth
        };

        // Which camera are we using?
        public enum class CameraLocation
        {
            Front = (int)Windows::Phone::Media::Capture::CameraSensorLocation::Front,
            Back = (int)Windows::Phone::Media::Capture::CameraSensorLocation::Back
        };

        // Used to indicate the status of video/audio capture/render
        public enum class MediaOperations
        {
            None                = 0x00,
            VideoCapture        = 0x01,
            VideoRender         = 0x02,
            AudioCapture        = 0x04,
            AudioRender         = 0x08
        };

        // An interface that is used by the call controller to deliver status change notifications.
        // This interface is meant to be implemented in the UI process, and will be called back by
        // the agent host process using out-of-process WinRT.
		public interface class ICallControllerStatusListener
        {
			void OnSignalBarsChanged(int newSignal);

			void OnCallStateChanged(CallState newState);

            // The status of a call has changed.
            void OnCallStatusChanged(CallStatus newStatus);

            // The call audio route has changed. Also called when the available audio routes have changed.
            void OnCallAudioRouteChanged(CallAudioRoute newRoute);

            // Video/audio capture/render has started/stopped
            void OnMediaOperationsChanged(MediaOperations newOperations);

            // Camera location has changed
            void OnCameraLocationChanged(CameraLocation newCameraLocation);
        };
    }
}
