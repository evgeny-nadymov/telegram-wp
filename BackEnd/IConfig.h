#pragma once

namespace PhoneVoIPApp
{
	namespace BackEnd
	{
		public interface class IConfig
		{
			property double InitTimeout;
			property double RecvTimeout;
		};
	}
}