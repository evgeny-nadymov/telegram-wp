// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Data;

namespace Telegram.Controls.Helpers
{
    /// <summary>
    /// This class implements a listener to receive notifications for dependency property changes.
    /// </summary>
    public class DependencyPropertyChangedListener
    {

        #region Inner types

        // Helper element to make it possible to use the binding engine to get notified when the source element property changes.
        private sealed class RelayObject : DependencyObject
        {
            private DependencyPropertyChangedListener _listener;

            internal RelayObject(DependencyPropertyChangedListener listener)
            {
                _listener = listener;
            }

            #region Value (DependencyProperty)

            public object Value
            {
                get { return (object)GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }
            public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(RelayObject), new PropertyMetadata(default(object), new PropertyChangedCallback(OnValueChanged)));

            private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                object oldValue = (object)e.OldValue;
                object newValue = (object)e.NewValue;
                RelayObject source = (RelayObject)d;
                source.OnValueChanged(oldValue, newValue);
            }

            private void OnValueChanged(object oldValue, object newValue)
            {
                _listener.OnValueChanged(oldValue, newValue);
            }

            #endregion
        }

        #endregion

        #region Events

        /// <summary>
        /// Raises when the dependency property changes.
        /// </summary>
        public event EventHandler<DependencyPropertyValueChangedEventArgs> ValueChanged;

        #endregion

        #region Ctor

        private DependencyPropertyChangedListener()
        {
            // just to make it private
        }

        #endregion

        // holds a reference to the relay object in order that the GC does not collect it
        private RelayObject RelayInstance { get; set; }

        public static DependencyPropertyChangedListener Create(DependencyObject sourceElement, string propertyPath)
        //public static DependencyPropertyChangedListener Create(DependencyObject sourceElement, DependencyProperty property)
        {
            // check input
            if (sourceElement == null)
                throw new ArgumentNullException("sourceElement");
            if (string.IsNullOrWhiteSpace(propertyPath))
                throw new ArgumentException("propertyPath is empty");

            // create listener
            DependencyPropertyChangedListener listener = new DependencyPropertyChangedListener();

            // setup binding
            Binding binding = new Binding();
            binding.Source = sourceElement;
            binding.Mode = BindingMode.OneWay;
            //binding.Path = new PropertyPath(property); // throws exception
            binding.Path = new PropertyPath(propertyPath);

            // create relay object
            RelayObject relay = new RelayObject(listener);
            // ...the listener holds a reference to the relay object in order that the GC does not collect it
            listener.RelayInstance = relay;

            // set binding
            BindingOperations.SetBinding(relay, RelayObject.ValueProperty, binding);

            return listener;
        }

        public void Detach()
        {
            if (this.RelayInstance != null)
            {
                // first: reset member to prevent further eventing of ValueChanged event.
                RelayObject temp = this.RelayInstance;
                this.RelayInstance = null;

                // second: clear the binding -> raises property changed event...
                temp.ClearValue(RelayObject.ValueProperty);
            }
        }

        private void OnValueChanged(object oldValue, object newValue)
        {
            // raise event, but only if the listener is not detached.
            if (ValueChanged != null && this.RelayInstance != null)
                ValueChanged(this, new DependencyPropertyValueChangedEventArgs(oldValue, newValue));
        }

    }
}
