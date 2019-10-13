// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Interactivity;

namespace Telegram.Controls.Triggers
{
    public class CompressionTrigger : TriggerBase<LazyListBox>
    {
        public static DependencyProperty IsDisabledProperty = DependencyProperty.Register("IsDisabled", typeof(bool), typeof(CompressionTrigger), null);

        public bool IsDisabled
        {
            get { return (bool)GetValue(IsDisabledProperty); }
            set { SetValue(IsDisabledProperty, value); }
        }

        public static DependencyProperty CompressionTypeProperty = DependencyProperty.Register("CompressionType", typeof(CompressionType), typeof(CompressionTrigger), null);

        public CompressionType CompressionType
        {
            get { return (CompressionType)GetValue(CompressionTypeProperty); }
            set { SetValue(CompressionTypeProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Compression += AssociatedObject_Compression;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Compression -= AssociatedObject_Compression;

            base.OnDetaching();
        }

        private void AssociatedObject_Compression(object sender, CompressionEventArgs args)
        {
            if (!IsDisabled
                && args.Type == CompressionType)
            {
                InvokeActions(null);
            }
        }
    }

    public class CompressionTrigger2 : TriggerBase<LazyItemsControl>
    {
        public static DependencyProperty IsDisabledProperty = DependencyProperty.Register("IsDisabled", typeof(bool), typeof(CompressionTrigger2), null);

        public bool IsDisabled
        {
            get { return (bool)GetValue(IsDisabledProperty); }
            set { SetValue(IsDisabledProperty, value); }
        }

        public static DependencyProperty CompressionTypeProperty = DependencyProperty.Register("CompressionType", typeof(CompressionType), typeof(CompressionTrigger2), null);

        public CompressionType CompressionType
        {
            get { return (CompressionType)GetValue(CompressionTypeProperty); }
            set { SetValue(CompressionTypeProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Compression += AssociatedObject_Compression;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Compression -= AssociatedObject_Compression;

            base.OnDetaching();
        }

        private void AssociatedObject_Compression(object sender, CompressionEventArgs args)
        {
            if (!IsDisabled
                && args.Type == CompressionType)
            {
                InvokeActions(null);
            }
        }
    }
}
