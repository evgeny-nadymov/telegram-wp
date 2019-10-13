// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Microsoft.Xna.Framework.Audio;

namespace TelegramClient
{
    // In Windows Phone applications that use the CaptureSource class, 
    // you must also use the Microsoft.Devices.Camera, Microsoft.Devices.PhotoCamera, 
    // or Microsoft.Xna.Framework.Audio.Microphone class to enable audio capture 
    // and accurate capability detection in the application.

    // Since this sample does not need any of these classes, this unused
    // class prompts the Marketplace capability detection process to add the 
    // ID_CAP_MICROPHONE capability to the application capabilities list upon ingestion. 

    // For more information about capability detection, see: http://go.microsoft.com/fwlink/?LinkID=204620

    public class CapabilityPlaceholder
    {
        Microphone unusedMic = null;

        private string unusedMethod()
        {
            return unusedMic.ToString();
        }
    }
}
