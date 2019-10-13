using Telegram.Api.TL;

namespace Telegram.Api.Services.VoIP
{
    public interface IVoIPService
    {
        void StartOutgoingCall(TLInputUserBase userId);
    }
}
