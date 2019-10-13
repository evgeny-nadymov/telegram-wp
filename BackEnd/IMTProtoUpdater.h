// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once

namespace PhoneVoIPApp
{
	namespace BackEnd
	{
		// An interface that is used by the call controller to start and stop mtproto communication.
		public interface class IMTProtoUpdater
		{
			// Start handle background updates.
			void Start(int pts, int date, int qts);

			// Stop handle background updates.
			void Stop();

			// Discard incoming call
			void DiscardCall(int64 id, int64 accessHash);

			// Received incoming call
			void ReceivedCall(int64 id, int64 accessHash);
		};
	}
}