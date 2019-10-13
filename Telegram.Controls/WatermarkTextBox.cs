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
using System.Windows.Media;
using Telegram.Controls.Helpers;

namespace Telegram.Controls
{
    public class WatermarkedTextBox : TextBox
    {
        public static readonly DependencyProperty InlineWatermarkProperty = DependencyProperty.Register(
            "InlineWatermark", typeof (string), typeof (WatermarkedTextBox), new PropertyMetadata(default(string), OnInlineWatermarkPropertyChanged));

        private static void OnInlineWatermarkPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as WatermarkedTextBox;
            if (textBox != null)
            {
                SetInlineWatermarkPosition(textBox);
            }
        }

        private static void SetInlineWatermarkPosition(WatermarkedTextBox textBox)
        {
            var text = textBox.Text ?? string.Empty;
            if (textBox._watermarkInlineContentBorder != null)
            {
                var measuredText = text;

                var rect = textBox.GetRectFromCharacterIndex(measuredText.Length);
#if DEBUG
    //Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("'" + text + "' " + text.Length + " " + rect));
#endif
                
                //textBox._watermarkInlineContentBorder.Margin = new Thickness(textBox.Margin.Left, textBox.Margin.Top, textBox.ActualWidth - rect.X - 15.0, textBox.Margin.Bottom);
                //textBox.GetRectFromCharacterIndex(0, )
                var delta = !measuredText.EndsWith(" ") && measuredText.Length > 0 ? rect.X - textBox.GetRectFromCharacterIndex(measuredText.Length - 1).X : 0.0;
                textBox._watermarkInlineContentBorder.RenderTransform = new TranslateTransform { X = rect.X + delta, Y = 0.0 };
            }
        }

        public string InlineWatermark
        {
            get { return (string) GetValue(InlineWatermarkProperty); }
            set { SetValue(InlineWatermarkProperty, value); }
        }

        public static readonly DependencyProperty TextScaleFactorProperty = DependencyProperty.Register(
            "TextScaleFactor", typeof(double), typeof(WatermarkedTextBox), new PropertyMetadata(1.0, OnTextScaleFactorChanged));

        private static void OnTextScaleFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (WatermarkedTextBox)d;
            if (textBox != null && textBox._contentElement != null)
            {
                var opacity = textBox._watermarkInlineContent.Opacity;
                textBox._watermarkInlineContent.Opacity = 0.0;
                textBox.FontSize = textBox._defaultFontSize * (double)e.NewValue;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    textBox._watermarkInlineContent.Opacity = opacity;
                    SetInlineWatermarkPosition(textBox);
                });
            }
        }

        private double _defaultFontSize;

        public double TextScaleFactor
        {
            get { return (double)GetValue(TextScaleFactorProperty); }
            set { SetValue(TextScaleFactorProperty, value); }
        }


        private ContentControl _watermarkContent;

        private ContentControl _watermarkInlineContent;

        private ContentControl _contentElement;

        public static readonly DependencyProperty WatermarkForegroundProperty = DependencyProperty.Register(
            "WatermarkForeground", typeof (Brush), typeof (WatermarkedTextBox), new PropertyMetadata(default(Brush)));

        public Brush WatermarkForeground
        {
            get { return (Brush) GetValue(WatermarkForegroundProperty); }
            set { SetValue(WatermarkForegroundProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(object), typeof(WatermarkedTextBox), new PropertyMetadata(OnWatermarkPropertyChanged));

        public static readonly DependencyProperty WatermarkStyleProperty =
            DependencyProperty.Register("WatermarkStyle", typeof(Style), typeof(WatermarkedTextBox), null);

        public Style WatermarkStyle
        {
            get { return GetValue(WatermarkStyleProperty) as Style; }
            set { SetValue(WatermarkStyleProperty, value); }
        }

        public object Watermark
        {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        private readonly DependencyPropertyChangedListener _listener;
        private Border _watermarkInlineContentBorder;

        public WatermarkedTextBox()
        {
            DefaultStyleKey = typeof(WatermarkedTextBox);

            _listener = DependencyPropertyChangedListener.Create(this, "Text");
            _listener.ValueChanged += OnTextChanged;
        }

        private void OnTextChanged(object sender, DependencyPropertyValueChangedEventArgs args)
        {
            if (_watermarkContent != null)
            {
                _watermarkContent.Opacity = !string.IsNullOrEmpty(Text) ? 0.0 : 0.5;
            }

            if (_watermarkInlineContent != null)
            {
                _watermarkInlineContent.Opacity = !string.IsNullOrEmpty(Text) ? 0.5 : 0.0;
            }
            //var text = Text ?? string.Empty;
            //if (_watermarkInlineContentBorder != null)
            //{
            //    var rect = GetRectFromCharacterIndex(text.Length);
            //    _watermarkInlineContentBorder.RenderTransform = new TranslateTransform { X = rect.X - 12.0, Y = rect.Y - rect.Height / 2.0 - 8.0 };
            //}
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _defaultFontSize = FontSize;
            if (TextScaleFactor > 1.0)
            {
                FontSize = _defaultFontSize*TextScaleFactor;
            }

            _watermarkContent = GetTemplateChild("WatermarkContent") as ContentControl;
            _watermarkInlineContent = GetTemplateChild("WatermarkInlineContent") as ContentControl;
            _watermarkInlineContentBorder = GetTemplateChild("WatermarkInlineContentBorder") as Border;
            _contentElement = GetTemplateChild("ContentElement") as ContentControl;

            if (_watermarkContent != null)
            {
                DetermineWatermarkContentVisibility();
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            DetermineWatermarkContentVisibility();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            DetermineWatermarkContentVisibility();
            base.OnLostFocus(e);
        }

        private static void OnWatermarkPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var watermarkTextBox = sender as WatermarkedTextBox;
            if (watermarkTextBox != null && watermarkTextBox._watermarkContent != null)
            {
                watermarkTextBox.DetermineWatermarkContentVisibility();
            }
        }

        private void DetermineWatermarkContentVisibility()
        {
            _watermarkContent.Opacity = string.IsNullOrEmpty(Text) ? 0.5 : 0.0;
        }
    }
}
