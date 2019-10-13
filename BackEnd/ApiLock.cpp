// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "ApiLock.h"

namespace PhoneVoIPApp
{
    namespace BackEnd
    {
        // A mutex used to protect objects accessible from the API surface exposed by this DLL
        std::recursive_mutex g_apiLock;
    }
}
