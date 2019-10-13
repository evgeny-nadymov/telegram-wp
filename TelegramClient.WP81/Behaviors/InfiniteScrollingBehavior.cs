// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;

namespace TelegramClient.Behaviors
{
    public class IncrementalLoadingTrigger : TriggerBase<LongListSelector>
    {
        private int _knob = 1;

        public int Knob
        {
            get { return _knob; }
            set { _knob = value; }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemRealized += OnItemRealized;
        }

        private void OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            var longListSelector = sender as LongListSelector;
            if (longListSelector == null)
            {
                return;
            }

            var item = e.Container.Content;
            var items = longListSelector.ItemsSource;
            var index = items.IndexOf(item);

            //if (items.Count >= Knob
            //    && e.Container.Content.Equals(longListSelector.ItemsSource[longListSelector.ItemsSource.Count - Knob]))
            //{
            //    InvokeActions(null);
            //}

            if (items.Count - index <= Knob)
            {
                InvokeActions(null);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.ItemRealized -= OnItemRealized;
        }
    }

    public class InfiniteScrollingBehavior : Behavior<LongListSelector>
    {

    }
}
