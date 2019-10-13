// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Media;
using Telegram.Api.TL;

namespace TelegramClient.Views.Controls
{
    public partial class DialogControl
    {
        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register(
            "Object", typeof(TLObject), typeof(DialogControl), new PropertyMetadata(default(TLObject), OnObjectChanged));

        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                control.Tile.Object = e.NewValue as TLObject;
            }
        }

        public TLObject Object
        {
            get { return (TLObject) GetValue(ObjectProperty); }
            set { SetValue(ObjectProperty, value); }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(DialogControl), new PropertyMetadata(default(Brush), OnFillChanged));

        private static void OnFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                control.Tile.Fill = e.NewValue as Brush;
            }
        }

        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(DialogControl), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                control.Tile.Text = e.NewValue as string;
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(DialogControl), new PropertyMetadata(default(ImageSource), OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                control.Tile.Source = e.NewValue as ImageSource;
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty ShortNameProperty = DependencyProperty.Register(
            "ShortName", typeof(string), typeof(DialogControl), new PropertyMetadata(default(string), OnShortNameChanged));

        private static void OnShortNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                control.Title.Text = e.NewValue as string;
            }
        }

        public string ShortName
        {
            get { return (string) GetValue(ShortNameProperty); }
            set { SetValue(ShortNameProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(DialogControl), new PropertyMetadata(default(bool), OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                var isSelected = (bool)e.NewValue;
                control.SelectionControl.SuppressAnimation = control.DataContext is DialogItem && ((DialogItem)control.DataContext).SuppressAnimation;
                control.SelectionControl.IsSelected = isSelected;
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty SelectedBorderBrushProperty = DependencyProperty.Register(
            "SelectedBorderBrush", typeof(Brush), typeof(DialogControl), new PropertyMetadata(default(Brush), OnSelectedBorderBrushChanged));

        public Brush SelectedBorderBrush
        {
            get { return (Brush) GetValue(SelectedBorderBrushProperty); }
            set { SetValue(SelectedBorderBrushProperty, value); }
        }

        private static void OnSelectedBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                control.SelectionControl.SelectedBorderBrush = e.NewValue as Brush;
            }
        }

        public static readonly DependencyProperty UnselectedBorderBrushProperty = DependencyProperty.Register(
            "UnselectedBorderBrush", typeof(Brush), typeof(DialogControl), new PropertyMetadata(default(Brush), OnUnselectedBorderBrushChanged));

        public Brush UnselectedBorderBrush
        {
            get { return (Brush) GetValue(UnselectedBorderBrushProperty); }
            set { SetValue(UnselectedBorderBrushProperty, value); }
        }

        private static void OnUnselectedBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DialogControl;
            if (control != null)
            {
                control.SelectionControl.UnselectedBorderBrush = e.NewValue as Brush;
            }
        }

        public DialogControl()
        {
            InitializeComponent();
        }
    }
}
