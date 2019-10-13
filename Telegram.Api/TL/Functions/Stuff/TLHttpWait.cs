// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Stuff
{
    public class TLHttpWait : TLObject
    {
        public const string Signature = "#9299359f";

        /// <summary>
        /// Сервер ждет max_delay миллисекунд, после чего отправляет все сообщения, что у него накопились для клиента.
        /// По умолчанию 0. Второй по приоритету.
        /// </summary>
        public TLInt MaxDelay { get; set; }

        /// <summary>
        /// После получения последнего сообщения для данной сессии сервер ждет еще wait_after миллисекунд, на тот случай, если появятся еще сообщения. 
        /// Если ни одного дополнительного сообщения не появляется, отправляется результат (контейнер со всеми сообщениями); 
        /// если же появляются еще сообщения, отсчет wait_after начинается заново.
        /// По умолчанию 0. Последний по приоритету.
        /// </summary>
        public TLInt WaitAfter { get; set; }

        /// <summary>
        /// Сервер ждет не более max_wait миллисекунд, пока такое сообщение не появится.
        /// Если сообщений так и не появилось, отправляется пустой контейнер.
        /// По умолчанию 25000. Главный по приоритету.
        /// </summary>
        public TLInt MaxWait { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                MaxDelay.ToBytes(),
                WaitAfter.ToBytes(),
                MaxWait.ToBytes());
        }
    }
}
