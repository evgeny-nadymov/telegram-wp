// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#include "pch.h"
#include "App.h"

#include <windows.applicationmodel.core.h>
#include <wrl.h>
#include <string>
#include <stdexcept>

using namespace ABI::Windows::ApplicationModel::Core;
using namespace ABI::Windows::Foundation;
using namespace Microsoft::WRL;
using namespace Wrappers;

HRESULT __cdecl MyGetActivationFactory(_In_ HSTRING activatableClassId, _COM_Outptr_ IInspectable **factory);

class GetCustomClass : public RuntimeClass<RuntimeClassFlags<RuntimeClassType::WinRtClassicComMix>,
	IGetActivationFactory,
	CloakedIid<IAgileObject >>
{
public:
	IFACEMETHODIMP GetActivationFactory(_In_ HSTRING activatableClassId, _COM_Outptr_ IInspectable **factory)
	{
		return MyGetActivationFactory(activatableClassId, factory);
	}

private:
	HMODULE m_hMod;
};

[Platform::MTAThread]
int main(Platform::Array<Platform::String^>^)
{
	HRESULT hr = Initialize(RO_INIT_MULTITHREADED);
	if (FAILED(hr))
	{
		throw std::runtime_error(std::string("Failed to Initialize(RO_INIT_MULTITHREADED), HRESULT: ").append(std::to_string(hr)));
	}

	// Scoping for smart pointers
	{
		ComPtr<ICoreApplication> spApplicationFactory;
		hr = GetActivationFactory(HStringReference(RuntimeClass_Windows_ApplicationModel_Core_CoreApplication).Get(), &spApplicationFactory);
		if (FAILED(hr))
		{
			throw std::runtime_error(std::string("Failed to GetActivationFactory(RuntimeClass_Windows_ApplicationModel_Core_CoreApplication), HRESULT: ").append(std::to_string(hr)));
		}

		ComPtr<IGetActivationFactory> spGetActivationFactory = Make<GetCustomClass>();
		spApplicationFactory->RunWithActivationFactories(spGetActivationFactory.Get());
	}

	Uninitialize();
}

