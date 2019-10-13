// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using Telegram.EmojiPanel.Controls.Emoji;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class StickerPreviewMenu
    {
        public event EventHandler<StickerSelectedEventArgs> Send;

        public event EventHandler<ButtonEventArgs> OpenPack;

        public event EventHandler<StickerSelectedEventArgs> ChangeFaved;

        public event EventHandler Closed;

        private readonly UIElement _preview;

        public StickerPreviewMenu(UIElement preview)
        {
            _preview = preview;

            InitializeComponent();

            LayoutRoot.Visibility = Visibility.Collapsed;
            LayoutRoot.RenderTransform = new TranslateTransform{ Y = 0.0 };
        }

        private void Send_OnTap(object sender, GestureEventArgs e)
        {
            CloseInternal();

            var handler = Send;
            if (handler != null) handler(this, new StickerSelectedEventArgs{ Sticker = _sticker });
        }

        private void OpenPack_OnTap(object sender, GestureEventArgs e)
        {
            CloseInternal();

            var handler = OpenPack;
            if (handler != null) handler(this, new ButtonEventArgs { Button = _stickerSetButton });
        }

        private void ChangeFaved_OnTap(object sender, GestureEventArgs e)
        {
            CloseInternal();

            var handler = ChangeFaved;
            if (handler != null) handler(this, new StickerSelectedEventArgs { Sticker = _sticker });
        }

        private void Cancel_OnTap(object sender, GestureEventArgs e)
        {
            Close();
        }

        public bool IsOpened
        {
            get { return LayoutRoot.Visibility == Visibility.Visible && !_isClosing; }
        }

        public TLInputStickerSetBase InputStickerSet { get; set; }

        private IApplicationBar _applicationBar;

        public void Open()
        {
            OpenInternal();
        }

        private void OpenInternal()
        {
            _preview.IsHitTestVisible = true;

            var page = Application.Current.RootVisual.FindChildOfType<PhoneApplicationPage>();
            if (page != null)
            {
                _applicationBar = page.ApplicationBar;
                page.ApplicationBar = null;
            }

            OpenStoryboard.Begin();
        }

        private bool _fireClosed;

        private bool _isClosing;

        public void Close()
        {
            CloseInternal(true);
        }

        private void CloseInternal(bool fireClosed = false)
        {
            _fireClosed = fireClosed;

            _preview.IsHitTestVisible = false;

            if (_applicationBar != null)
            {
                var page = Application.Current.RootVisual.FindChildOfType<PhoneApplicationPage>();
                if (page != null)
                {
                    page.ApplicationBar = _applicationBar;
                }
            }

            _isClosing = true;
            Telegram.Api.Helpers.Execute.BeginOnUIThread(CloseStoryboard.Begin);
        }

        private TLStickerItem _sticker;

        public void SetStickerItem(TLStickerItem sticker)
        {
            _sticker = sticker;
        }

        private Button _stickerSetButton;

        public void SetButton(Button stickerSetButton)
        {
            _stickerSetButton = stickerSetButton;
            //OpenPackButton.Visibility = stickerSetButton == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OpenStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            
        }

        private void CloseStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            _isClosing = false;

            if (!_fireClosed)
            {
                return;
            }

            _fireClosed = false;
            var closed = Closed;
            if (closed != null) closed(this, System.EventArgs.Empty);
        }
    }

    public class ButtonEventArgs
    {
        public Button Button { get; set; }
    }
}
