// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once
#include <windows.h>
#include "Globals.h"

namespace PhoneVoIPApp
{
    namespace BackEnd
    {
        namespace OutOfProcess
        {
            // A remotely activatable class that is used by the UI process and managed code within
            // the VoIP background process to get access to native objects that exist in the VoIP background process.
            public ref class Server sealed
            {
            public:
                // Constructor
                Server()
                {
                }

                // Destructor
                virtual ~Server()
                {
                }

                // Called by the UI process to get the call controller object
                property CallController^ CallController
                {
                    PhoneVoIPApp::BackEnd::CallController^ get()
                    {
                        return Globals::Instance->CallController;
                    };
                }

                // Add methods and properties to get other objects here, as required.
            };
        }
    }
}
