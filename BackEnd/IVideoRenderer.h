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
        // An interface that is used by the call controller to start and stop video rendering.
        public interface class IVideoRenderer
        {
            // Start rendering video.
            void Start();

            // Stop rendering video.
            void Stop();
        };
    }
}
