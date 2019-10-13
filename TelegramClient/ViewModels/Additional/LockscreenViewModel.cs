// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Controls;
using TelegramClient.Helpers;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class LockscreenViewModel : TelegramPropertyChangedBase
    {
        public string EmptyDialogImageSource
        {
            get
            {
                if (StateService.CurrentBackground != null)
                {
                    return "/Images/LockScreen/lockscreen.logo.png";
                }

                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/LockScreen/lockscreen.logo.white.png";
                }

                return "/Images/LockScreen/lockscreen.logo.png";
            }
        }

        public double LogoOpacity
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return 1.0;
                }

                if (StateService.CurrentBackground == null)
                {
                    return 0.5;
                }

                return 1.0;
            }
        }

        private string _passcode;

        public string Passcode
        {
            get { return _passcode; }
            set
            {
                if (_passcode != value)
                {
                    _passcode = value;
                    NotifyOfPropertyChange(() => Passcode);
                }
            }
        }

        public IStateService StateService
        {
            get { return IoC.Get<IStateService>(); }
        }

        public bool Simple
        {
            get { return IsSimple(StateService); }
        }

        public static bool IsSimple(IStateService stateService)
        {
            return PasscodeUtils.IsSimple;
        }

        public LockscreenViewModel()
        {
            PropertyChanged += LockscreenViewModel_PropertyChanged;
        }

        void LockscreenViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => Passcode))
            {
                if (Simple && Passcode.Length == 4)
                {
                    Done();
                }
            }
        }

        public event EventHandler PasscodeIncorrect;

        protected virtual void RaisePasscodeIncorrect()
        {
            var handler = PasscodeIncorrect;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }


        public void Done()
        {
            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
            {
                if (!PasscodeUtils.Check(Passcode))
                {
                    Passcode = string.Empty;
                    RaisePasscodeIncorrect();

                    return;
                }

                var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                if (frame != null)
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        frame.CloseLockscreen();
                        PasscodeUtils.Unlock();     // will not invoke here ShellViewModel.UpdateDeviceLockedAsync()
                        var shellViewModel = IoC.Get<ShellViewModel>();
                        if (shellViewModel != null)
                        {
                            shellViewModel.NotifyOfPropertyChange(() => shellViewModel.PasscodeImageBrush);
                            shellViewModel.NotifyOfPropertyChange(() => shellViewModel.PasscodeImageSource);
                            shellViewModel.UpdateDeviceLockedAsync();
                        }

                        Passcode = string.Empty;
                    });
                }
            });
        }
    }

    public static class PasscodeUtils
    {
        private static readonly object PasscodeParamsFileSyncRoot = new object();

        private static TLPasscodeParams _cachedParams;

        private static bool _readOnce;

        public static TLPasscodeParams GetParams()
        {
            if (_cachedParams != null)
            {
                return _cachedParams;
            }

            if (!_readOnce)
            {
                _readOnce = true; 
                _cachedParams = TLUtils.OpenObjectFromMTProtoFile<TLPasscodeParams>(PasscodeParamsFileSyncRoot, Constants.PasscodeParamsFileName);
            }

            return _cachedParams;
        }

        public static bool IsEnabled
        {
            get
            {
                var passcodeParams = GetParams();
                if (passcodeParams != null)
                {
                    return !TLString.IsNullOrEmpty(passcodeParams.Hash);
                }

                var stateService = IoC.Get<IStateService>();
                return !string.IsNullOrEmpty(stateService.Passcode);
            }
        }

        public static bool IsSimple
        {
            get
            {
                var passcodeParams = GetParams();
                if (passcodeParams != null)
                {
                    return passcodeParams.IsSimple.Value;
                }

                var stateService = IoC.Get<IStateService>();
                return stateService.IsSimplePasscode;
            }
        }

        public static bool Locked
        {
            get
            {
                var passcodeParams = GetParams();
                if (passcodeParams != null)
                {
                    return passcodeParams.Locked.Value;
                }

                var stateService = IoC.Get<IStateService>();
                return stateService.Locked;
            }
        }

        public static DateTime CloseTime
        {
            get
            {
                var passcodeParams = GetParams();
                if (passcodeParams != null)
                {
                    return TLUtils.ToDateTime(passcodeParams.CloseTime);
                }

                var stateService = IoC.Get<IStateService>();
                return stateService.CloseTime;
            }
            set
            {
                var passcodeParams = GetParams();
                if (passcodeParams != null)
                {
                    passcodeParams.CloseTime = TLUtils.ToTLInt(value);
                    Save();
                }
            }
        }

        public static int AutolockTimeout
        {
            get
            {
                var passcodeParams = GetParams();
                if (passcodeParams != null)
                {
                    return passcodeParams.AutolockTimeout.Value;
                }

                var stateService = IoC.Get<IStateService>();
                return stateService.AutolockTimeout;
            }
            set
            {
                var passcodeParams = GetParams();
                if (passcodeParams != null)
                {
                    passcodeParams.AutolockTimeout = new TLInt(value);
                    Save();
                }
            }
        }

        public static bool IsLockscreenRequired
        {
            get
            {
                return IsEnabled
                    && (DateTime.Now > CloseTime.AddSeconds(AutolockTimeout) || Locked);
            }
        }

        public static void Lock()
        {
            var passcodeParams = GetParams();
            if (passcodeParams != null)
            {
                passcodeParams.Locked = TLBool.True;
                Save();
            }
        }

        public static void Unlock()
        {
            var passcodeParams = GetParams();
            if (passcodeParams != null)
            {
                passcodeParams.Locked = TLBool.False;
                Save();
            }
        }

        public static void ChangeLocked()
        {
            var passcodeParams = GetParams();
            if (passcodeParams != null)
            {
                if (passcodeParams.Locked.Value)
                {
                    Unlock();
                }
                else
                {
                    Lock();
                }
            }
        }

        private static void Save()
        {
            if (_cachedParams != null)
            {
                TLUtils.SaveObjectToMTProtoFile(PasscodeParamsFileSyncRoot, Constants.PasscodeParamsFileName, _cachedParams);
            }
        }

        public static void SetParams(string passcode, bool isSimple, int autolockTimeout)
        {
            var salt = new byte[256];
            var secureRandom = new RNGCryptoServiceProvider();
            secureRandom.GetBytes(salt);
            var hash = Telegram.Api.Helpers.Utils.ComputeSHA1(TLUtils.Combine(salt, new TLString(passcode).Data, salt));
            var passcodeParams = new TLPasscodeParams
            {
                Hash = TLString.FromBigEndianData(hash),
                Salt = TLString.FromBigEndianData(salt),
                IsSimple = new TLBool(isSimple),
                AutolockTimeout = new TLInt(autolockTimeout),
                CloseTime = new TLInt(0),
                Locked = TLBool.False
            };

            _cachedParams = passcodeParams;
            Save();
        }

        public static bool CheckSimple(string passcode)
        {
            return passcode != null && passcode.Length == 4 && passcode.All(x => x >= '0' && x <= '9');
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static byte[] ComputeHash(TLString salt, TLString passcode)
        {
            var bytes = TLUtils.Combine(salt.Data, passcode.Data, salt.Data);

            for (var i = 0; i < Constants.PasscodeHashIterations; i++)
            {
                var tempBytes = TLUtils.Combine(BitConverter.GetBytes(i), bytes);
                Telegram.Api.Helpers.Utils.ComputeSHA1(tempBytes);
            }

            return Telegram.Api.Helpers.Utils.ComputeSHA1(bytes);
        }

        public static bool Check(string passcode)
        {
            var passcodeParams = GetParams();
            if (passcodeParams == null)
            {
                var stateService = IoC.Get<IStateService>();
                var result = String.Equals(stateService.Passcode, passcode);

                if (result)
                {
                    SetParams(stateService.Passcode, stateService.IsSimplePasscode, stateService.AutolockTimeout);
                    RemovePasscodeKeys();
                }

                return result;
            }

            var computedHash = ComputeHash(passcodeParams.Salt, new TLString(passcode));

            return TLUtils.ByteArraysEqual(computedHash, passcodeParams.Hash.Data);
        }

        public static void Reset()
        {
            FileUtils.Delete(PasscodeParamsFileSyncRoot, Constants.PasscodeParamsFileName);
            _cachedParams = null;

            RemovePasscodeKeys();
        }

        private static void RemovePasscodeKeys()
        {
            SettingsHelper.CrossThreadAccess(
                settings =>
                {
                    settings.Remove(Constants.PasscodeKey);
                    settings.Remove(Constants.IsSimplePasscodeKey);
                    settings.Remove(Constants.IsPasscodeEnabledKey);
                    settings.Remove(Constants.AppCloseTimeKey);
                    settings.Remove(Constants.PasscodeAutolockKey);
                });
        }
    }
}
