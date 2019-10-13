#pragma once

using namespace Windows::ApplicationModel::Background;

namespace PhoneVoIPApp
{
	namespace BackEnd
	{
		[Windows::Foundation::Metadata::WebHostHidden]
		public ref class BackgroundTask sealed : public IBackgroundTask
		{
		public:
			BackgroundTask();
			virtual void Run(IBackgroundTaskInstance^ taskInstance);

		private:
			void IncomingCallDissmissed();
		};
	}
}

