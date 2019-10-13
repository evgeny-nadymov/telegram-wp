// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class SecretChatsViewModel : ViewModelBase
    {
        public static readonly object LinkPreviewsSyncRoot = new object();

        private bool _linkPreviews;

        public bool LinkPreviews
        {
            get { return _linkPreviews; }
            set { SetField(ref _linkPreviews, value, () => LinkPreviews); }
        }

        public SecretChatsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _linkPreviews = StateService.LinkPreviews;
            StateService.LinkPreviews = false;

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => LinkPreviews))
                {
                    TLUtils.SaveObjectToMTProtoFile(LinkPreviewsSyncRoot, Constants.WebPagePreviewsFileName, new TLBool(LinkPreviews));
                }
            };
        }
    }
}
