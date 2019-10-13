// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;

namespace Telegram.Controls
{
    public partial class FlipPanel
    {
        public FlipPanel()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register("TextBlockStyle", typeof (Style), typeof (FlipPanel), new PropertyMetadata(default(Style), OnTextBlockStyleChanged));

        private static void OnTextBlockStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (FlipPanel)d;
            if (panel != null)
            {
                panel.FrontText.Style = (Style)e.NewValue;
                panel.BackText.Style = (Style)e.NewValue;
            }

        }

        public Style TextBlockStyle
        {
            get { return (Style) GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }


        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FlipPanel), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (FlipPanel)d;
            if (panel != null)
            {
                var oldText = (string)e.OldValue;
                var newText = (string)e.NewValue;
                
                if (oldText != newText)
                {

                    panel.FrontText.Text = panel.BackText.Text;
                    var check1 = VisualStateManager.GoToState(panel, "Normal", false);
                    panel.BackText.Text = Convert.ToString(newText);
                    var check = VisualStateManager.GoToState(panel, "Flipped", true);
                }
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
