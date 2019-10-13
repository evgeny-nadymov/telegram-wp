// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// A double headed slider for selecting a range.
    /// </summary>
    public class RangeSlider : Control
    {
        /// <summary>
        /// The minimum value dependency protperty.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new PropertyMetadata(0.0, new PropertyChangedCallback(RangeBounds_Changed)));

        /// <summary>
        /// The maximum value dependency protperty.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new PropertyMetadata(1.0, new PropertyChangedCallback(RangeBounds_Changed)));

        /// <summary>
        /// The minimum range span dependency protperty.
        /// </summary>
        public static readonly DependencyProperty MinimumRangeSpanProperty =
            DependencyProperty.Register("MinimumRangeSpan", typeof(double), typeof(RangeSlider), new PropertyMetadata(0.0));

        /// <summary>
        /// The range start dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeStartProperty =
            DependencyProperty.Register("RangeStart", typeof(double), typeof(RangeSlider), new PropertyMetadata(0.0, new PropertyChangedCallback(Range_Changed)));

        /// <summary>
        /// The range end dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeEndProperty =
            DependencyProperty.Register("RangeEnd", typeof(double), typeof(RangeSlider), new PropertyMetadata(1.0, new PropertyChangedCallback(Range_Changed)));

        /// <summary>
        /// The element name for the range start thumb.
        /// </summary>
        private const string ElementRangeStartThumb = "RangeStartThumb";

        /// <summary>
        /// The element name for the range center thumb.
        /// </summary>
        private const string ElementRangeCenterThumb = "RangeCenterThumb";

        /// <summary>
        /// The element name for the range end thumb.
        /// </summary>
        private const string ElementRangeEndThumb = "RangeEndThumb";

        /// <summary>
        /// The element name for the selected range borer.
        /// </summary>
        private const string ElementSelectedRangeBorder = "SelectedRangeBorder";

        /// <summary>
        /// The range start thumb.
        /// </summary>
        private Thumb rangeStartThumb;

        /// <summary>
        /// The range center thumb.
        /// </summary>
        private Thumb rangeCenterThumb;

        /// <summary>
        /// The range end thumb.
        /// </summary>
        private Thumb rangeEndThumb;

        /// <summary>
        /// The selected range border.
        /// </summary>
        private Border selectedRangeBorder;

        /// <summary>
        /// RangeSlider constructor.
        /// </summary>
        public RangeSlider()
        {
            this.DefaultStyleKey = typeof(RangeSlider);
            this.SizeChanged += new SizeChangedEventHandler(this.RangeSlider_SizeChanged);
        }

        /// <summary>
        /// RangeChanged event.
        /// </summary>
        public event EventHandler RangeChanged;

        /// <summary>
        /// Gets or sets the slider minimum value.
        /// </summary>
        public double Minimum
        {
            get
            {
                return (double)GetValue(MinimumProperty);
            }

            set
            {
                SetValue(MinimumProperty, Math.Min(this.Maximum, Math.Max(0, value)));

                if (this.Maximum - this.Minimum < this.MinimumRangeSpan)
                {
                    this.MinimumRangeSpan = this.Maximum - this.Minimum;
                }
            }
        }

        /// <summary>
        /// Gets or sets the slider maximum value.
        /// </summary>
        public double Maximum
        {
            get
            {
                return (double)GetValue(MaximumProperty);
            }

            set
            {
                SetValue(MaximumProperty, Math.Max(this.Minimum, value));

                if (this.Maximum - this.Minimum < this.MinimumRangeSpan)
                {
                    this.MinimumRangeSpan = this.Maximum - this.Minimum;
                }
            }
        }

        /// <summary>
        /// Gets or sets the slider minimum range span.
        /// </summary>
        public double MinimumRangeSpan
        {
            get
            {
                return (double)GetValue(MinimumRangeSpanProperty);
            }

            set
            {
                SetValue(MinimumRangeSpanProperty, Math.Min(this.Maximum - this.Minimum, value));
                this.UpdateSelectedRangeMinimumWidth();
                this.UpdateRange(false);
            }
        }

        /// <summary>
        /// Gets or sets the range start.
        /// </summary>
        public double RangeStart
        {
            get
            {
                return (double)GetValue(RangeStartProperty);
            }

            set
            {
                double rangeStart = Math.Max(this.Minimum, value);
                SetValue(RangeStartProperty, rangeStart);
            }
        }

        /// <summary>
        /// Gets or sets the range end.
        /// </summary>
        public double RangeEnd
        {
            get
            {
                return (double)GetValue(RangeEndProperty);
            }

            set
            {
                double rangeEnd = Math.Min(this.Maximum, value);
                SetValue(RangeEndProperty, rangeEnd);
            }
        }

        /// <summary>
        /// Gets the template parts from the template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.selectedRangeBorder = this.GetTemplateChild(RangeSlider.ElementSelectedRangeBorder) as Border;
            this.rangeStartThumb = this.GetTemplateChild(RangeSlider.ElementRangeStartThumb) as Thumb;
            if (this.rangeStartThumb != null)
            {
                this.rangeStartThumb.DragDelta += new DragDeltaEventHandler(this.RangeStartThumb_DragDelta);
                this.rangeStartThumb.SizeChanged += new SizeChangedEventHandler(this.RangeThumb_SizeChanged);
            }

            this.rangeCenterThumb = this.GetTemplateChild(RangeSlider.ElementRangeCenterThumb) as Thumb;
            if (this.rangeCenterThumb != null)
            {
                this.rangeCenterThumb.DragDelta += new DragDeltaEventHandler(this.RangeCenterThumb_DragDelta);
            }

            this.rangeEndThumb = this.GetTemplateChild(RangeSlider.ElementRangeEndThumb) as Thumb;
            if (this.rangeEndThumb != null)
            {
                this.rangeEndThumb.DragDelta += new DragDeltaEventHandler(this.RangeEndThumb_DragDelta);
                this.rangeEndThumb.SizeChanged += new SizeChangedEventHandler(this.RangeThumb_SizeChanged);
            }
        }

        #region Dependency property events.
        /// <summary>
        /// Updates the slider when the selected range changes.
        /// </summary>
        /// <param name="d">The range slider.</param>
        /// <param name="args">Dependency Property Changed Event Args.</param>
        private static void Range_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            RangeSlider rangeSlider = d as RangeSlider;
            rangeSlider.UpdateSlider();

            if (rangeSlider.RangeChanged != null)
            {
                rangeSlider.RangeChanged(rangeSlider, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Updates the range start and end values.
        /// </summary>
        /// <param name="d">The range slider.</param>
        /// <param name="args">Dependency Property Changed Event Args.</param>
        private static void RangeBounds_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            (d as RangeSlider).UpdateRange(true);
        }
        #endregion

        #region Range Slider events
        /// <summary>
        /// Updates the slider UI.
        /// </summary>
        /// <param name="sender">The range slider.</param>
        /// <param name="e">Size Changed Event Args.</param>
        private void RangeSlider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateSelectedRangeMinimumWidth();
            this.UpdateSlider();
        }
        #endregion

        #region Thumb events
        /// <summary>
        /// Updates the slider's minimum width.
        /// </summary>
        /// <param name="sender">The range thumb.</param>
        /// <param name="e">Size changed event args.</param>
        private void RangeThumb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateSelectedRangeMinimumWidth();
        }

        /// <summary>
        /// Moves the whole range slider.
        /// </summary>
        /// <param name="sender">The range cetner thumb.</param>
        /// <param name="e">Drag Delta Event Args.</param>
        private void RangeCenterThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.selectedRangeBorder != null)
            {
                double startMargin = this.selectedRangeBorder.Margin.Left + e.HorizontalChange;
                double endMargin = this.selectedRangeBorder.Margin.Right - e.HorizontalChange;

                if (startMargin + e.HorizontalChange <= 0)
                {
                    startMargin = 0;
                    endMargin = Math.Round(this.ActualWidth - (((this.RangeEnd - this.RangeStart) / (this.Maximum - this.Minimum)) * this.ActualWidth), 0);
                }
                else if (endMargin - e.HorizontalChange <= 0)
                {
                    endMargin = 0;
                    startMargin = Math.Round(this.ActualWidth - (((this.RangeEnd - this.RangeStart) / (this.Maximum - this.Minimum)) * this.ActualWidth), 0);
                }

                if (!double.IsNaN(startMargin) && !double.IsNaN(endMargin))
                {
                    this.selectedRangeBorder.Margin = new Thickness(
                        startMargin,
                        this.selectedRangeBorder.Margin.Top,
                        endMargin,
                        this.selectedRangeBorder.Margin.Bottom);
                }

                this.UpdateRange(true);
            }
        }

        /// <summary>
        /// Moves the range end thumb.
        /// </summary>
        /// <param name="sender">The range end thumb.</param>
        /// <param name="e">Drag Delta Event Args.</param>
        private void RangeEndThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.selectedRangeBorder != null)
            {
                double endMargin = Math.Round(Math.Min(this.ActualWidth - this.selectedRangeBorder.MinWidth, Math.Max(0, this.selectedRangeBorder.Margin.Right - e.HorizontalChange)), 0);
                double startMargin = Math.Round(this.selectedRangeBorder.Margin.Left, 0);

                if (this.ActualWidth - startMargin - endMargin < this.selectedRangeBorder.MinWidth)
                {
                    startMargin = Math.Round(this.ActualWidth - endMargin - this.selectedRangeBorder.MinWidth, 0);
                }

                this.selectedRangeBorder.Margin = new Thickness(
                    startMargin,
                    this.selectedRangeBorder.Margin.Top,
                    endMargin,
                    this.selectedRangeBorder.Margin.Bottom);

                this.UpdateRange(true);
            }
        }

        /// <summary>
        /// Moves the range start thumb.
        /// </summary>
        /// <param name="sender">The range start thumb.</param>
        /// <param name="e">Drag Delta Event Args.</param>
        private void RangeStartThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.selectedRangeBorder != null)
            {
                double startMargin = Math.Round(Math.Min(this.ActualWidth - this.selectedRangeBorder.MinWidth, Math.Max(0, this.selectedRangeBorder.Margin.Left + e.HorizontalChange)), 0);
                double endMargin = Math.Round(this.selectedRangeBorder.Margin.Right, 0);

                if (this.ActualWidth - startMargin - endMargin < this.selectedRangeBorder.MinWidth)
                {
                    endMargin = Math.Round(this.ActualWidth - startMargin - this.selectedRangeBorder.MinWidth, 0);
                }

                

                this.selectedRangeBorder.Margin = new Thickness(
                    startMargin,
                    this.selectedRangeBorder.Margin.Top,
                    endMargin,
                    this.selectedRangeBorder.Margin.Bottom);

                this.UpdateRange(true);
            }
        }

        public void Log(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
        }

        #endregion

        /// <summary>
        /// Updates the thumb mimimum width.
        /// </summary>
        private void UpdateSelectedRangeMinimumWidth()
        {
            if (this.selectedRangeBorder != null && this.rangeStartThumb != null && this.rangeEndThumb != null)
            {
                this.selectedRangeBorder.MinWidth = Math.Max(
                    this.rangeStartThumb.ActualWidth + this.rangeEndThumb.ActualWidth,
                    (this.MinimumRangeSpan / ((this.Maximum - this.Minimum) == 0 ? 1 : (this.Maximum - this.Minimum))) * this.ActualWidth);
            }
        }

        /// <summary>
        /// Updates the slider UI.
        /// </summary>
        private void UpdateSlider()
        {
            if (this.selectedRangeBorder != null)
            {
                double startMargin = (this.RangeStart / (this.Maximum - this.Minimum)) * this.ActualWidth;
                double endMargin = ((this.Maximum - this.RangeEnd) / (this.Maximum - this.Minimum)) * this.ActualWidth;

                if (!double.IsNaN(startMargin) && !double.IsNaN(endMargin))
                {
                    this.selectedRangeBorder.Margin = new Thickness(
                            startMargin,
                            this.selectedRangeBorder.Margin.Top,
                            endMargin,
                            this.selectedRangeBorder.Margin.Bottom);
                }
            }
        }

        /// <summary>
        /// Updates the selected range.
        /// </summary>
        /// <param name="raiseEvent">Whether the range changed event should fire.</param>
        private void UpdateRange(bool raiseEvent)
        {
            if (this.selectedRangeBorder != null)
            {
                bool rangeChanged = false;
                double rangeStart = Math.Round(((this.Maximum - this.Minimum) * (this.selectedRangeBorder.Margin.Left / this.ActualWidth)) + this.Minimum, 0);
                double rangeEnd = Math.Round(this.Maximum - ((this.Maximum - this.Minimum) * (this.selectedRangeBorder.Margin.Right / this.ActualWidth)), 0);

                if (rangeEnd - rangeStart < this.MinimumRangeSpan)
                {
                    if (rangeStart + this.MinimumRangeSpan > this.Maximum)
                    {
                        rangeStart = this.Maximum - this.MinimumRangeSpan;
                    }

                    rangeEnd = Math.Min(this.Maximum, rangeStart + this.MinimumRangeSpan);
                }

                if (rangeStart != this.RangeStart || rangeEnd != this.RangeEnd)
                {
                    rangeChanged = true;
                }

                this.RangeStart = rangeStart;
                this.RangeEnd = rangeEnd;

                if (raiseEvent && rangeChanged && this.RangeChanged != null)
                {
                    this.RangeChanged(this, EventArgs.Empty);
                }
            }
        }
    }
}
