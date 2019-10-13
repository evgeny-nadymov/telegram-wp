// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Telegram.Api.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Models
{
    public class Settings : TelegramPropertyChangedBase
    {
        public Settings()
        {
            IsNotifying = false;
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(propertyName);
            return true;
        }

        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(selectorExpression);
            return true;
        }

        #region Message Notifications

        public bool _contactAlert = true;

        public bool ContactAlert
        {
            get { return _contactAlert; }
            set { SetField(ref _contactAlert, value, () => ContactAlert); }
        }

        public bool _contactMessagePreview = true;

        public bool ContactMessagePreview
        {
            get { return _contactMessagePreview; }
            set { SetField(ref _contactMessagePreview, value, () => ContactMessagePreview); }
        }

        public string _contactSound;

        public string ContactSound
        {
            get { return _contactSound; }
            set { SetField(ref _contactSound, value, () => ContactSound); }
        }

        #endregion

        #region Group Notifications

        public bool _groupAlert = true;

        public bool GroupAlert
        {
            get { return _groupAlert; }
            set { SetField(ref _groupAlert, value, () => GroupAlert); }
        }

        public bool _groupMessagePreview = true;

        public bool GroupMessagePreview
        {
            get { return _groupMessagePreview; }
            set { SetField(ref _groupMessagePreview, value, () => GroupMessagePreview); }
        }

        public string _groupSound;

        public string GroupSound
        {
            get { return _groupSound; }
            set { SetField(ref _groupSound, value, () => GroupSound); }
        }

        #endregion

        #region In-App Notifications

        public bool _inAppVibration = true;

        public bool InAppVibration
        {
            get { return _inAppVibration; }
            set { SetField(ref _inAppVibration, value, () => InAppVibration); }
        }

        public bool _inAppSound = true;

        public bool InAppSound
        {
            get { return _inAppSound; }
            set { SetField(ref _inAppSound, value, () => InAppSound); }
        }

        public bool _inAppMessagePreview = true;

        public bool InAppMessagePreview
        {
            get { return _inAppMessagePreview; }
            set { SetField(ref _inAppMessagePreview, value, () => InAppMessagePreview); }
        }

        #endregion

        public bool _locationServices = false;

        public bool LocationServices
        {
            get { return _locationServices; }
            set { SetField(ref _locationServices, value, () => LocationServices); }
        }

        public bool _peopleHub = false;

        public bool PeopleHub
        {
            get { return _peopleHub; }
            set { SetField(ref _peopleHub, value, () => PeopleHub); }
        }

        public bool AskAllowingLocationServices = false;

        public bool _saveIncomingPhotos = false;

        public bool SaveIncomingPhotos
        {
            get { return _saveIncomingPhotos; }
            set { SetField(ref _saveIncomingPhotos, value, () => SaveIncomingPhotos); }
        }

        public BackgroundItem _background;

        public BackgroundItem Background
        {
            get { return _background; }
            set { SetField(ref _background, value, () => Background); }
        }

        public bool SendByEnter { get; set; }

        public bool InvisibleMode { get; set; }

        public bool _contactJoined = true;

        public bool ContactJoined
        {
            get { return _contactJoined; }
            set { SetField(ref _contactJoined, value, () => ContactJoined); }
        }
    }
}
