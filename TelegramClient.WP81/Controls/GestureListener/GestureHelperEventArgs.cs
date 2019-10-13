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

namespace TelegramClient.Controls.GestureListener
{
    /// <summary>
    /// The base class for all gesture events. Also used by Tap, DoubleTap and Hold.
    /// </summary>
    public class GestureEventArgs : System.EventArgs
    {
        /// <summary>
        /// The point, in unrotated screen coordinates, where the gesture occurred.
        /// </summary>
        protected Point GestureOrigin { get; private set; }
        
        /// <summary>
        /// The point, in unrotated screen coordinates, where the first touchpoint is now.
        /// </summary>
        protected Point TouchPosition { get; private set; }

        internal GestureEventArgs(Point gestureOrigin, Point position)
        {
            GestureOrigin = gestureOrigin;
            TouchPosition = position;
        }

        /// <summary>
        /// The first hit-testable item under the touch point. Determined by a combination of order in the tree and
        /// Z-order.
        /// </summary>
        public object OriginalSource { get; internal set; }

        /// <summary>
        /// If an event handler sets this to true, it stops event bubbling.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Returns the position of the gesture's starting point relative to a given UIElement.
        /// </summary>
        /// <param name="relativeTo">The return value will be relative to this element.</param>
        /// <returns>The gesture's starting point relative to the given UIElement.</returns>
        public Point GetPosition(UIElement relativeTo)
        {
            return GetPosition(relativeTo, TouchPosition);
        }

        /// <summary>
        /// Returns the position of a given point relative to a given UIElement.
        /// </summary>
        /// <param name="relativeTo">The return value will be relative to this element.</param>
        /// <param name="point">The point to translate.</param>
        /// <returns>The given point relative to the given UIElement.</returns>
        protected static Point GetPosition(UIElement relativeTo, Point point)
        {
            if (relativeTo == null)
            {
                // Transform relative to RootVisual
                relativeTo = Application.Current.RootVisual;
            }
            if (relativeTo != null)
            {
                // Determine position
                GeneralTransform transform = relativeTo.TransformToVisual(null).Inverse;
                return transform.Transform(point);
            }
            else
            {
                // Unable to transform; return point as-is
                return point;
            }
        }
    }

    /// <summary>
    /// The event args used in the DragStarted event.
    /// </summary>
    public class DragStartedGestureEventArgs : GestureEventArgs
    {
        internal DragStartedGestureEventArgs(Point gestureOrigin, Orientation direction) 
            : base(gestureOrigin, gestureOrigin)
        {
            Direction = direction;
        }

        /// <summary>
        /// The direction of the drag gesture, as determined by the initial drag change.
        /// </summary>
        public Orientation Direction { get; private set; }
    }

    /// <summary>
    /// The event args used by the DragDelta event.
    /// </summary>
    public class DragDeltaGestureEventArgs : GestureEventArgs
    {
        internal DragDeltaGestureEventArgs(Point gestureOrigin, Point currentPosition, Point change, Orientation direction) 
            : base(gestureOrigin, currentPosition)
        {
            HorizontalChange = change.X;
            VerticalChange = change.Y;
            Direction = direction;
        }

        /// <summary>
        /// The horizontal (X) change for this drag event.
        /// </summary>
        public double HorizontalChange { get; private set; }

        /// <summary>
        /// The vertical (Y) change for this drag event.
        /// </summary>
        public double VerticalChange { get; private set; }

        /// <summary>
        /// The direction of the drag gesture, as determined by the initial drag change.
        /// </summary>
        public Orientation Direction { get; private set; }
    }

    /// <summary>
    /// The event args used by the DragCompleted event.
    /// </summary>
    public class DragCompletedGestureEventArgs : GestureEventArgs
    {
        internal DragCompletedGestureEventArgs(Point gestureOrigin, Point currentPosition, Point change, Orientation direction, Point finalVelocity)
            : base(gestureOrigin, currentPosition)
        {
            HorizontalChange = change.X;
            VerticalChange = change.Y;
            Direction = direction;
            HorizontalVelocity = finalVelocity.X;
            VerticalVelocity = finalVelocity.Y;
        }

        /// <summary>
        /// The total horizontal (X) change of the drag event.
        /// </summary>
        public double HorizontalChange { get; private set; }

        /// <summary>
        /// The total vertical (Y) change of the drag event.
        /// </summary>
        public double VerticalChange { get; private set; }

        /// <summary>
        /// The direction of the drag gesture, as determined by the initial drag change.
        /// </summary>
        public Orientation Direction { get; private set; }

        /// <summary>
        /// The final horizontal (X) velocity of the drag, if the drag was inertial.
        /// </summary>
        public double HorizontalVelocity { get; private set; }

        /// <summary>
        /// The final vertical (Y) velocity of the drag, if the drag was inertial.
        /// </summary>
        public double VerticalVelocity { get; private set; }
    }

    /// <summary>
    /// The event args used by the Flick event.
    /// </summary>
    public class FlickGestureEventArgs : GestureEventArgs
    {
        private Point _velocity;

        internal FlickGestureEventArgs(Point hostOrigin, Point velocity) 
            : base(hostOrigin, hostOrigin)
        {
            _velocity = velocity;
        }

        /// <summary>
        /// The horizontal (X) velocity of the flick.
        /// </summary>
        public double HorizontalVelocity { get { return _velocity.X; } }

        /// <summary>
        /// The vertical (Y) velocity of the flick.
        /// </summary>
        public double VerticalVelocity { get { return _velocity.Y; } }

        /// <summary>
        /// The angle of the flick.
        /// </summary>
        public double Angle
        {
            get { return MathHelpers.GetAngle(_velocity.X, _velocity.Y); }
        }

        /// <summary>
        /// The direction of the flick gesture, as determined by the flick velocities.
        /// </summary>
        public Orientation Direction
        {
            get { return Math.Abs(_velocity.X) >= Math.Abs(_velocity.Y) ? Orientation.Horizontal : Orientation.Vertical; }
        }
    }

    /// <summary>
    /// The base class for multi-touch gesture event args. Currently used only for
    /// two-finger (pinch) operations.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
    public class MultiTouchGestureEventArgs : GestureEventArgs
    {
        /// <summary>
        /// The second touch point's initial position
        /// </summary>
        protected Point GestureOrigin2 { get; private set; }

        /// <summary>
        /// The second touch point. The first is stored in GestureEventArgs.
        /// </summary>
        protected Point TouchPosition2 { get; private set; }

        internal MultiTouchGestureEventArgs(Point gestureOrigin, Point gestureOrigin2, Point position, Point position2)
            : base(gestureOrigin, position)
        {
            GestureOrigin2 = gestureOrigin2;
            TouchPosition2 = position2;
        }

        /// <summary>
        /// Returns the position of either of the two touch points (0 or 1) relative to
        /// the UIElement provided.
        /// </summary>
        /// <param name="relativeTo">The return value will be relative to this element.</param>
        /// <param name="index">The touchpoint to use (0 or 1).</param>
        /// <returns>The gesture's starting point relative to the given UIElement.</returns>
        public Point GetPosition(UIElement relativeTo, int index)
        {
            if (index == 0)
            {
                return GetPosition(relativeTo);
            }
            else if (index == 1)
            {
                return GetPosition(relativeTo, TouchPosition2);
            }
            else
                throw new ArgumentOutOfRangeException("index");
        }
    }

    /// <summary>
    /// The event args used by the PinchStarted event.
    /// </summary>
    public class PinchStartedGestureEventArgs : MultiTouchGestureEventArgs
    {
        internal PinchStartedGestureEventArgs(Point gestureOrigin, Point gestureOrigin2, Point pinch, Point pinch2)
            : base(gestureOrigin, gestureOrigin2, pinch, pinch2)
        {
        }

        /// <summary>
        /// The distance between the two touch points.
        /// </summary>
        public double Distance
        {
            get { return MathHelpers.GetDistance(TouchPosition, TouchPosition2); }
        }

        /// <summary>
        /// The angle defined by the two touch points.
        /// </summary>
        public double Angle
        {
            get { return MathHelpers.GetAngle(TouchPosition2.X - TouchPosition.X, TouchPosition2.Y - TouchPosition.Y); }
        }
    }

    /// <summary>
    /// The event args used by the PinchDelta and PinchCompleted events.
    /// </summary>
    public class PinchGestureEventArgs : MultiTouchGestureEventArgs
    {
        internal PinchGestureEventArgs(Point gestureOrigin, Point gestureOrigin2, Point position, Point position2)
            : base(gestureOrigin, gestureOrigin2, position, position2)
        {
        }

        /// <summary>
        /// Returns the ratio of the current distance between touchpoints / the original distance
        /// between the touchpoints.
        /// </summary>
        public double DistanceRatio
        {
            get
            {
                double originalDistance = Math.Max(MathHelpers.GetDistance(GestureOrigin, GestureOrigin2), 1.0);
                double newDistance = Math.Max(MathHelpers.GetDistance(TouchPosition, TouchPosition2), 1.0);

                return newDistance / originalDistance;
            }
        }

        /// <summary>
        /// Returns the difference in angle between the current touch positions and the original
        /// touch positions.
        /// </summary>
        public double TotalAngleDelta
        {
            get
            {
                double oldAngle = MathHelpers.GetAngle(GestureOrigin2.X - GestureOrigin.X, GestureOrigin2.Y - GestureOrigin.Y);
                double newAngle = MathHelpers.GetAngle(TouchPosition2.X - TouchPosition.X, TouchPosition2.Y - TouchPosition.Y);

                return newAngle - oldAngle;
            }
        }
    }
}
