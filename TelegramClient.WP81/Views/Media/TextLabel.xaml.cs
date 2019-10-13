// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace TelegramClient.Views.Media
{
    public abstract class SelectableUserControl : UserControl
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(SelectableUserControl), new PropertyMetadata(default(bool), OnIsSelectedChangedInternal));

        private static void OnIsSelectedChangedInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selectableUserControl = d as SelectableUserControl;
            if (selectableUserControl != null)
            {
                var isSelected = (bool)e.NewValue;

                selectableUserControl.OnIsSelectedChanged(selectableUserControl, isSelected);
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        protected abstract void OnIsSelectedChanged(object sender, bool isSelected);
    }

    public partial class TextLabel
    {
        protected override void OnIsSelectedChanged(object sender, bool isSelected)
        {
            Border.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
            //LeftRotatePoint.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
            //RightRotatePoint.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TextLabel), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textLabel = d as TextLabel;
            if (textLabel != null)
            {
                textLabel.Label.Text = e.NewValue as string;
                textLabel.LabelShadow.Text = e.NewValue as string;
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private double _colorPosition;

        public double ColorPosition
        {
            get { return _colorPosition; }
            set
            {
                _colorPosition = value;
#if DEBUG
                //DEBUG.Text = _colorPosition.ToString();
#endif
            }
        }

        public TextLabel()
        {
            InitializeComponent();
        }

        private ContextMenu _lastContextMenu;

        private void RemoveContextMenu()
        {
            _lastContextMenu = ContextMenuService.GetContextMenu(this);

            ContextMenuService.SetContextMenu(this, null);
        }

        private void RestoreContextMenu()
        {
            ContextMenuService.SetContextMenu(this, _lastContextMenu);

            _lastContextMenu = null;
        }

        private void DebugManipulationDelta(ManipulationDeltaEventArgs e)
        {
            DEBUG.Text = string.Format("pinch{0}\ncumulative\nX={1}\nY={2}\ndelta\nX={3}\nY={4}\n{5}\n{6}",
                e.PinchManipulation,
                e.CumulativeManipulation.Translation.X,
                e.CumulativeManipulation.Translation.Y,
                e.DeltaManipulation.Translation.X,
                e.DeltaManipulation.Translation.Y,
                e.CumulativeManipulation.Scale,
                e.DeltaManipulation.Scale);
        }

        private void LeftRotatePoint_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;

            RemoveContextMenu();
        }

        private void LeftRotatePoint_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            DebugManipulationDelta(e);

            e.Handled = true;

            //var pointA = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
            //var pointB = new Point(0.0, ActualHeight / 2.0);
            //var pointC = new Point(pointB.X + e.DeltaManipulation.Translation.X, pointB.Y + e.DeltaManipulation.Translation.Y);

            //var AB = Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2.0) + Math.Pow(pointA.Y - pointB.Y, 2.0));
            //var BC = Math.Sqrt(Math.Pow(pointB.X - pointC.X, 2.0) + Math.Pow(pointB.Y - pointC.Y, 2.0));
            //var AC = Math.Sqrt(Math.Pow(pointA.X - pointC.X, 2.0) + Math.Pow(pointA.Y - pointC.Y, 2.0));

            //var cosA = (AB * AB + AC * AC - BC * BC) / (2.0 * AB * AC);
            //var a = Math.Acos(cosA) * 180.0 / Math.PI;
            //if (pointC.Y > pointB.Y)
            //{
            //    a = -a;
            //}

            //RotateTransform.CenterX = ActualWidth/2.0;
            //RotateTransform.CenterY = ActualHeight/2.0;
            //RotateTransform.Angle += a;
        }

        private void LeftRotatePoint_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            RestoreContextMenu();
        }

        private void RightRotatePoint_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;

            RemoveContextMenu();
        }

        private void RightRotatePoint_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            DebugManipulationDelta(e);

            e.Handled = true;

            //var pointA = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
            //var pointB = new Point(0.0, ActualHeight / 2.0);
            //var pointC = new Point(pointB.X + e.DeltaManipulation.Translation.X, pointB.Y + e.DeltaManipulation.Translation.Y);

            //var AB = Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2.0) + Math.Pow(pointA.Y - pointB.Y, 2.0));
            //var BC = Math.Sqrt(Math.Pow(pointB.X - pointC.X, 2.0) + Math.Pow(pointB.Y - pointC.Y, 2.0));
            //var AC = Math.Sqrt(Math.Pow(pointA.X - pointC.X, 2.0) + Math.Pow(pointA.Y - pointC.Y, 2.0));

            //var cosA = (AB * AB + AC * AC - BC * BC) / (2.0 * AB * AC);
            //var a = Math.Acos(cosA) * 180.0 / Math.PI;
            //if (pointC.Y < pointB.Y)
            //{
            //    a = -a;
            //}

            //RotateTransform.CenterX = ActualWidth / 2.0;
            //RotateTransform.CenterY = ActualHeight / 2.0;
            //RotateTransform.Angle += a;

        }

        private void RightRotatePoint_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            RestoreContextMenu();
        }

        public void SetForeground(SolidColorBrush foreground)
        {
            Label.Foreground = foreground;
        }

        public SolidColorBrush GetForeground()
        {
            return Label.Foreground as SolidColorBrush;
        }
    }
}
