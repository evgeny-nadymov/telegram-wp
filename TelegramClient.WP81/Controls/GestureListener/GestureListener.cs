// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace TelegramClient.Controls.GestureListener
{
    //public enum GestureType
    //{
    //    None = 0,
    //    Tap = 1,
    //    DoubleTap = 2,
    //    Hold = 4,
    //    HorizontalDrag = 8,
    //    VerticalDrag = 16,
    //    FreeDrag = 32,
    //    PinchDelta = 64,
    //    Flick = 128,
    //    DragCompleted = 256,
    //    PinchCompleted = 512,
    //}

    public partial class GestureListener
    {
        /// <summary>
        /// The GestureBegin event.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.GestureEventArgs> GestureBegin;

        /// <summary>
        /// The GestureCompleted event.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.GestureEventArgs> GestureCompleted;

        /// <summary>
        /// The Tap event (touch, release, no movement).
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.GestureEventArgs> Tap;

        /// <summary>
        /// The DoubleTap event is raised instead of Tap if the time between two taps is short eonugh.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.GestureEventArgs> DoubleTap;

        /// <summary>
        /// The Hold event (touch and hold for one second)
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.GestureEventArgs> Hold;

        /// <summary>
        /// The DragStarted event.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.DragStartedGestureEventArgs> DragStarted;

        /// <summary>
        /// The DragDelta event.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.DragDeltaGestureEventArgs> DragDelta;

        /// <summary>
        /// The DragCompleted event. Will be raised on touch release after a drag, or
        /// when a second touch point is added.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.DragCompletedGestureEventArgs> DragCompleted;

        /// <summary>
        /// The Flick event. Raised when a drag that was fast enough ends with a release.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.FlickGestureEventArgs> Flick;

        /// <summary>
        /// The PinchStarted event.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.PinchStartedGestureEventArgs> PinchStarted;

        /// <summary>
        /// Any two-touch point (two finger) operation.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.PinchGestureEventArgs> PinchDelta;

        /// <summary>
        /// The end of a pinch operation.
        /// </summary>
        public event EventHandler<TelegramClient.Controls.GestureListener.PinchGestureEventArgs> PinchCompleted;
    }
}
