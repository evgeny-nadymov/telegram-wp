// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TelegramClient.Views.Controls
{
    public class WaveformSlider : Slider
    {
        private Path _horizontalTrack;

        private Path _horizontalFill;

        private FrameworkElement _horizontalCenterElement;

        public override void OnApplyTemplate()
        {
            _horizontalFill = GetTemplateChild("HorizontalTrack") as Path;
            _horizontalTrack = GetTemplateChild("HorizontalFill") as Path;
            _horizontalCenterElement = GetTemplateChild("HorizontalCenterElement") as FrameworkElement;

            CreatePathData(Waveform);

            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty WaveformProperty = DependencyProperty.Register(
            "Waveform", typeof (IList<double>), typeof (WaveformSlider), new PropertyMetadata(default(IList<double>), OnWaveformChanged));

        private static void OnWaveformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var waveformSlider = d as WaveformSlider;
            if (waveformSlider != null)
            {
                var waveform = e.NewValue as IList<double>;
                if (waveformSlider._horizontalFill == null
                    || waveformSlider._horizontalTrack == null
                    || waveformSlider._horizontalCenterElement == null)
                {
                    return;
                }

                waveformSlider.CreatePathData(waveform);
            }
        }

        private void CreatePathData(IList<double> waveform)
        {
            if (waveform == null)
            {
                _horizontalCenterElement.Width = 14.0;

                _horizontalFill.Data = new RectangleGeometry
                {
                    RadiusX = 2.0,
                    RadiusY = 2.0,
                    Rect = new Rect(0.0, 18.0, 204.0, 4.0)
                };
                _horizontalTrack.Data = new RectangleGeometry
                {
                    RadiusX = 2.0,
                    RadiusY = 2.0,
                    Rect = new Rect(0.0, 18.0, 204.0, 4.0)
                };
            }
            else
            {
                _horizontalCenterElement.Width = 0.0;
                var data1 = new GeometryGroup();
                var data2 = new GeometryGroup();
                var bitmapHeight = 22;
                var minBarHeight = 4;
                var barWidth = 4.0;
                var barSpace = 4.0;
                var bitmapWidth = 204.0;

                var totalBarsCount = waveform.Count;
                var barsToDisplay = (bitmapWidth - barWidth) / (barWidth + barSpace) + 1;
                var barsRate = totalBarsCount / barsToDisplay;
                for (var i = 0; i < barsToDisplay; i++)
                {
                    var startBarNumber = (int)(i * barsRate);
                    var endBarNumber = (int)((i + 1) * barsRate);
                    var peak = 0.0;
                    for (var j = startBarNumber; j < endBarNumber && j < waveform.Count; j++)
                    {
                        if (Math.Abs(waveform[j]) > peak)
                        {
                            peak = Math.Abs(waveform[j]);
                        }
                    }

                    var barHeight = peak * (bitmapHeight - minBarHeight) + minBarHeight;

                    data1.Children.Add(new RectangleGeometry { RadiusX = 2.0, RadiusY = 2.0, Rect = new Rect(i * (barWidth + barSpace), 22.0 - barHeight, 4.0, barHeight) });
                    data2.Children.Add(new RectangleGeometry { RadiusX = 2.0, RadiusY = 2.0, Rect = new Rect(i * (barWidth + barSpace), 22.0 - barHeight, 4.0, barHeight) });
                }

                _horizontalFill.Data = data1;
                _horizontalTrack.Data = data2;
            }
        }

        public IList<double> Waveform
        {
            get { return (IList<double>) GetValue(WaveformProperty); }
            set { SetValue(WaveformProperty, value); }
        }
    }
}
