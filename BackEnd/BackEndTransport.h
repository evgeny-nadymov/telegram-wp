// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once
#include "windows.h"

namespace PhoneVoIPApp
{
    namespace BackEnd
    {
        namespace TransportMessageType
        {
            enum Value
            {
                Audio = 0,
                Video = 1
            };
        }

        public delegate void MessageReceivedEventHandler(Windows::Storage::Streams::IBuffer ^pBuffer, UINT64 hnsPresentationTime, UINT64 hnsSampleDuration);

        /// <summary>
        /// This is an abstraction of a network transport class
        /// which does not actually send data over the network.
        /// </summary>
        public ref class BackEndTransport sealed
        {
        public:
            // Constructor
            BackEndTransport();

            // Destructor
            virtual ~BackEndTransport();

            void WriteAudio(BYTE* bytes, int byteCount);
            void WriteVideo(BYTE* bytes, int byteCount, UINT64 hnsPresentationTime, UINT64 hnsSampleDuration);

            event MessageReceivedEventHandler^ AudioMessageReceived;
            event MessageReceivedEventHandler^ VideoMessageReceived;

        private:
            void Write(BYTE* bytes, int byteCount, TransportMessageType::Value dataType, UINT64 hnsPresentationTime, UINT64 hnsSampleDurationTime);
        };
    }
}
