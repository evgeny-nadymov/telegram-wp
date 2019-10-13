using System.Collections.ObjectModel;
using Caliburn.Micro;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Media
{

    public class PivotImageViewerViewModel : PropertyChangedBase
    {
        private ImageViewerViewModel _activeItem;

        public ImageViewerViewModel ActiveItem
        {
            get { return _activeItem; }
            set
            {
                if (value != _activeItem)
                {
                    _activeItem = value;
                    NotifyOfPropertyChange(() => ActiveItem);
                }
            }
        }

        public ObservableCollection<ImageViewerViewModel> Items { get; set; }

        private IStateService _stateService; 

        public PivotImageViewerViewModel(IStateService stateService)
        {
            _stateService = stateService;

            Items = new ObservableCollection<ImageViewerViewModel>();
            Items.Add(FirstItem);
            Items.Add(SecondItem);
            Items.Add(ThirdItem);
        }

        public ImageViewerViewModel FirstItem { get; set; }

        public ImageViewerViewModel SecondItem { get; set; }

        public ImageViewerViewModel ThirdItem { get; set; }

        
    }
}
