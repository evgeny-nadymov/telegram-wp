// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;

namespace TelegramClient.Controls.GestureListener
{
    /// <summary>
    /// The GestureService class is the helper for getting and setting GestureListeners
    /// on elements.
    /// </summary>
    public static class GestureService
    {
        /// <summary>
        /// Gets a GestureListener for the new element. Will create a new one if necessary.
        /// </summary>
        /// <param name="obj">The object to get the GestureListener from.</param>
        /// <returns>Either the previously existing GestureListener, or a new one.</returns>
        public static TelegramClient.Controls.GestureListener.GestureListener GetGestureListener(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return GetGestureListenerInternal(obj, true);
        }

        /// <summary>
        /// Gets the GestureListener on an element. If one is not set, can create a new one
        /// so that this will never return null, depending on the state of the createIfMissing
        /// flag.
        /// </summary>
        /// <param name="obj">The object to get the GestureListener from.</param>
        /// <param name="createIfMissing">When this is true, if the attached property was not set on the element, it will create one and set it on the element.</param>
        /// <returns></returns>
        internal static TelegramClient.Controls.GestureListener.GestureListener GetGestureListenerInternal(DependencyObject obj, bool createIfMissing)
        {
            TelegramClient.Controls.GestureListener.GestureListener listener = (TelegramClient.Controls.GestureListener.GestureListener)obj.GetValue(GestureListenerProperty);
            if (listener == null && createIfMissing)
            {
                listener = new TelegramClient.Controls.GestureListener.GestureListener();
                SetGestureListenerInternal(obj, listener);
            }
            return listener;
        }

        /// <summary>
        /// Sets the GestureListener on an element. Needed for XAML, but should not be used in code. Use
        /// GetGestureListener instead, which will create a new instance if one is not already set, to 
        /// add your handlers to an element.
        /// </summary>
        /// <param name="obj">The object to set the GestureListener on.</param>
        /// <param name="value">The GestureListener.</param>
        [Obsolete("Do not add handlers using this method. Instead, use GetGestureListener, which will create a new instance if one is not already set, to add your handlers to an element.", true)]
        public static void SetGestureListener(DependencyObject obj, TelegramClient.Controls.GestureListener.GestureListener value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            SetGestureListenerInternal(obj, value);
        }

        /// <summary>
        /// This is used to set the value of the attached DependencyProperty internally.
        /// </summary>
        /// <param name="obj">The object to set the GestureListener on.</param>
        /// <param name="value">The GestureListener.</param>
        private static void SetGestureListenerInternal(DependencyObject obj, TelegramClient.Controls.GestureListener.GestureListener value)
        {
            obj.SetValue(GestureListenerProperty, value);
        }

        /// <summary>
        /// The definition of the GestureListener attached DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty GestureListenerProperty =
            DependencyProperty.RegisterAttached("GestureListener", typeof(TelegramClient.Controls.GestureListener.GestureListener), typeof(GestureService), new PropertyMetadata(null));
    }
}
