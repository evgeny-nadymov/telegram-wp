// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using TelegramClient.Controls.StartView;
using DragEventArgs = TelegramClient.Controls.StartView.DragEventArgs;

internal abstract class GestureHelper
{
    private readonly Size DeadZoneInPixels = new Size(12, 12);

    private DragLock _dragLock;
    private bool _dragging;
    private WeakReference _gestureSource;
    private Point _gestureOrigin;

    protected GestureHelper(UIElement target, bool shouldHandleAllDrags)
    {
        Target = target;
        ShouldHandleAllDrags = shouldHandleAllDrags;
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    protected bool ShouldHandleAllDrags { get; private set; }

    protected UIElement Target { get; private set; }

    public event EventHandler<GestureEventArgs> GestureStart;

    public event EventHandler<FlickEventArgs> Flick;

    public event EventHandler<EventArgs> GestureEnd;

    public event EventHandler<DragEventArgs> HorizontalDrag;

    public event EventHandler<DragEventArgs> VerticalDrag;

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public static GestureHelper Create(UIElement target)
    {
        return GestureHelper.Create(target, true);
    }

    public static GestureHelper Create(UIElement target, bool shouldHandleAllDrags)
    {
        GestureHelper gestureHelper = new ManipulationGestureHelper(target, shouldHandleAllDrags);
        gestureHelper.Start();
        return gestureHelper;
    }

    protected abstract void HookEvents();

    public void Start()
    {
        HookEvents();
    }

    protected void NotifyDown(InputBaseArgs args)
    {
        GestureEventArgs e = new GestureEventArgs();
        _gestureSource = new WeakReference(args.Source);
        _gestureOrigin = args.Origin;
        _dragLock = DragLock.Unset;
        _dragging = false;
        RaiseGestureStart(e);
    }

    protected void NotifyMove(InputDeltaArgs args)
    {
        if (Math.Abs(args.CumulativeTranslation.X) > DeadZoneInPixels.Width || Math.Abs(args.CumulativeTranslation.Y) > DeadZoneInPixels.Height)
        {
            if (!_dragging)
            {
                ReleaseMouseCaptureAtGestureOrigin();
            }

            _dragging = true;

            if (_dragLock == DragLock.Unset)
            {
                double angle = GestureHelper.AngleFromVector(args.CumulativeTranslation.X, args.CumulativeTranslation.Y) % 180;
                _dragLock = angle <= 45 || angle >= 135 ? DragLock.Horizontal : DragLock.Vertical;
            }
        }

        if (_dragging)
        {
            RaiseDragEvents(args);
        }
    }

    private void ReleaseMouseCaptureAtGestureOrigin()
    {
        if (_gestureSource != null)
        {
            FrameworkElement gestureSource = _gestureSource.Target as FrameworkElement;
            if (gestureSource != null)
            {
                foreach (UIElement element in VisualTreeHelper.FindElementsInHostCoordinates(
                        gestureSource.TransformToVisual(null).Transform(_gestureOrigin), Application.Current.RootVisual))
                {
                    element.ReleaseMouseCapture();
                }
            }
        }
    }

    protected void NotifyUp(InputCompletedArgs args)
    {
        EventArgs e = EventArgs.Empty;
        _dragLock = DragLock.Unset;
        _dragging = false;

        if (args.IsInertial)
        {
            double angle = GestureHelper.AngleFromVector(args.FinalLinearVelocity.X, args.FinalLinearVelocity.Y);
            if (angle <= 45 || angle >= 315)
            {
                angle = 0;
            }
            else if (angle >= 135 && angle <= 225)
            {
                angle = 180;
            }

            FlickEventArgs flickEventArgs = new FlickEventArgs
            {
                Angle = angle
            };
            ReleaseMouseCaptureAtGestureOrigin();
            RaiseFlick(flickEventArgs);
        }
        else if (args.TotalTranslation.X != 0 || args.TotalTranslation.Y != 0)
        {
            DragEventArgs dragEventArgs = new DragEventArgs
            {
                CumulativeDistance = args.TotalTranslation
            };
            dragEventArgs.MarkAsFinalTouchManipulation();
            e = dragEventArgs;
        }

        RaiseGestureEnd(e);
    }

    private void RaiseGestureStart(GestureEventArgs args)
    {
        SafeRaise.Raise<GestureEventArgs>(GestureStart, this, args);
    }

    private void RaiseFlick(FlickEventArgs args)
    {
        SafeRaise.Raise<FlickEventArgs>(Flick, this, args);
    }

    private void RaiseGestureEnd(EventArgs args)
    {
        SafeRaise.Raise<EventArgs>(GestureEnd, this, args);
    }

    private void RaiseDragEvents(InputDeltaArgs args)
    {
        DragEventArgs e = new DragEventArgs(args);
        if (args.DeltaTranslation.X != 0 && _dragLock == DragLock.Horizontal)
        {
            RaiseHorizontalDrag(e);
        }
        else if (args.DeltaTranslation.Y != 0 && _dragLock == DragLock.Vertical)
        {
            RaiseVerticalDrag(e);
        }
    }

    private void RaiseHorizontalDrag(DragEventArgs args)
    {
        SafeRaise.Raise<DragEventArgs>(HorizontalDrag, this, args);
    }

    private void RaiseVerticalDrag(DragEventArgs args)
    {
        SafeRaise.Raise<DragEventArgs>(VerticalDrag, this, args);
    }

    private static double AngleFromVector(double x, double y)
    {
        double num = Math.Atan2(y, x);
        if (num < 0)
        {
            num = 2 * Math.PI + num;
        }
        return num * 360 / (2 * Math.PI);
    }

    private enum DragLock
    {
        Unset,
        Free,
        Vertical,
        Horizontal,
    }
}
