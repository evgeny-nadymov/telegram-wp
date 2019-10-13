// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TelegramClient.Views.Controls
{

    public class PieSlice : Path
    {
        private bool m_HasLoaded = false;
        public PieSlice()
        {
            Loaded += (s, e) =>
            {
                m_HasLoaded = true;
                UpdatePath();
            };
        }

        // StartAngle
        public static readonly DependencyProperty StartAngleProperty
            = DependencyProperty.Register("StartAngle", typeof(double), typeof(PieSlice),
            new PropertyMetadata(DependencyProperty.UnsetValue, (s, e) => { Changed(s as PieSlice); }));
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        // Angle
        public static readonly DependencyProperty AngleProperty
            = DependencyProperty.Register("Angle", typeof(double), typeof(PieSlice),
            new PropertyMetadata(DependencyProperty.UnsetValue, (s, e) => { Changed(s as PieSlice); }));
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // Radius
        public static readonly DependencyProperty RadiusProperty
            = DependencyProperty.Register("Radius", typeof(double), typeof(PieSlice),
            new PropertyMetadata(DependencyProperty.UnsetValue, (s, e) => { Changed(s as PieSlice); }));
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        private static void Changed(PieSlice pieSlice)
        {
            if (pieSlice.m_HasLoaded)
                pieSlice.UpdatePath();
        }

        public void UpdatePath()
        {
            // ensure variables
            if (GetValue(StartAngleProperty) == DependencyProperty.UnsetValue)
                throw new ArgumentNullException("Start Angle is required");
            if (GetValue(RadiusProperty) == DependencyProperty.UnsetValue)
                throw new ArgumentNullException("Radius is required");
            if (GetValue(AngleProperty) == DependencyProperty.UnsetValue)
                throw new ArgumentNullException("Angle is required");

            Width = Height = 2 * (Radius + StrokeThickness);
            var _EndAngle = StartAngle + Angle;

            // path container
            var _FigureP = new Point(Radius, Radius);
            var _Figure = new PathFigure
            {
                StartPoint = _FigureP,
                IsClosed = true,
            };

            //  start angle line
            var _LineX = Radius + Math.Sin(StartAngle * Math.PI / 180) * Radius;
            var _LineY = Radius - Math.Cos(StartAngle * Math.PI / 180) * Radius;
            var _LineP = new Point(_LineX, _LineY);
            var _Line = new LineSegment { Point = _LineP };
            _Figure.Segments.Add(_Line);

            // outer arc
            var _ArcX = Radius + Math.Sin(_EndAngle * Math.PI / 180) * Radius;
            var _ArcY = Radius - Math.Cos(_EndAngle * Math.PI / 180) * Radius;
            var _ArcS = new Size(Radius, Radius);
            var _ArcP = new Point(_ArcX, _ArcY);
            var _Arc = new ArcSegment
            {
                IsLargeArc = Angle >= 180.0,
                Point = _ArcP,
                Size = _ArcS,
                SweepDirection = SweepDirection.Clockwise,
            };
            _Figure.Segments.Add(_Arc);

            // finalé
            Data = new PathGeometry { Figures = { _Figure } };
            InvalidateArrange();
        }


    }
}
