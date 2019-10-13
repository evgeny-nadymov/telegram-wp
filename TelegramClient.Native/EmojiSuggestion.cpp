// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"
#include "EmojiSuggestion.h"
#include "emoji_suggestions.h"

using namespace TelegramClient_Native;
using namespace Platform;
using namespace Ui::Emoji;

int EmojiSuggestion::GetSuggestionMaxLength()
{
	return Ui::Emoji::GetSuggestionMaxLength();
}

EmojiSuggestion::EmojiSuggestion(Suggestion suggestion)
{
	auto emoji = reinterpret_cast<const wchar_t*>(suggestion.emoji().data());
	m_emoji = ref new String(emoji, suggestion.emoji().size());

	auto label = reinterpret_cast<const wchar_t*>(suggestion.label().data());
	m_label = ref new String(label, suggestion.label().size());

	auto replacement = reinterpret_cast<const wchar_t*>(suggestion.replacement().data());
	m_replacement = ref new String(replacement, suggestion.replacement().size());
}

Array<EmojiSuggestion^>^ EmojiSuggestion::GetSuggestions(String^ query)
{
	auto data = reinterpret_cast<const utf16char*>(query->Data());
	auto results = Ui::Emoji::GetSuggestions(utf16string(data, query->Length()));

	std::vector<EmojiSuggestion^> suggestions;

	for (auto &item : results)
	{
		suggestions.push_back(ref new EmojiSuggestion(item));
	}

	return ref new Array<EmojiSuggestion^>(suggestions.data(), suggestions.size());
}
