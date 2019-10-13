// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Media;

namespace TelegramClient.Views.Dialogs
{
    public partial class UnreadCounter
    {
        public static readonly DependencyProperty UnreadMarkProperty = DependencyProperty.Register(
            "UnreadMark", typeof(bool), typeof(UnreadCounter), new PropertyMetadata(default(bool), OnUnreadMarkChanged));

        private static void OnUnreadMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var counter = d as UnreadCounter;
            if (counter != null)
            {
                counter.UpdateLabels(counter.Count, counter.MentionsCount, (bool)e.NewValue, counter.Pinned);
            }
        }

        public bool UnreadMark
        {
            get { return (bool)GetValue(UnreadMarkProperty); }
            set { SetValue(UnreadMarkProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(UnreadCounter), new PropertyMetadata(default(Brush), OnBrushChanged));

        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        private static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var counter = d as UnreadCounter;
            if (counter != null)
            {
                counter.UnreadLabel.Background = e.NewValue as Brush;
            }
        }

        public static readonly DependencyProperty MentionsCountProperty = DependencyProperty.Register(
            "MentionsCount", typeof(int), typeof(UnreadCounter), new PropertyMetadata(default(int), OnMentionsCountChanged));

        public int MentionsCount
        {
            get { return (int)GetValue(MentionsCountProperty); }
            set { SetValue(MentionsCountProperty, value); }
        }

        private static void OnMentionsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var counter = d as UnreadCounter;
            if (counter != null)
            {
                counter.UpdateLabels(counter.Count, (int)e.NewValue, counter.UnreadMark, counter.Pinned);
            }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count", typeof(int), typeof(UnreadCounter), new PropertyMetadata(default(int), OnCountChanged));

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var counter = d as UnreadCounter;
            if (counter != null)
            {
                counter.UnreadText.Text = e.NewValue.ToString();
                counter.UpdateLabels((int)e.NewValue, counter.MentionsCount, counter.UnreadMark, counter.Pinned);
            }
        }

        public static readonly DependencyProperty PinnedProperty = DependencyProperty.Register(
            "Pinned", typeof(bool), typeof(UnreadCounter), new PropertyMetadata(default(bool), OnPinnedChanged));

        public bool Pinned
        {
            get { return (bool)GetValue(PinnedProperty); }
            set { SetValue(PinnedProperty, value); }
        }

        private static void OnPinnedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var counter = d as UnreadCounter;
            if (counter != null)
            {
                counter.UpdateLabels(counter.Count, counter.MentionsCount, counter.UnreadMark, (bool)e.NewValue);
            }
        }

        private void UpdateLabels(int unreadCount, int unreadMentionsCount, bool unreadMark, bool pinned)
        {
            UnreadMentionsLabel.Visibility = unreadMentionsCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            UnreadLabel.Visibility = unreadCount > 0 && !(unreadCount == 1 && unreadMentionsCount == 1) || unreadMark ? Visibility.Visible : Visibility.Collapsed;
            PinnedLabel.Visibility = pinned ? Visibility.Visible : Visibility.Collapsed;
        }

        public UnreadCounter()
        {
            InitializeComponent();
        }
    }
}
