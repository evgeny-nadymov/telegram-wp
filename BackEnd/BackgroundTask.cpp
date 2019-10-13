#include "BackgroundTask.h"
#include "Globals.h"
#include "CallController.h"

using namespace PhoneVoIPApp::BackEnd;
using namespace Windows::ApplicationModel::Background;
using namespace Windows::Phone::Networking::Voip;
using namespace Windows::Foundation;
using namespace Platform;
using namespace Windows::Phone::Media::Devices;

BackgroundTask::BackgroundTask()
{

}

void BackgroundTask::Run(IBackgroundTaskInstance^ taskInstance){

	return;

	VoipCallCoordinator^ callCoordinator = Windows::Phone::Networking::Voip::VoipCallCoordinator::GetDefault();

	Windows::Phone::Networking::Voip::VoipPhoneCall^ incomingCall;

	String^ installFolder = String::Concat(Windows::ApplicationModel::Package::Current->InstalledLocation->Path, "\\");
	TimeSpan timeout;
	timeout.Duration = 90 * 10 * 1000 * 1000; // in 100ns units
	callCoordinator->RequestNewIncomingCall(
		ref new String(L"/Views/ShellView.xaml"),
		ref new String(L"test contact"),
		ref new String(L"test number"),
		ref new Uri(installFolder, "Assets\\DefaultContactImage.png"),
		ref new String(L"PhoneVoIPApp"),
		ref new Uri(installFolder, "Assets\\ApplicationIcon.png"),
		ref new String(L"call details"),                      // Was this call forwarded/delegated to this user on behalf of someone else? At this time, we won't use this field
		ref new Uri(installFolder, "Assets\\Ringtone.wma"),
		VoipCallMedia::Audio,
		timeout,                // Maximum amount of time to ring for
		&incomingCall);
}

void BackgroundTask::IncomingCallDissmissed(){

}
