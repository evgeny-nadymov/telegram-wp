// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace TelegramClient.Helpers
{
    internal class WeakEventListener<TInstance, TSource, TEventArgs> where TInstance : class where TSource : class
    {
        private readonly WeakReference _instance;

        private readonly WeakReference _source;

        private Action<TInstance, TSource, TEventArgs> _onEventAction;

        public Action<TInstance, TSource, TEventArgs> OnEventAction
        {
            get { return _onEventAction; }
            set
            {
                if (value != null && !value.Method.IsStatic)
                {
                    throw new ArgumentException("OnEventAction method must be static otherwise the event WeakEventListner class does not prevent memory leaks.");
                }

                _onEventAction = value;
            }
        }

        private Action<WeakEventListener<TInstance, TSource, TEventArgs>, TSource> _onDetachAction;

        public Action<WeakEventListener<TInstance, TSource, TEventArgs>, TSource> OnDetachAction
        {
            get { return _onDetachAction; }
            set
            {
                if (value != null && !value.Method.IsStatic)
                {
                    throw new ArgumentException("OnDetachAction method must be static otherwise the event WeakEventListner class does not prevent memory leaks.");
                }

                _onDetachAction = value;
            }
        }

        public WeakEventListener(TInstance instance, TSource source)
        {
            _instance = new WeakReference(instance);
            _source = new WeakReference(source);
        }

        public void OnEvent(TSource source, TEventArgs args)
        {
            var instance = _instance.Target;
            if (instance != null)
            {
                if (OnEventAction != null) OnEventAction((TInstance)instance, source, args);
            }
            else
            {
                Detach();
            }
        }

        public void Detach()
        {
            var source = _source.Target as TSource;
            if (source != null && OnDetachAction != null)
            {
                OnDetachAction(this, source);
                OnDetachAction = null;
            }
        }
    }
}
