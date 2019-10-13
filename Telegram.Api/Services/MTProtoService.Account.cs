// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Threading;
#if WIN_RT
using Windows.Data.Xml.Dom;
#if WNS_PUSH_SERVICE
using Windows.UI.Notifications;
#endif
#endif
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using Telegram.Api.TL.Account;
using Telegram.Api.TL.Functions.Account;
using Telegram.Api.TL.Functions.Help;
using TLUpdateUserName = Telegram.Api.TL.Account.TLUpdateUserName;

namespace Telegram.Api.Services
{
	public partial class MTProtoService
	{
	    public event EventHandler CheckDeviceLocked;

	    protected virtual void RaiseCheckDeviceLocked()
	    {
	        var handler = CheckDeviceLocked;
	        if (handler != null) handler(this, EventArgs.Empty);
	    }

	    private void CheckDeviceLockedInternal(object state)
        {
            RaiseCheckDeviceLocked();
        }

        public void ReportPeerAsync(TLInputPeerBase peer, TLInputReportReasonBase reason, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLReportPeer { Peer = peer, Reason = reason };

            SendInformativeMessage("account.reportPeer", obj, callback, faultCallback);
        }

	    public void DeleteAccountAsync(TLString reason, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLDeleteAccount { Reason = reason };

            SendInformativeMessage("account.deleteAccount", obj, callback, faultCallback);
	    }

        public void UpdateDeviceLockedAsync(TLInt period, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLUpdateDeviceLocked{ Period = period };

            SendInformativeMessage("account.updateDeviceLocked", obj, callback, faultCallback);
        }

	    public void GetWallpapersAsync(Action<TLVector<TLWallPaperBase>> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLGetWallPapers();

            SendInformativeMessage("account.getWallpapers", obj, callback, faultCallback);
	    }

        public void SendChangePhoneCodeAsync(TLString phoneNumber, TLString currentNumber, Action<TLSentCodeBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSendChangePhoneCode { Flags = new TLInt(0), PhoneNumber = phoneNumber, CurrentNumber = currentNumber };

            SendInformativeMessage("account.sendChangePhoneCode", obj, callback, faultCallback);
        }

        public void ChangePhoneAsync(TLString phoneNumber, TLString phoneCodeHash, TLString phoneCode, Action<TLUserBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLChangePhone { PhoneNumber = phoneNumber, PhoneCodeHash = phoneCodeHash, PhoneCode = phoneCode };

            SendInformativeMessage<TLUserBase>("account.changePhone", obj, user => _cacheService.SyncUser(user, callback.SafeInvoke), faultCallback);
        }

        public void RegisterDeviceAsync(TLInt tokenType, TLString token, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            if (_activeTransport.AuthKey == null)
            {
                faultCallback.SafeInvoke(new TLRPCError
                {
                    Code = new TLInt(404),
                    Message = new TLString("Service is not initialized to register device")
                });

                return;
            }

            var obj = new TLRegisterDevice
            {
                //TokenType = new TLInt(3),   // MPNS
                //TokenType = new TLInt(8),   // WNS
                //TokenType = new TLInt(11),  // MPNS raw
                TokenType = tokenType,
                Token = token,
                AppSandbox = TLBool.False,
                Secret = TLString.Empty,
                OtherUids = new TLVector<TLInt>()
            };

            const string methodName = "account.registerDevice";
            Logs.Log.Write(string.Format("{0} {1}", methodName, obj));
            SendInformativeMessage<TLBool>(methodName, obj,
                result =>
                {
                    Logs.Log.Write(string.Format("{0} result={1}", methodName, result));

                    callback.SafeInvoke(result);
                },
                error =>
                {
                    Logs.Log.Write(string.Format("{0} error={1}", methodName, error));

                    faultCallback.SafeInvoke(error);
                });
        }

        public void UnregisterDeviceAsync(TLInt tokenType, TLString token, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLUnregisterDevice
            {
                //TokenType = new TLInt(3),   // MPNS
                //TokenType = new TLInt(8),   // WNS
                TokenType = tokenType,
                Token = token
            };

            const string methodName = "account.unregisterDevice";
            Logs.Log.Write(string.Format("{0} {1}", methodName, obj));
            SendInformativeMessage<TLBool>("account.unregisterDevice", obj,
                result =>
                {
                    Logs.Log.Write(string.Format("{0} result={1}", methodName, result));

                    callback.SafeInvoke(result);
                },
                error =>
                {
                    Logs.Log.Write(string.Format("{0} error={1}", methodName, error));

                    faultCallback.SafeInvoke(error);
                });
        }

        public void GetNotifySettingsAsync(TLInputNotifyPeerBase peer, Action<TLPeerNotifySettingsBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetNotifySettings{ Peer = peer };

            SendInformativeMessage("account.getNotifySettings", obj, callback, faultCallback);
        }

        public void ResetNotifySettingsAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            Execute.ShowDebugMessage(string.Format("account.resetNotifySettings"));

            var obj = new TLResetNotifySettings();

            SendInformativeMessage("account.resetNotifySettings", obj, callback, faultCallback);
        }

	    public void UpdateNotifySettingsAsync(TLInputNotifyPeerBase peer, TLInputPeerNotifySettings settings, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            //Execute.ShowDebugMessage(string.Format("account.updateNotifySettings peer=[{0}] settings=[{1}]", peer, settings));

            var obj = new TL.Functions.Account.TLUpdateNotifySettings { Peer = peer, Settings = settings };

            SendInformativeMessage("account.updateNotifySettings", obj, callback, faultCallback);
        }

        public void UpdateProfileAsync(TLString firstName, TLString lastName, TLString about, Action<TLUserBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLUpdateProfile { FirstName = firstName, LastName = lastName, About = about };

            SendInformativeMessage<TLUserBase>("account.updateProfile", obj, result => _cacheService.SyncUser(result, callback), faultCallback);
        }

        public void UpdateStatusAsync(TLBool offline, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            if (_activeTransport.AuthKey == null) return;

#if WIN_RT
            if (_deviceInfo != null && _deviceInfo.IsBackground)
            {
                var message = string.Format("::{0} {1} account.updateStatus {2}", _deviceInfo.BackgroundTaskName, _deviceInfo.BackgroundTaskId, offline);
                Logs.Log.Write(message);
#if DEBUG && WNS_PUSH_SERVICE
                AddToast("task", message);
#endif
            }
#endif
            
            TLObject obj = null;
            if (_deviceInfo != null && _deviceInfo.IsBackground)
            {
                obj = new TLInvokeWithoutUpdates {Object = new TLUpdateStatus {Offline = offline}};
            }
            else
            {
                obj = new TLUpdateStatus { Offline = offline };
            }

            System.Diagnostics.Debug.WriteLine("account.updateStatus offline=" + offline.Value);
            SendInformativeMessage("account.updateStatus", obj, callback, faultCallback);
        }
#if WIN_RT && WNS_PUSH_SERVICE
        public static void AddToast(string caption, string message)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            SetText(toastXml, caption, message);

            try
            {
                var toast = new ToastNotification(toastXml);
                //RemoveToastGroup(group);
                toastNotifier.Show(toast);
            }
            catch (Exception ex)
            {
                Logs.Log.Write(ex.ToString());
            }
        }

        private static void SetText(XmlDocument document, string caption, string message)
        {
            var toastTextElements = document.GetElementsByTagName("text");
            toastTextElements[0].InnerText = caption ?? string.Empty;
            toastTextElements[1].InnerText = message ?? string.Empty;
        }
#endif

	    public void CheckUsernameAsync(TLString username, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLCheckUsername { Username = username };

            SendInformativeMessage("account.checkUsername", obj, callback, faultCallback);
	    }

	    public void UpdateUsernameAsync(TLString username, Action<TLUserBase> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLUpdateUserName { Username = username };

            SendInformativeMessage("account.updateUsername", obj, callback, faultCallback);
	    }

	    public void GetAccountTTLAsync(Action<TLAccountDaysTTL> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLGetAccountTTL();

            SendInformativeMessage("account.getAccountTTL", obj, callback, faultCallback);
	    }

        public void SetAccountTTLAsync(TLAccountDaysTTL ttl, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSetAccountTTL{TTL = ttl};

            SendInformativeMessage("account.setAccountTTL", obj, callback, faultCallback);
        }

        public void DeleteAccountTTLAsync(TLString reason, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLDeleteAccount { Reason = reason };

            SendInformativeMessage("account.deleteAccount", obj, callback, faultCallback);
        }

        public void GetPrivacyAsync(TLInputPrivacyKeyBase key, Action<TLPrivacyRules> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetPrivacy { Key = key };

            SendInformativeMessage("account.getPrivacy", obj, callback, faultCallback);
        }

        public void SetPrivacyAsync(TLInputPrivacyKeyBase key, TLVector<TLInputPrivacyRuleBase> rules, Action<TLPrivacyRules> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSetPrivacy { Key = key, Rules = rules };

            SendInformativeMessage("account.setPrivacy", obj, callback, faultCallback);
        }

        public void GetAuthorizationsAsync(Action<TLAccountAuthorizations> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetAuthorizations();

            SendInformativeMessage("account.getAuthorizations", obj, callback, faultCallback);
        }

        public void ResetAuthorizationAsync(TLLong hash, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLResetAuthorization { Hash = hash };

            SendInformativeMessage("account.resetAuthorization", obj, callback, faultCallback);
        }

	    public void GetPasswordAsync(Action<TLPasswordBase> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLGetPassword();

            SendInformativeMessage("account.getPassword", obj, callback, faultCallback);
	    }

        public void GetTmpPasswordAsync(TLInputCheckPasswordBase password, TLInt period, Action<TLTmpPassword> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetTmpPassword{ Password = password, Period = period };

            SendInformativeMessage("account.getTmpPassword", obj, callback, faultCallback);
        }

        public void GetPasswordSettingsAsync(TLInputCheckPasswordBase password, Action<TLPasswordSettings> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetPasswordSettings { Password = password };

            SendInformativeMessage("account.getPasswordSettings", obj, callback, faultCallback);
        }

        public void UpdatePasswordSettingsAsync(TLInputCheckPasswordBase password, TLPasswordInputSettings newSettings, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLUpdatePasswordSettings { Password = password, NewSettings = newSettings };

            SendInformativeMessage("account.updatePasswordSettings", obj, callback, faultCallback);
	    }

	    public void CheckPasswordAsync(TLInputCheckPasswordBase password, Action<TLAuthorization> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLCheckPassword { Password = password };

            SendInformativeMessage("account.checkPassword", obj, callback, faultCallback);
	    }

	    public void RequestPasswordRecoveryAsync(Action<TLPasswordRecovery> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLRequestPasswordRecovery();

            SendInformativeMessage("account.requestPasswordRecovery", obj, callback, faultCallback);
	    }

	    public void RecoverPasswordAsync(TLString code, Action<TLAuthorization> callback, Action<TLRPCError> faultCallback = null)
	    {
	        var obj = new TLRecoverPassword {Code = code};

            SendInformativeMessage("account.recoverPassword", obj, callback, faultCallback);
	    }

	    public void ConfirmPhoneAsync(TLString phoneCodeHash, TLString phoneCode, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLConfirmPhone { PhoneCodeHash = phoneCodeHash, PhoneCode = phoneCode };

            SendInformativeMessage("account.confirmPhone", obj, callback, faultCallback);
	    }

	    public void SendConfirmPhoneCodeAsync(TLString hash, TLBool currentNumber, Action<TLSentCodeBase> callback, Action<TLRPCError> faultCallback = null)
	    {
            var obj = new TLSendConfirmPhoneCode { Flags = new TLInt(0), Hash = hash, CurrentNumber = currentNumber };

            SendInformativeMessage("account.sendConfirmPhoneCode", obj, callback, faultCallback);
        }

        public void GetWebAuthorizationsAsync(Action<TLWebAuthorizations> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetWebAuthorizations();

            SendInformativeMessage("account.getWebAuthorizations", obj, callback, faultCallback);
        }

        public void ResetWebAuthorizationAsync(TLLong hash, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLResetWebAuthorization { Hash = hash };

            SendInformativeMessage("account.resetWebAuthorization", obj, callback, faultCallback);
        }

        public void ResetWebAuthorizationsAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLResetWebAuthorizations();

            SendInformativeMessage("account.resetWebAuthorizations", obj, callback, faultCallback);
        }

        public void GetAllSecureValuesAsync(Action<TLVector<TLSecureValue>> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetAllSecureValues();

            SendInformativeMessage<TLObject>("account.getAllSecureValues", obj,
                result =>
                {
                    var vector = result as TLVector<TLSecureValue>;
                    if (vector != null)
                    {
                        callback.SafeInvoke(vector);
                    }
                    else
                    {
                        callback.SafeInvoke(new TLVector<TLSecureValue>());
                    }
                }, 
                faultCallback);
        }

	    public void GetPassportDataAsync(Action<TLPasswordBase, IList<TLSecureValue>> callback, Action<TLRPCError> faultCallback = null)
	    {
	        var requests = new TLObject[]
	        {
	            new TLGetPassword(), 
                new TLGetAllSecureValues()
	        };
            var returnValue = new TLObject[2];
	        GetPassportRequestsInternal(
                requests,
	            result =>
	            {
	                bool completed;
                    lock (returnValue)
                    {
                        if (result is TLPasswordBase) returnValue[0] = result;
                        if (result is IList<TLSecureValue>) returnValue[1] = result;
                        else if (result is IList<TLInt>) returnValue[1] = new TLVector<TLSecureValue>();
                        completed = returnValue[0] != null && returnValue[1] != null;
                    }

                    if (completed)
                    {
                        callback.SafeInvoke(returnValue[0] as TLPasswordBase, returnValue[1] as IList<TLSecureValue>);
                    }
	            },
	            faultCallback.SafeInvoke);
	    }

        private void GetPassportRequestsInternal(TLObject[] requests, Action<TLObject> getResultCallback, Action<TLRPCError> faultCallback = null)
	    {
            var container = new TLContainer { Messages = new List<TLContainerTransportMessage>() };
            var historyItems = new List<HistoryItem>();

            for (var i = 0; i < requests.Length; i++)
            {
                var obj = requests[i];
                int sequenceNumber;
                TLLong messageId;
                lock (_activeTransportRoot)
                {
                    sequenceNumber = _activeTransport.SequenceNumber * 2 + 1;
                    _activeTransport.SequenceNumber++;
                    messageId = _activeTransport.GenerateMessageId(true);
                }

                var data = i > 0 ? new TLInvokeAfterMsg { MsgId = container.Messages[i - 1].MessageId, Object = obj } : obj;

                var transportMessage = new TLContainerTransportMessage
                {
                    MessageId = messageId,
                    SeqNo = new TLInt(sequenceNumber),
                    MessageData = data
                };

                var historyItem = new HistoryItem
                {
                    SendTime = DateTime.Now,
                    Caption = "passport.item" + i,
                    Object = obj,
                    Message = transportMessage,
                    Callback = getResultCallback,
                    AttemptFailed = null,
                    FaultCallback = faultCallback,
                    ClientTicksDelta = ClientTicksDelta,
                    Status = RequestStatus.Sent,
                };
                historyItems.Add(historyItem);

                container.Messages.Add(transportMessage);
            }

            lock (_historyRoot)
            {
                foreach (var item in historyItems)
                {
                    _history[item.Hash] = item;
                }
            }
#if DEBUG
            NotifyOfPropertyChange(() => History);
#endif
            
            SendNonInformativeMessage<TLObject>("passport.container", container,
                result =>
                {
                    
                },
                faultCallback);
	    }

        public void GetSecureValueAsync(TLVector<TLSecureValueTypeBase> types, Action<TLVector<TLSecureValue>> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetSecureValue { Types = types };

            SendInformativeMessage("account.getSecureValue", obj, callback, faultCallback);
        }

        public void SaveSecureValueAsync(TLInputSecureValue value, TLLong secureSecretHash, Action<TLSecureValue> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSaveSecureValue { Value = value, SecureSecretId = secureSecretHash };

            SendInformativeMessage("account.saveSecureValue", obj, callback, faultCallback);
        }

        public void DeleteSecureValueAsync(TLVector<TLSecureValueTypeBase> types, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLDeleteSecureValue { Types = types };

            SendInformativeMessage("account.deleteSecureValue", obj, callback, faultCallback);
        }

        public void GetAuthorizationFormAsync(TLInt botId, TLString scope, TLString publicKey, Action<TLAuthorizationForm> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetAuthorizationForm { BotId = botId, Scope = scope, PublicKey = publicKey };

            SendInformativeMessage<TLAuthorizationForm>("account.getAuthorizationForm", obj,
                result =>
                {
                    _cacheService.SyncUsers(result.Users, users =>
                    {
                        result.Users = users;
                        callback.SafeInvoke(result);
                    });
                },
                faultCallback);
        }

        public void GetAuthorizationFormAndPassportConfigAsync(TLInt botId, TLString scope, TLString publicKey, TLInt passportConfigHash, Action<TLAuthorizationForm, TLPassportConfigBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var requests = new TLObject[]
            {
                new TLGetAuthorizationForm
                {
                    BotId = botId,
                    Scope = scope,
                    PublicKey = publicKey
                },
                new TLGetPassportConfig
                {
                    Hash = passportConfigHash
                }
            };

            var returnValue = new TLObject[2];
            GetPassportRequestsInternal(
                requests,
                result =>
                {
                    bool completed;
                    lock (returnValue)
                    {
                        if (result is TLAuthorizationForm) returnValue[0] = result;
                        if (result is TLPassportConfigBase) returnValue[1] = result;
                        completed = returnValue[0] != null && returnValue[1] != null;
                    }

                    if (completed)
                    {
                        callback.SafeInvoke(returnValue[0] as TLAuthorizationForm, returnValue[1] as TLPassportConfigBase);
                    }
                },
                faultCallback.SafeInvoke);
        }

        public void AcceptAuthorizationAsync(TLInt botId, TLString scope, TLString publicKey, TLVector<TLSecureValueHash> valueHashes, TLSecureCredentialsEncrypted credentials, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLAcceptAuthorization { BotId = botId, Scope = scope, PublicKey = publicKey, ValueHashes = valueHashes, Credentials = credentials };

            SendInformativeMessage("account.acceptAuthorization", obj, callback, faultCallback);
        }

        public void SendVerifyPhoneCodeAsync(TLString phoneNumber, TLBool currentNumber, Action<TLSentCodeBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSendVerifyPhoneCode { Flags = new TLInt(0), PhoneNumber = phoneNumber, CurrentNumber = currentNumber };

            SendInformativeMessage("account.sendVerifyPhoneCode", obj, callback, faultCallback);
        }

        public void VerifyPhoneAsync(TLString phoneNumber, TLString phoneCodeHash, TLString phoneCode, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLVerifyPhone { PhoneNumber = phoneNumber, PhoneCodeHash = phoneCodeHash, PhoneCode = phoneCode };

            SendInformativeMessage("account.verifyPhone", obj, callback, faultCallback);
        }

        public void SendVerifyEmailCodeAsync(TLString email, Action<TLSentEmailCode> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSendVerifyEmailCode { Email = email };

            SendInformativeMessage("account.sendVerifyEmailCode", obj, callback, faultCallback);
        }

        public void VerifyEmailAsync(TLString email, TLString code, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLVerifyEmail { Email = email, Code = code };

            SendInformativeMessage("account.verifyEmail", obj, callback, faultCallback);
        }

        public void InitTakeoutSessionAsync(bool contacts, bool messageUsers, bool messageChats, bool messageMegagroups, bool messageChannels, bool files, TLInt fileMaxSize, TLLong takeoutId, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLInitTakeoutSession
            {
                Flags = new TLInt(0),
                Contacts = contacts,
                MessageUsers = messageUsers,
                MessageChats = messageChats,
                MessageMegagroups = messageMegagroups,
                MessageChannels = messageChannels,
                Files = files,
                FileMaxSize = fileMaxSize,
                TakeoutId = takeoutId
            };

            SendInformativeMessage("account.initTakeoutSession", obj, callback, faultCallback);
        }
	}
}
