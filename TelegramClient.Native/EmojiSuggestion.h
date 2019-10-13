// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once

#include "emoji_suggestions.h"

using namespace Platform;
using namespace Ui::Emoji;

namespace TelegramClient_Native
{
	public ref class EmojiSuggestion sealed
	{
	public:
		static Array<EmojiSuggestion^>^ GetSuggestions(String^ query);
		static int GetSuggestionMaxLength();


		property String^ Emoji
		{
			String^ get() { return m_emoji; }
		}

		property String^ Label
		{
			String^ get() { return m_label; }
		}

		property String^ Replacement
		{
			String^ get() { return m_replacement; }
		}


	private:
		EmojiSuggestion(Suggestion suggestion);

		String^ m_emoji;
		String^ m_label;
		String^ m_replacement;
	};
}
