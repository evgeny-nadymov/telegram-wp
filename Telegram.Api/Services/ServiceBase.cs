// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Telegram.Api.Helpers;

namespace Telegram.Api.Services
{
    [DataContract]
    public abstract class TelegramPropertyChangedBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Enables/Disables property change notification.
        /// 
        /// </summary>
        public bool IsNotifying { get; set; }
        
        private static bool _isNotifyingGlobal = true;

        public static bool IsNotifyingGlobal
        {
            get { return _isNotifyingGlobal; }
            set { _isNotifyingGlobal = false; }
        }

        private static bool _logNotify = false;

        public static bool LogNotify
        {
            get { return _logNotify; }
            set { _logNotify = value; }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (param0, param1) => { };


        public TelegramPropertyChangedBase()
        {
            IsNotifying = true;
        }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// 
        /// </summary>
        public void Refresh()
        {
            NotifyOfPropertyChange(String.Empty);
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// 
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public virtual void NotifyOfPropertyChange(string propertyName)
        {
            if (!IsNotifyingGlobal)
                return;

            if (!IsNotifying)
                return;

#if DEBUG
            if (LogNotify)
            {
                Debug.WriteLine("Notify " + propertyName + " " + GetType());
            }
#endif
            if (Execute.CheckAccess())
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Execute.BeginOnUIThread(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));
            }
            //Execute.OnUIThread(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// 
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam><param name="property">The property expression.</param>
        public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            this.NotifyOfPropertyChange(GetMemberInfo(property).Name);
        }

        public static MemberInfo GetMemberInfo(Expression expression)
        {
            var lambdaExpression = (LambdaExpression)expression;
            return (!(lambdaExpression.Body is UnaryExpression) ? (MemberExpression)lambdaExpression.Body : (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand).Member;
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event directly.
        /// 
        /// </summary>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler changedEventHandler = PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler(this, e);
        }

        /// <summary>
        /// Called when the object is deserialized.
        /// 
        /// </summary>
        /// <param name="c">The streaming context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext c)
        {
            IsNotifying = true;
        }

        /// <summary>
        /// Used to indicate whether or not the IsNotifying property is serialized to Xml.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// Whether or not to serialize the IsNotifying property. The default is false.
        /// </returns>
        public virtual bool ShouldSerializeIsNotifying()
        {
            return false;
        }
    }

    public abstract class ServiceBase : TelegramPropertyChangedBase
    {
    }
}
