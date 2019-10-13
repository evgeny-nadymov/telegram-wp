using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Telegram.Controls.VirtualizedView;

namespace Telegram.Controls.VirtualizedView
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
