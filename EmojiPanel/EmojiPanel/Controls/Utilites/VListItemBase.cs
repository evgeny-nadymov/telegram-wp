using System.Collections.Generic;
using System.Windows;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Telegram.Controls.VirtualizedView
{
    public abstract class VListItemBase
    {
        private readonly List<FrameworkElement> _children = new List<FrameworkElement>();

        public MyListItemBase View { get; private set; }

        public List<FrameworkElement> Children
        {
            get { return _children; }
        }

        protected VListItemBase()
        {
            View = new MyListItemBase
            {
                VirtSource = this,
                Width = 440
            };
        }
        public abstract double FixedHeight { get; set; }

        public Thickness Margin = new Thickness();

        public virtual object ItemSource { get; set; }

        private bool _isVLoaded;

        public bool IsVLoaded
        {
            get { return _isVLoaded; }
            set
            {
                if (value != IsVLoaded)
                {
                    if (value) Load();
                    else Unload();
                }
                _isVLoaded = value;
            }
        }

        public virtual void Load()
        {
            if (View.Children.Count == 0)
                foreach (var child in _children)
                    View.Children.Add(child);
        }

        public virtual void Unload()
        {
            View.Children.Clear();
        }
    }
}
