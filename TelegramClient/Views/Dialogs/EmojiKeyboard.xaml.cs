using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using Telegram.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public class EmojiItem
    {
        public string String { get; set; }
        public ulong Code { get; set; }
        public Uri ImageSource { get; set; }
        public string SourceString { get; set; }
    }

    public partial class EmojiKeyboard
    {
        private int _currentCategory;

        private int _currentList;

        public static IList<EmojiItem> RecentItems = new List<EmojiItem>(); 

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(IList<EmojiItem>), typeof(EmojiKeyboard), new PropertyMetadata(default(IList<EmojiItem>)));

        public IList<EmojiItem> Items
        {
            get { return (IList<EmojiItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public EmojiKeyboard()
        {
            InitializeComponent();

            SwitchCategoryInternal(0);
        }

        private void SwitchCategory(int category)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                SwitchCategoryInternal(category);
            });
        }

        private void SwitchCategoryInternal(int category, int skip = 0)
        {
            _currentCategory = category;
            _currentList = skip;
            var items = new List<EmojiItem>();
            var timer = Stopwatch.StartNew();
            foreach (var dataItem in Emoji.Data[category].Skip(skip).Take(30))
            {
                var bytes = BitConverter.GetBytes(dataItem);

                byte[] bytesOrdered;
                Uri imageUri;
                string sourceString;
                if (bytes[6] != 0 || bytes[7] != 0)
                {
                    bytesOrdered = new byte[8];
                    bytesOrdered[0] = bytes[6];
                    bytesOrdered[1] = bytes[7];
                    bytesOrdered[2] = bytes[4];
                    bytesOrdered[3] = bytes[5];
                    bytesOrdered[4] = bytes[2];
                    bytesOrdered[5] = bytes[3];
                    bytesOrdered[6] = bytes[0];
                    bytesOrdered[7] = bytes[1];
                    sourceString = "/Images/Emoji/Items/" + BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", string.Empty) + ".png";
                    imageUri = new Uri(sourceString, UriKind.Relative);
                }
                else if (bytes[4] != 0 || bytes[5] != 0)
                {
                    bytesOrdered = new byte[6];
                    bytesOrdered[0] = bytes[4];
                    bytesOrdered[1] = bytes[5];
                    bytesOrdered[2] = bytes[2];
                    bytesOrdered[3] = bytes[3];
                    bytesOrdered[4] = bytes[0];
                    bytesOrdered[5] = bytes[1];
                    sourceString = "/Images/Emoji/Items/" + BitConverter.ToString(bytes.Take(6).Reverse().ToArray()).Replace("-", string.Empty) + ".png";
                    imageUri = new Uri(sourceString, UriKind.Relative);

                }
                else if (bytes[2] != 0 || bytes[3] != 0)
                {
                    bytesOrdered = new byte[4];
                    bytesOrdered[0] = bytes[2];
                    bytesOrdered[1] = bytes[3];
                    bytesOrdered[2] = bytes[0];
                    bytesOrdered[3] = bytes[1];
                    sourceString = "/Images/Emoji/Items/" + BitConverter.ToString(bytes.Take(4).Reverse().ToArray()).Replace("-", string.Empty) + ".png";
                    imageUri = new Uri(sourceString, UriKind.Relative);
                }
                else
                {
                    bytesOrdered = new byte[2];
                    bytesOrdered[0] = bytes[0];
                    bytesOrdered[1] = bytes[1];
                    sourceString = "/Images/Emoji/Items/" + BitConverter.ToString(bytes.Take(2).Reverse().ToArray()).Replace("-", string.Empty) + ".png";
                    imageUri = new Uri(sourceString, UriKind.Relative);
                }

                items.Add(new EmojiItem
                {
                    Code = dataItem,
                    String = System.Text.Encoding.Unicode.GetString(bytesOrdered, 0, bytesOrdered.Length),
                    ImageSource = imageUri,
                    SourceString = sourceString
                });
            }
            TLUtils.WritePerformance("%% Emoji generation time " + timer.Elapsed);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                Items = items;
            });
        }

        public event EventHandler OpenABCKeyboard;

        protected virtual void RaiseOpenAbcKeyboard()
        {
            var handler = OpenABCKeyboard;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void ABCKeyboard_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseOpenAbcKeyboard();
        }

        public event EventHandler BackspaceClick;

        protected virtual void RaiseBackspaceClick()
        {
            EventHandler handler = BackspaceClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void Backspace_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseBackspaceClick();
        }

        private void Category0_OnChecked(object sender, RoutedEventArgs e)
        {
            SwitchCategory(0);
        }

        private void Category1_OnChecked(object sender, RoutedEventArgs e)
        {
            SwitchCategory(1);
        }

        private void Category2_OnChecked(object sender, RoutedEventArgs e)
        {
            SwitchCategory(2);
        }

        private void Category3_OnChecked(object sender, RoutedEventArgs e)
        {
            SwitchCategory(3);
        }

        private void Category4_OnChecked(object sender, RoutedEventArgs e)
        {
            SwitchCategory(4);
        }

        public event EventHandler<EmojiAddedEventArgs> EmojiAdded;

        protected virtual void RaiseEmojiAdded(EmojiAddedEventArgs e)
        {
            EventHandler<EmojiAddedEventArgs> handler = EmojiAdded;
            if (handler != null) handler(this, e);
        }

        private void EmojiButton_OnClick(object sender, GestureEventArgs gestureEventArgs)
        {
            var button = (FrameworkElement)sender;
            var emojiItem = (EmojiItem)button.DataContext;

            RaiseEmojiAdded(new EmojiAddedEventArgs { Emoji = emojiItem.String });
#if WP8
            if (RecentButton.IsChecked == false)
            {
                var prevItem = RecentItems.FirstOrDefault(x => x.Code == emojiItem.Code);
                if (prevItem != null)
                {
                    RecentItems.Remove(prevItem);
                    RecentItems.Insert(0, prevItem);
                }
                else
                {
                    RecentItems.Insert(0, emojiItem);
                    RecentItems = RecentItems.Take(30).ToList();
                }
            }
#endif
        }

        private void Recent_OnChecked(object sender, RoutedEventArgs e)
        {
            Items = RecentItems;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Border) sender).Background = (Brush)App.Current.Resources["PhoneAccentBrush"];
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Background = (Brush)App.Current.Resources["PhoneChromeBrush2"];
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((Border)sender).Background = (Brush)App.Current.Resources["PhoneChromeBrush2"];
        }

        private void GestureListener_OnFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.HorizontalVelocity > 0)
            {
                
            }
            else
            {
                
            }
        }
    }

    public class EmojiAddedEventArgs : System.EventArgs
    {

        public string Emoji { get; set; }
    }
}
