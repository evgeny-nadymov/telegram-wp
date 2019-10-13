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
	public partial class FlipCounter
	{
		public FlipCounter()
		{
			// Required to initialize variables
			InitializeComponent();

		    BackTile.Width = 2*6.0 + 1*8.66;
            FrontTile.Width = 2 * 6.0 + 1 * 8.66;
		}

	    private int _previousCounter;

	    public static readonly DependencyProperty CounterProperty =
	        DependencyProperty.Register("Counter", typeof (int), typeof (FlipCounter), new PropertyMetadata(default(int), OnCounterChanged));

	    private bool _initialized;

	    private static void OnCounterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	    {
	        var flipCounter = (FlipCounter) d;
            if (flipCounter != null)
            {
                var oldCounter = (int)e.OldValue;
                var oldCounterLength = e.OldValue.ToString().Length;
                var newCounter = (int)e.NewValue;
                var newCounterLength = e.NewValue.ToString().Length;
                if (!flipCounter._initialized)
                {
                    flipCounter._initialized = true;
                    flipCounter.BackTile.Width = 2 * 6.0 + newCounterLength * 8.66;
                    flipCounter.FrontTile.Width = 2 * 6.0 + newCounterLength * 8.66;
                }
                if (oldCounter != newCounter)
                {
                    if (oldCounterLength != newCounterLength)
                    {
                        flipCounter.BackTile.Width = 2 * 6.0 + newCounterLength * 8.66;
                        flipCounter.FrontTile.Width = 2 * 6.0 + newCounterLength * 8.66;
                    }

                    flipCounter.FrontText.Text = flipCounter.BackText.Text;
                    var check1 = VisualStateManager.GoToState(flipCounter, "Normal", false);
                    flipCounter.BackText.Text = Convert.ToString(newCounter);
                    var check = VisualStateManager.GoToState(flipCounter, "Flipped", true);
                }
            }
	    }

	    public int Counter
	    {
	        get { return (int) GetValue(CounterProperty); }
	        set { SetValue(CounterProperty, value); }
	    }

        private void Flip_Completed(object sender, EventArgs e)
        {
            //FrontText.Text = BackText.Text;
            //var check = VisualStateManager.GoToState(this, "Normal", false);
        }
	}
}