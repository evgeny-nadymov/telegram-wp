// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows.Controls;
using Telegram.Controls.VirtualizedView;

namespace Telegram.EmojiPanel.Controls.Utilites
{
    public class MyListItemBase : Grid
    {
        //private Panel _contentPanel;
        //public Panel ContentPanel
        //{
        //    get
        //    {
        //        return _contentPanel;
        //    }
        //    set
        //    {
        //        _contentPanel = value;
        //        Content = _contentPanel;
        //    }
        //}

        public VListItemBase VirtSource { get; set; }
    }
}
