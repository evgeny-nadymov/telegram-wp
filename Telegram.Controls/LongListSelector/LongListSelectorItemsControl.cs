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

namespace Telegram.Controls.LongListSelector
{
    /// <summary>
    /// Partial definition of LongListSelector. Includes ItemsControl subclass.
    /// </summary>
    public partial class LongListSelector : Control
    {
        private class GroupSelectedEventArgs : EventArgs
        {
            public GroupSelectedEventArgs(object group)
            {
                Group = group;
            }

            public object Group { get; private set; }
        }

        private delegate void GroupSelectedEventHandler(object sender, GroupSelectedEventArgs e);

        private class LongListSelectorItemsControl : ItemsControl
        {
            public event GroupSelectedEventHandler GroupSelected;

            protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
            {
                base.PrepareContainerForItemOverride(element, item);
                ((UIElement)element).Tap += LongListSelectorItemsControl_Tap;
            }

            protected override void ClearContainerForItemOverride(DependencyObject element, object item)
            {
                base.ClearContainerForItemOverride(element, item);

                ((UIElement)element).Tap -= LongListSelectorItemsControl_Tap;
            }

            private void LongListSelectorItemsControl_Tap(object sender, System.Windows.Input.GestureEventArgs e)
            {
                ContentPresenter cc = sender as ContentPresenter;
                if (cc != null)
                {
                    var handler = GroupSelected;
                    if (handler != null)
                    {
                        GroupSelectedEventArgs args = new GroupSelectedEventArgs(cc.Content);
                        handler(this, args);
                    }

                }
            }
        }
    }
}
