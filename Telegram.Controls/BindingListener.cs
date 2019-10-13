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
using System.Windows.Data;

namespace DanielVaughan.WindowsPhone7Unleashed
{
	public class BindingListener
	{
		public delegate void ChangedHandler(object sender, BindingChangedEventArgs e);

		static readonly List<DependencyPropertyListener> freeListeners = new List<DependencyPropertyListener>();

		readonly ChangedHandler changedHandler;
		Binding binding;
		DependencyPropertyListener listener;
		FrameworkElement target;
		object value;

		public BindingListener(ChangedHandler changedHandler)
		{
			this.changedHandler = changedHandler;
		}

		public Binding Binding
		{
			get
			{
				return binding;
			}
			set
			{
				binding = value;
				Attach();
			}
		}

		public FrameworkElement Element
		{
			get
			{
				return target;
			}
			set
			{
				target = value;
				Attach();
			}
		}

		public object Value
		{
			get
			{
				return value;
			}
		}

		void Attach()
		{
			Detach();

			if (target != null && binding != null)
			{
				listener = GetListener();
				listener.Attach(target, binding);
			}
		}

		void Detach()
		{
			if (listener != null)
			{
                this.listener.Detach(); // This is not called in the original samples            
				ReturnListener();
			}
		}

		DependencyPropertyListener GetListener()
		{
			DependencyPropertyListener listener;

			if (freeListeners.Count != 0)
			{
				listener = freeListeners[freeListeners.Count - 1];
				freeListeners.RemoveAt(freeListeners.Count - 1);

				//return listener; //Memory leak here
			}
			listener = new DependencyPropertyListener();

			listener.Changed += HandleValueChanged;

			return listener;
		}

		void ReturnListener()
		{
			listener.Changed -= HandleValueChanged;

			freeListeners.Add(listener);

			listener = null;
		}

		void HandleValueChanged(object sender, BindingChangedEventArgs e)
		{
			value = e.EventArgs.NewValue;

			changedHandler(this, e);
		}
	}

	public class DependencyPropertyListener
	{
		static int index;
		readonly DependencyProperty property;
		FrameworkElement target;

		public DependencyPropertyListener()
		{
			property = DependencyProperty.RegisterAttached(
				"DependencyPropertyListener" + index++,
				typeof(object),
				typeof(DependencyPropertyListener),
				new PropertyMetadata(null, HandleValueChanged));
		}

		public event EventHandler<BindingChangedEventArgs> Changed;

		void HandleValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			OnChanged(new BindingChangedEventArgs(e));
		}

		protected void OnChanged(BindingChangedEventArgs e)
		{
			var temp = Changed;
			if (temp != null)
			{
				temp(target, e);
			}
		}

		public void Attach(FrameworkElement element, Binding binding)
		{
			if (target != null)
			{
				throw new Exception(
					"Cannot attach an already attached listener");
			}

			target = element;
			target.SetBinding(property, binding);
		}

		public void Detach()
		{
		    if (target == null) return;

			target.ClearValue(property);
			target = null;
		}
	}

	public class BindingChangedEventArgs : EventArgs
	{
		public BindingChangedEventArgs(DependencyPropertyChangedEventArgs e)
		{
			EventArgs = e;
		}

		public DependencyPropertyChangedEventArgs EventArgs { get; private set; }
	}
}