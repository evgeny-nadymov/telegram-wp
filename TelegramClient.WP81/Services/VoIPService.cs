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
using System.Windows.Threading;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Phone.Media.Devices;
using Windows.Phone.Networking.Voip;
using Windows.Storage;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Audio;
using TelegramClient.Controls;
using TelegramClient.ViewModels;
using PhoneVoIPApp.BackEnd;
using PhoneVoIPApp.UI;
using Org.BouncyCastle.Security;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Api.Transport;
using TelegramClient.ViewModels.Additional;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Controls;
using Action = System.Action;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Services
{
    public class VoIPService : IVoIPService, Telegram.Api.Aggregator.IHandle<TLUpdatePhoneCall>, IDisposable, ICallControllerStatusListener
    {
        public long AcceptedCallId { get; set; }

        private PhoneCallSound _spPlayId;

        private TLInt _userId;

        public TLInt UserId { get { return _userId; } }

        public TLPhoneCallBase Call { get { return _call; } }

        private readonly IMTProtoService _mtProtoService;

        private readonly ITelegramEventAggregator _eventAggregator;

        private readonly ICacheService _cacheService;

        private readonly IStateService _stateService;

        private readonly ITransportService _transportService;

        private Action _timeoutRunnable;

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public VoIPService(IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator, ICacheService cacheService, IStateService stateService, ITransportService transportService)
        {
            _mtProtoService = mtProtoService;
            _eventAggregator = eventAggregator;
            _cacheService = cacheService;
            _stateService = stateService;
            _transportService = transportService;

            NetworkInformation.NetworkStatusChanged += UpdateNetworkType;
            _timer.Tick += OnTimerTick;

            if (BackgroundProcessController.Instance != null
                && BackgroundProcessController.Instance.CallController != null)
            {
                var callController = BackgroundProcessController.Instance.CallController;
                Telegram.Logs.Log.Write(string.Format("VoIPService.ctor call={0} user_id={1} callController call_status={2} key={3} call_id={4}", _call != null, _userId != null, callController.CallStatus, callController.Key != null, callController.CallId));
                BackgroundProcessController.Instance.CallController.SetStatusCallback(this);

                if (_call == null || _userId == null)
                {
                    if (BackgroundProcessController.Instance.CallController.CallStatus == CallStatus.InProgress
                        || BackgroundProcessController.Instance.CallController.CallStatus == CallStatus.Held
                        || BackgroundProcessController.Instance.CallController.CallStatus == CallStatus.None)
                    {
                        if (BackgroundProcessController.Instance.CallController.Key != null)
                        {
                            lock (__callSyncRoot)
                            {
                                SetCall(new TLPhoneCall
                                {
                                    Id = new TLLong(BackgroundProcessController.Instance.CallController.CallId),
                                    AccessHash =
                                        new TLLong(BackgroundProcessController.Instance.CallController.CallAccessHash)
                                });
                            }

                            _userId = new TLInt((int) BackgroundProcessController.Instance.CallController.OtherPartyId);
                            _authKey = BackgroundProcessController.Instance.CallController.Key;
                            _outgoing = BackgroundProcessController.Instance.CallController.Outgoing;
                            _emojis = BackgroundProcessController.Instance.CallController.Emojis;

                            var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                            if (frame != null)
                            {
                                frame.ShowCallPlaceholder(ShellViewModel.OpenCurrentCall);
                            }
                        }
                    }
                }
            }
            else
            {
                Telegram.Logs.Log.Write("VoIPService.ctor empty");
            }

            _eventAggregator.Subscribe(this);
        }

        ~VoIPService()
        {
            BackgroundProcessController.Instance.CallController.SetStatusCallback(null);
        }

        private void SetCall(TLPhoneCallBase newCall)
        {
            lock (__callSyncRoot)
            {
                var callDiscarded = _call as TLPhoneCallDiscarded61;
                if (callDiscarded != null 
                    && newCall != null
                    && callDiscarded.Id.Value == newCall.Id.Value)
                {
                    Telegram.Logs.Log.Write(string.Format("SetCall skip\ncurrent={0}\nnew={1}", _call, newCall));
                    return;
                }

                _call = newCall;
            }
        }

        private void RunTimer(TimeSpan interval)
        {
            _timer.Stop();
            _timer.Interval = interval;
            _timer.Start();
        }

        private void CancelTimer()
        {
            _timer.Stop();
        }

        private void OnTimerTick(object sender, System.EventArgs e)
        {
            _timer.Stop();
            if (_timeoutRunnable != null)
            {
                _timeoutRunnable();
            }
        }

        private void UpdateNetworkType(object sender)
        {
            //return;
            //var networkType = GetNetworkType();

            //if (_controller != null)
            //{
            //    _controller.SetNetworkType(networkType);
            //}
        }

        private NetworkType GetNetworkType()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
            {
                if (profile.NetworkAdapter.IanaInterfaceType == 6)
                {
                    return NetworkType.Ethernet;
                }

                if (profile.IsWlanConnectionProfile)
                {
                    //wi-fi
                    return NetworkType.WiFi;
                }

                if (profile.IsWwanConnectionProfile)
                {
                    //mobile
                    var connectionClass = profile.WwanConnectionProfileDetails.GetCurrentDataClass();
                    switch (connectionClass)
                    {
                        //2G-equivalent
                        case WwanDataClass.Edge:
                            return NetworkType.EDGE;
                        case WwanDataClass.Gprs:
                            return NetworkType.GPRS;
                        //3G-equivalent
                        case WwanDataClass.Cdma1xEvdo:
                        case WwanDataClass.Cdma1xEvdoRevA:
                        case WwanDataClass.Cdma1xEvdoRevB:
                        case WwanDataClass.Cdma1xEvdv:
                        case WwanDataClass.Cdma1xRtt:
                        case WwanDataClass.Cdma3xRtt:
                        case WwanDataClass.CdmaUmb:
                        case WwanDataClass.Umts:
                            return NetworkType.UMTS;
                        case WwanDataClass.Hsdpa:
                        case WwanDataClass.Hsupa:
                            return NetworkType.HSPA;
                        //4G-equivalent
                        case WwanDataClass.LteAdvanced:
                            return NetworkType.LTE;

                        //not connected
                        case WwanDataClass.None:
                            return NetworkType.Unknown;

                        //unknown
                        case WwanDataClass.Custom:
                        default:
                            return NetworkType.OtherMobile;
                    }
                }
            }

            return NetworkType.Unknown;
        }

        private const int CALL_MIN_LAYER = 65;
        private const int CALL_MAX_LAYER = 74;

        private TLString _secretP;

        private TLString _secretRandom;

        private TLInt _secretG;

        private TLInt _lastVersion;

        private byte[] _ga;

        private readonly object __callSyncRoot = new object();

        private TLPhoneCallBase _call;
        private byte[] _aOrB;
        private byte[] _authKey;
        private bool _outgoing = true;

        //private VoIPControllerWrapper _controller;
        //private CallController _controller;
        //BackgroundProcessController.Instance.CallController
        private Error _lastError;

        private bool _controllerStarted;
        private DateTime? _startCallTime;
        private bool _needSendDebugLog;
        private PhoneCallState _currentState;
        private int _signal;
        private bool _endCallAfterRequest;
        private string[] _emojis;
        private bool _needPlayEndSound;
        private IList<TLUpdatePhoneCall> _pendingUpdates = new List<TLUpdatePhoneCall>();
        private byte[] _gb;
        private string _debugLog;
        private long _debugCallId;

        //private VoipPhoneCall _voipPhoneCall;

        public void StartOutgoingCall(TLUser user, Action<TLLong> callback)
        {
            SetCall(null);
            _authKey = null;

            if (user == null)
            {
                return;
            }

            var inputUser = user.ToInputUser();
            if (inputUser == null)
            {
                return;
            }

            _userId = user.Id;

            ConfigureDeviceForCall();
            ShowNotifications();
            StartConnectingSound();
            DispatchStateChanged(PhoneCallState.STATE_REQUESTING);
            _outgoing = true;

            var salt = new Byte[256];
            var random = new SecureRandom();
            random.NextBytes(salt);

            var version = _lastVersion ?? new TLInt(0);
            var randomLength = new TLInt(256);

            _mtProtoService.GetDHConfigAsync(version, randomLength,
                result =>
                {
                    var dhConfig =  result as TLDHConfig;
                    if (dhConfig != null)
                    {
                        if (!TLUtils.CheckPrime(dhConfig.P.Data, dhConfig.G.Value))
                        {
                            CallFailed();
                            return;
                        }

                        _secretP = dhConfig.P;
                        _secretG = dhConfig.G;
                        _secretRandom = dhConfig.Random;
                    }

                    for (var i = 0; i < 256; i++)
                    {
                        salt[i] = (byte) (salt[i] ^ _secretRandom.Data[i]);
                    }

                    var gaBytes = MTProtoService.GetGB(salt, _secretG, _secretP);

                    var protocol = new TLPhoneCallProtocol
                    {
                        Flags = new TLInt(0),
                        UdpP2P = true,
                        UdpReflector = true,
                        MinLayer = new TLInt(CALL_MIN_LAYER),
                        MaxLayer = new TLInt(CALL_MAX_LAYER)
                    };
                    _ga = gaBytes;
                    var gaHash = Telegram.Api.Helpers.Utils.ComputeSHA256(_ga);

                    _mtProtoService.RequestCallAsync(user.ToInputUser(), TLInt.Random(), TLString.FromBigEndianData(gaHash), protocol,
                        result2 =>
                        {
                            SetCall(result2.PhoneCall);
                            _aOrB = salt;
                            DispatchStateChanged(PhoneCallState.STATE_WAITING);

                            if (_pendingUpdates.Count > 0 && _call != null)
                            {
                                foreach (var updatePhoneCall in _pendingUpdates)
                                {
                                    Handle(updatePhoneCall);
                                }
                                _pendingUpdates.Clear();
                            }

                            callback.SafeInvoke(result2.PhoneCall.Id);

                            Execute.BeginOnUIThread(() =>
                            {
                                _timeoutRunnable = () =>
                                {
                                    _timeoutRunnable = null;

                                    var inputPhoneCall = _call as IInputPhoneCall;
                                    if (inputPhoneCall != null)
                                    {
                                        var duration = _controllerStarted ? (GetCallDuration() / 1000) : 0;
                                        var connectionId = _controllerStarted ? BackgroundProcessController.Instance.CallController.GetPreferredRelayID() : 0;

                                        _mtProtoService.DiscardCallAsync(inputPhoneCall.ToInputPhoneCall(), new TLInt(duration), new TLPhoneCallDiscardReasonMissed(), new TLLong(connectionId),
                                            result3 =>
                                            {
                                                Telegram.Logs.Log.Write("phone.discardCall result " + result3);
                                            },
                                            error3 =>
                                            {
                                                Telegram.Logs.Log.Write("phone.discardCall error " + error3);
                                                CallFailed();
                                            });
                                    }
                                };

                                var config = _cacheService.GetConfig() as TLConfig63;
                                var timeout = config != null ? config.CallReceiveTimeoutMs.Value : 30000;
                                RunTimer(TimeSpan.FromMilliseconds(timeout));
                            });
                        },
                        error2 =>
                        {
                            
                        });
                },
                error =>
                {
                    Telegram.Logs.Log.Write("messages.getDHConfig error " + error);
                    CallFailed();
                });
        }

        public void AcceptIncomingCall(TLPhoneCallRequested64 callRequested)
        {
            if (callRequested == null) return;

            _authKey = null;

            StopRinging();
            ShowNotification();
            ConfigureDeviceForCall();
            StartConnectingSound();
            DispatchStateChanged(PhoneCallState.STATE_EXCHANGING_KEYS);

            var salt = new Byte[256];
            var random = new SecureRandom();
            random.NextBytes(salt);

            var version = _lastVersion ?? new TLInt(0);
            var randomLength = new TLInt(256);

            _mtProtoService.GetDHConfigAsync(version, randomLength,
                result =>
                {
                    var dhConfig = result as TLDHConfig;
                    if (dhConfig != null)
                    {
                        if (!TLUtils.CheckPrime(dhConfig.P.Data, dhConfig.G.Value))
                        {
                            CallFailed();
                            return;
                        }

                        _secretP = dhConfig.P;
                        _secretG = dhConfig.G;
                        _secretRandom = dhConfig.Random;
                    }

                    for (var i = 0; i < 256; i++)
                    {
                        salt[i] = (byte)(salt[i] ^ _secretRandom.Data[i]);
                    }

                    _aOrB = salt;
                    var gbBytes = MTProtoService.GetGB(salt, _secretG, _secretP);
                    
                    var protocol = new TLPhoneCallProtocol
                    {
                        Flags = new TLInt(0),
                        UdpP2P = true,
                        UdpReflector = true,
                        MinLayer = new TLInt(CALL_MIN_LAYER),
                        MaxLayer = new TLInt(CALL_MAX_LAYER)
                    };
                    _gb = gbBytes;

                    _mtProtoService.AcceptCallAsync(callRequested.ToInputPhoneCall(), TLString.FromBigEndianData(gbBytes), protocol,
                        result2 =>
                        {
                            Telegram.Logs.Log.Write(string.Format("phone.acceptCall result={0} current={1}", result2, _call));
                            SetCall(result2.PhoneCall);

                            if (result2.PhoneCall is TLPhoneCallDiscarded61)
                            {
                                Handle(new TLUpdatePhoneCall { PhoneCall = result2.PhoneCall });
                            }
                        },
                        error2 =>
                        {
                            Telegram.Logs.Log.Write("phone.acceptCall error=" + error2);
                            CallFailed();
                        });
                },
                error =>
                {
                    Telegram.Logs.Log.Write("messages.getDHConfig error " + error);
                    CallFailed();
                });
        }

        private void ShowNotification()
        {
            
        }

        private void StopRinging()
        {
            Execute.BeginOnUIThread(() =>
            {
                _maxLoopedCount = 0;
                _lastLoopedUri = null;
                MessagePlayerControl.Player.Stop();
            });
        }


        private void VoipPhoneCall_RejectRequested(VoipPhoneCall sender, CallRejectEventArgs args)
        {
            
        }

        private void VoipPhoneCall_AnswerRequested(VoipPhoneCall sender, CallAnswerEventArgs args)
        {
            
        }

        private void DispatchStateChanged(PhoneCallState callState)
        {
            System.Diagnostics.Debug.WriteLine("  DispatchSateChanged state=" + callState);
            _currentState = callState;
            _eventAggregator.Publish(new PhoneCallStateChangedEventArgs{CallState = callState, Call = Call });
        }

        private void StartConnectingSound()
        {
            PlaySound(PhoneCallSound.Connecting, -1);
        }

        private void ShowNotifications()
        {
            

        }

        private void ConfigureDeviceForCall()
        {
            _needPlayEndSound = true;
        }

        private void CallFailed()
        {
            CallFailed(_controllerStarted ? BackgroundProcessController.Instance.CallController.GetLastError() : 0);
        }

        private void CallFailed(Error errorCode)
        {
            Telegram.Logs.Log.Write("Call " + (_call != null ? _call.Id.Value : 0)+" failed with error code " + errorCode);

		    _lastError = errorCode;
		    if (_call != null) 
            {
			    Telegram.Logs.Log.Write("Discarding failed call");

                var phoneCall = _call as IInputPhoneCall;
                if (phoneCall != null)
                {
                    var peer = phoneCall.ToInputPhoneCall();
                    var duration = _controllerStarted ? (GetCallDuration() / 1000) : 0;
                    var connectionId = _controllerStarted ? BackgroundProcessController.Instance.CallController.GetPreferredRelayID() : 0;
                    var reason = new TLPhoneCallDiscardReasonDisconnect();

                    _mtProtoService.DiscardCallAsync(peer, new TLInt(duration), reason, new TLLong(connectionId),
                        result =>
                        {
                            Telegram.Logs.Log.Write("phone.discardCall result=" + result);
                        },
                        error =>
                        {
                            Telegram.Logs.Log.Write("phone.discardCall error=" + error);
                        });
                }
		    }
		    DispatchStateChanged(PhoneCallState.STATE_FAILED);
            PlaySound(PhoneCallSound.Failed, 0);

            BackgroundProcessController.Instance.CallController.EndCall();

            StopSelf();

            //if(errorCode!=VoIPController.ERROR_LOCALIZED && soundPool!=null){
            //    playingSound=true;
            //    soundPool.play(spFailedID, 1, 1, 0, 0, 1);
            //    AndroidUtilities.runOnUIThread(new Runnable(){
            //        @Override
            //        public void run(){
            //            soundPool.release();
            //            if(isBtHeadsetConnected)
            //                ((AudioManager) ApplicationLoader.applicationContext.getSystemService(AUDIO_SERVICE)).stopBluetoothSco();
            //        }
            //    }, 1000);
            //}
		    //StopSelf();
        }

        private void CallEnded()
        {
            Telegram.Logs.Log.Write("Call " + (_call != null ? _call.Id.Value : 0) + " ended");
            DispatchStateChanged(PhoneCallState.STATE_ENDED);
            if (_needPlayEndSound)
            {
                _needPlayEndSound = false;
                PlaySound(PhoneCallSound.End, 0);

                //playingSound = true;
                //soundPool.play(spEndId, 1, 1, 0, 0, 1);
                //AndroidUtilities.runOnUIThread(new Runnable() {
                //    @Override
                //    public void run() {
                //        soundPool.release();
                //        if(isBtHeadsetConnected)
                //            ((AudioManager)ApplicationLoader.applicationContext.getSystemService(AUDIO_SERVICE)).stopBluetoothSco();
                //    }
                //}, 1000);
            }
            else
            {
                StopRinging();
            }
            Execute.BeginOnUIThread(() =>
            {
                if (_timeoutRunnable != null)
                {
                    CancelTimer();
                    _timeoutRunnable = null;
                }
            });

            BackgroundProcessController.Instance.CallController.EndCall();

            StopSelf();
        }

        private int GetCallDuration()
        {
            if (_startCallTime.HasValue)
            {
                return (int) (DateTime.Now - _startCallTime.Value).TotalMilliseconds;
            }

            return 0;
        }

        public void Handle(TLUpdatePhoneCall updatePhoneCall)
        {
            Telegram.Logs.Log.Write(string.Format("VoIPService.Handle\nupdated_call={0}\ncurrent_call={1}", updatePhoneCall.PhoneCall, _call));
            //if (_call == null)
            //{
            //    _pendingUpdates.Add(updatePhoneCall);
            //    return;
            //}

            if (updatePhoneCall == null)
            {
                return;
            }

            if (Call != null 
                && !(Call is TLPhoneCallDiscarded)
                && Call.Id.Value != updatePhoneCall.PhoneCall.Id.Value)
            {
                if (updatePhoneCall.PhoneCall is TLPhoneCallRequestedBase)
                {
                    DeclineIncomingCallInternal(PhoneDiscardReason.DISCARD_REASON_LINE_BUSY, () => { }, updatePhoneCall.PhoneCall);
                }

                return;
            }
            //if (updatePhoneCall.PhoneCall.Id.Value != _call.PhoneCall.Id.Value)
            //{
            //    var error = "onCallUpdated called with wrong call id (got " + updatePhoneCall.PhoneCall.Id.Value + ", expected " + _call.PhoneCall.Id.Value + ")";
            //    Telegram.Logs.Log.Write(error);
            //    Execute.ShowDebugMessage(error);
            //    return;
            //}

            var phoneCallBase = updatePhoneCall.PhoneCall;

            var phoneCallAccepted = phoneCallBase as TLPhoneCallAccepted;
            if (phoneCallAccepted != null)
            {
                if (_authKey == null)
                {
                    ProcessAcceptedCall(phoneCallAccepted);
                }

                return;
            }

            var phoneCall = phoneCallBase as TLPhoneCall;
            if (phoneCall != null)
            {
                if (_authKey == null)
                {
                    ProcessIncomingCall(phoneCall);
                }

                return;
            }

            var phoneCallDiscarded = phoneCallBase as TLPhoneCallDiscarded61;
            if (phoneCallDiscarded != null)
            {
                var currentCall = _call;
                if (currentCall != null)
                {
                    SetCall(phoneCallDiscarded);
                    _needSendDebugLog = phoneCallDiscarded.NeedDebug;
                    Telegram.Logs.Log.Write("call discarded, stopping service");
                    if (phoneCallDiscarded.Reason is TLPhoneCallDiscardReasonBusy)
                    {
                        DispatchStateChanged(PhoneCallState.STATE_BUSY);

                        PlaySound(PhoneCallSound.Busy, 2);
                        StopSelf();
                        //playingSound = true;
                        //soundPool.play(spBusyId, 1, 1, 0, -1, 1);
                        //AndroidUtilities.runOnUIThread(new Runnable() {
                        //    @Override
                        //    public void run() {
                        //        soundPool.release();
                        //        if(isBtHeadsetConnected)
                        //            ((AudioManager)ApplicationLoader.applicationContext.getSystemService(AUDIO_SERVICE)).stopBluetoothSco();
                        //    }
                        //}, 2500);
                        //stopSelf();
                    }
                    else
                    {
                        CallEnded();
                    }

                    if (phoneCallDiscarded.NeedDebug)
                    {
                        _debugCallId = _call.Id.Value;
                        _debugLog = BackgroundProcessController.Instance.CallController.GetDebugLog();
                    }

                    BackgroundProcessController.Instance.CallController.DeleteVoIPControllerWrapper();
                    //if (_controller != null)
                    //{
                    //    _controller.Dispose();
                    //    _controller = null;
                    //}
                    _eventAggregator.Publish(new PhoneCallDiscardedEventArgs { Call = currentCall, DiscardedCall = phoneCallDiscarded, Outgoing = _outgoing });

                    if (phoneCallDiscarded.NeedRating)
                    {
                        StartRatingActivity();
                    }
                }
            }

            var phoneCallRequested = phoneCallBase as TLPhoneCallRequested64;
            if (phoneCallRequested != null)
            {
                SetCall(phoneCallRequested);
                _outgoing = false;
                OnStartCommand();
                _eventAggregator.Publish(new PhoneCallRequestedEventArgs{ RequestedCall = phoneCallRequested });

                Execute.BeginOnUIThread(() =>
                {
                    if (_timeoutRunnable != null)
                    {
                        _timeoutRunnable = null;
                        CancelTimer();
                    }

                    _timeoutRunnable = () =>
                    {
                        _timeoutRunnable = null;
                        DeclineIncomingCall(PhoneDiscardReason.DISCARD_REASON_MISSED, null);
                    };
                    var config = _cacheService.GetConfig() as TLConfig63;
                    var timeout = config != null ? config.CallRingTimeoutMs.Value : 30000;
                    RunTimer(TimeSpan.FromMilliseconds(timeout));
                });
            }

            var phoneCallWaiting = phoneCallBase as TLPhoneCallWaiting;
            if (phoneCallWaiting != null)
            {
                if (_currentState == PhoneCallState.STATE_WAITING && phoneCallWaiting.ReceiveDate != null)
                {
                    DispatchStateChanged(PhoneCallState.STATE_RINGING);
                    PlaySound(PhoneCallSound.Ringback, -1);

                    //Execute.BeginOnUIThread(() =>
                    //{
                    //    if (_timeoutRunnable != null)
                    //    {
                    //        _timeoutRunnable = null;
                    //        CancelTimer();
                    //    }

                    //    _timeoutRunnable = () =>
                    //    {
                    //        _timeoutRunnable = null;
                    //        DeclineIncomingCall(PhoneDiscardReason.DISCARD_REASON_MISSED, null);
                    //    };
                    //    RunTimer(TimeSpan.FromSeconds(Constants.CallRingTimeout));
                    //});
                }
            }
        }

        private void OnStartCommand()
        {
            if (_outgoing)
            {
                //StartOutgoingCall();
            }
            else
            {
                AcknowledgeCallAndStartRinging();
            }
        }

        private void AcknowledgeCallAndStartRinging()
        {
            var phoneCallDiscarded = _call as TLPhoneCallDiscarded;
            if (phoneCallDiscarded != null)
            {
                Telegram.Logs.Log.Write("Call " + phoneCallDiscarded.Id + " was discarded before the service started, stopping");
                StopSelf();
                return;
            }

            var inputPhoneCall = _call as IInputPhoneCall;
            if (inputPhoneCall != null)
            {
                _mtProtoService.ReceivedCallAsync(inputPhoneCall.ToInputPhoneCall(),
                    result =>
                    {
                        Telegram.Logs.Log.Write("phone.receivedCall result=" + result.Value);
                        StartRinging();
                    },
                    error =>
                    {
                        Telegram.Logs.Log.Write("phone.receivedCall error=" + error);
                        StopSelf();
                    });
            }
        }

        private void StartRinging()
        {
            Telegram.Logs.Log.Write("starting ringing for call " + (_call != null ? _call.Id : null));
            DispatchStateChanged(PhoneCallState.STATE_WAITING_INCOMING);
            PlaySound(PhoneCallSound.Call, -1);
        }

        private static Uri _lastLoopedUri;

        private static int _loopedCount;

        private static int _maxLoopedCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="loopedCount"> -1 -- forewer, 0 -- once, 1 -- twice </param>
        public static void PlaySound(PhoneCallSound sound, int loopedCount)
        {
            Execute.BeginOnUIThread(() =>
            {
                MessagePlayerControl.Stop();
                GifPlayerControl.StopVideo();

                var source = GetSoundSource(sound);

                MessagePlayerControl.Player.Volume = 0.75;
                MessagePlayerControl.Player.Source = new Uri(source, UriKind.Relative);
                MessagePlayerControl.Player.MediaOpened += MessagePlayerControl_OnMediaOpened;
                MessagePlayerControl.Player.MediaFailed += MessagePlayerControl_OnMediaFailed;

                MessagePlayerControl.Player.MediaEnded -= MessagePlayerControl_OnMediaEnded;
                _maxLoopedCount = loopedCount;
                _loopedCount = 0;
                _lastLoopedUri = MessagePlayerControl.Player.Source;
                MessagePlayerControl.Player.MediaEnded += MessagePlayerControl_OnMediaEnded;
            });
        }

        private static void MessagePlayerControl_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            if (_loopedCount > _maxLoopedCount - 1 && _maxLoopedCount != -1)
            {
                return;
            }

            if (_lastLoopedUri == MessagePlayerControl.Player.Source)
            {
                MessagePlayerControl.Player.Position = TimeSpan.FromSeconds(0.0);
                MessagePlayerControl.Player.Play();
                _loopedCount++;
            }
        }

        private static void MessagePlayerControl_OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessagePlayerControl.Player.MediaOpened -= MessagePlayerControl_OnMediaOpened;
            MessagePlayerControl.Player.MediaFailed -= MessagePlayerControl_OnMediaFailed;
        }

        private static void MessagePlayerControl_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            MessagePlayerControl.Player.MediaOpened -= MessagePlayerControl_OnMediaOpened;
            MessagePlayerControl.Player.MediaFailed -= MessagePlayerControl_OnMediaFailed;
            MessagePlayerControl.Player.Play();
        }

        private static string GetSoundSource(PhoneCallSound sound)
        {
            switch (sound)
            {
                case PhoneCallSound.Call:
                    return "/Sounds/voip_call2.mp3";
                case PhoneCallSound.Ringback:
                    return "/Sounds/voip_ringback.mp3";
                case PhoneCallSound.Failed:
                    return "/Sounds/voip_failed.mp3";
                case PhoneCallSound.End:
                    return "/Sounds/voip_end.mp3";
                case PhoneCallSound.Busy:
                    return "/Sounds/voip_busy.mp3";
                case PhoneCallSound.Connecting:
                    return "/Sounds/voip_connecting.mp3";
            }

            return null;
        }

        private void StopSelf()
        {
            
        }

        private void StartRatingActivity()
        {
            
        }

        private void ProcessIncomingCall(TLPhoneCall call)
        {
            if(call.GAorB == null){
				Telegram.Logs.Log.Write("stopping VoIP service, Ga == null");
				CallFailed();
				return;
			}

            //if(!Arrays.equals(g_a_hash, Utils.ComputeSHA256(call.GAorB.Data))){
            //    Telegram.Logs.Log.Write("stopping VoIP service, Ga hash doesn't match");
            //    CallFailed();
            //    return;
            //}

			_ga=call.GAorB.Data;

            if (_secretP == null
                || !TLUtils.CheckGaAndGb(call.GAorB.Data, _secretP.Data))
            {
                CallFailed();
                return;
            }

            _authKey = MTProtoService.GetAuthKey(_aOrB, call.GAorB.ToBytes(), _secretP.ToBytes());
            var buffer = TLUtils.Combine(_authKey, _ga);
            var sha256 = Telegram.Api.Helpers.Utils.ComputeSHA256(buffer);

            _emojis = EncryptionKeyEmojifier.EmojifyForCall(sha256);

            var keyHash = Telegram.Api.Helpers.Utils.ComputeSHA1(_authKey); 
            var keyFingerprint = new TLLong(BitConverter.ToInt64(keyHash, 12));


            if (keyFingerprint.Value != call.KeyFingerprint.Value)
            {
                Telegram.Logs.Log.Write("key fingerprints don't match");
                CallFailed();
                return;
            }

            SetCall(call);
            _outgoing = false;
            InitiateActualEncryptedCall();
        }

        private void ProcessAcceptedCall(TLPhoneCallAccepted phoneCallAccepted)
        {
            DispatchStateChanged(PhoneCallState.STATE_EXCHANGING_KEYS);

            if (!TLUtils.CheckGaAndGb(phoneCallAccepted.GB.Data, _secretP.Data))
            {
                CallFailed();
                return;
            }

            _authKey = MTProtoService.GetAuthKey(_aOrB, phoneCallAccepted.GB.ToBytes(), _secretP.ToBytes());
            var buffer = TLUtils.Combine(_authKey, _ga);
            var sha256 = Telegram.Api.Helpers.Utils.ComputeSHA256(buffer);

            _emojis = EncryptionKeyEmojifier.EmojifyForCall(sha256);

            var keyHash = Telegram.Api.Helpers.Utils.ComputeSHA1(_authKey); 
            var keyFingerprint = new TLLong(BitConverter.ToInt64(keyHash, 12));

            var peer = new TLInputPhoneCall
            {
                Id = phoneCallAccepted.Id, 
                AccessHash = phoneCallAccepted.AccessHash
            };

            var protocol = new TLPhoneCallProtocol
            {
                Flags = new TLInt(0),
                UdpP2P = true,
                UdpReflector = true,
                MinLayer = new TLInt(CALL_MIN_LAYER),
                MaxLayer = new TLInt(CALL_MAX_LAYER)
            };

            _mtProtoService.ConfirmCallAsync(peer, TLString.FromBigEndianData(_ga), keyFingerprint, protocol,
                result =>
                {
                    Telegram.Logs.Log.Write(string.Format("phone.confirmCall result={0} current={1}", result, _call));
                    SetCall(result.PhoneCall);
                    //_voipPhoneCall.NotifyCallActive();
                    InitiateActualEncryptedCall();
                },
                error =>
                {
                    Telegram.Logs.Log.Write(string.Format("phone.confirmCall error={0} current={1}", error, _call));
                    CallFailed();
                });

        }

        private EndpointStruct ToEndpoint(TLPhoneConnection connection)
        {
            var connection61 = connection as TLPhoneConnection61;
            if (connection61 != null)
            {
                return new EndpointStruct
                {
                    id = connection61.Id.Value,
                    ipv4 = connection61.Ip.ToString(),
                    ipv6 = connection61.IpV6.ToString(),
                    port = (ushort) connection61.Port.Value,
                    peerTag = connection61.PeerTag.ToString()
                };
            }

            return new EndpointStruct
            {
                id = 0,
                ipv4 = connection.Ip.ToString(),
                ipv6 = connection.IpV6.ToString(),
                port = (ushort)connection.Port.Value,
                peerTag = connection.PeerTag.ToString()
            };
        }

        private void InitiateActualEncryptedCall()
        {
            Execute.BeginOnUIThread(() =>
            {
                if (_timeoutRunnable != null)
                {
                    _timeoutRunnable = null;
                    CancelTimer();
                }
            });

            _mtProtoService.GetCallConfigAsync(result =>
            {
                var jobject = JsonValue.Parse(result.Data.ToString()).GetObject();
                foreach (var obj in jobject)
                {
                    System.Diagnostics.Debug.WriteLine(obj.Value.Stringify());
                }


                //VoIPControllerWrapper.UpdateServerConfig(result.Data.ToString());
                BackgroundProcessController.Instance.CallController.UpdateServerConfig(result.Data.ToString());

                var logFile = ApplicationData.Current.LocalFolder.Path + "\\tgvoip.logFile.txt";
                var statsDumpFile = ApplicationData.Current.LocalFolder.Path + "\\tgvoip.statsDump.txt";

                _cacheService.GetConfigAsync(config =>
                {
                    var config60 = config as TLConfig60;
                    if (config60 != null)
                    {
                        var phoneCall = _call as TLPhoneCall;
                        if (phoneCall != null)
                        {
                            var protocol = phoneCall.Protocol;

                            var callConfig = new Config
                            {
                                InitTimeout = config60.CallPacketTimeoutMs.Value/1000.0,
                                RecvTimeout = config60.CallConnectTimeoutMs.Value/1000.0,
                                DataSavingMode = DataSavingMode.Never,
                                EnableAEC = true,
                                EnableNS = true,
                                EnableAGC = true,
                                LogFilePath = logFile,
                                StatsDumpFilePath = statsDumpFile
                            };

                            var connection = phoneCall.Connection;
                            var endpoints = new EndpointStruct[phoneCall.AlternativeConnections.Count + 1];
                            endpoints[0] = ToEndpoint(connection);

                            for (int i = 0; i < phoneCall.AlternativeConnections.Count; i++)
                            {
                                connection = phoneCall.AlternativeConnections[i];
                                endpoints[i + 1] = ToEndpoint(connection);
                            }

                            //BackgroundProcessController.Instance.CallController.DeleteVoIPControllerWrapper();
                            //BackgroundProcessController.Instance.CallController.CreateVoIPControllerWrapper();
                            //BackgroundProcessController.Instance.CallController.SetConfig(callConfig);
                            //BackgroundProcessController.Instance.CallController.SetStatusCallback(this);
                            //BackgroundProcessController.Instance.CallController.SetEncryptionKey(_authKey, _outgoing);
                            //BackgroundProcessController.Instance.CallController.SetPublicEndpoints(endpoints, phoneCall.Protocol.UdpP2P);
                            //BackgroundProcessController.Instance.CallController.Start();
                            //UpdateNetworkType(null);
                            //BackgroundProcessController.Instance.CallController.Connect();
                            
                            var config82 = config as TLConfig82;
                            var defaultP2PContacts = config82 == null || config82.DefaultP2PContacts;
                            var callsSecurity = _stateService.GetCallsSecurity(defaultP2PContacts);

                            var proxy = new ProxyStruct
                            {
                                protocol = ProxyProtocol.None,
                                address = string.Empty,
                                port = 1080,
                                password = string.Empty,
                                username = string.Empty
                            };
                            var proxyConfig = _transportService.GetProxyConfig() as TLProxyConfig76;
                            if (proxyConfig != null 
                                && proxyConfig.IsEnabled.Value
                                && !proxyConfig.IsEmpty
                                && proxyConfig.UseForCalls.Value
                                && proxyConfig.SelectedIndex != null
                                && proxyConfig.SelectedIndex.Value >= 0
                                && proxyConfig.SelectedIndex.Value < proxyConfig.Items.Count)
                            {
                                var socks5Proxy = proxyConfig.Items[proxyConfig.SelectedIndex.Value] as TLSocks5Proxy;
                                if (socks5Proxy != null)
                                {
                                    proxy = new ProxyStruct
                                    {
                                        protocol = ProxyProtocol.SOCKS5,
                                        address = socks5Proxy.Server.ToString(),
                                        port = (ushort) socks5Proxy.Port.Value,
                                        username = socks5Proxy.Username.ToString(),
                                        password = socks5Proxy.Password.ToString()
                                    };
                                }
                            }

                            var userId = !_outgoing ? phoneCall.AdminId : phoneCall.ParticipantId;
                            var user = _cacheService.GetUser(userId) as TLUser;
                            if (user != null)
                            {
                                var allowP2P = false;
                                if (callsSecurity != null)
                                {
                                    allowP2P = callsSecurity.PeerToPeerEverybody 
                                        || callsSecurity.PeerToPeerContacts && user.IsContact
                                        || callsSecurity.PeerToPeerContacts && user.IsContactMutual;
                                }

                                _userId = user.Id;
                                
                                var initiated = BackgroundProcessController.Instance.CallController.InitiateOutgoingCall(
                                    user.FullName2, user.Index,
                                    phoneCall.Id.Value, phoneCall.AccessHash.Value,
                                    callConfig,
                                    _authKey, _outgoing,
                                    _emojis,
                                    endpoints, 
                                    phoneCall.Protocol.UdpP2P && allowP2P,
                                    protocol.MaxLayer.Value,
                                    proxy);

                                if (initiated)
                                {
                                    _controllerStarted = true;
                                    _startCallTime = DateTime.Now;

                                    _eventAggregator.Publish(new PhoneCallStartedEventArgs { Call = phoneCall, Emojis = _emojis });
                                }
                            }
                            //_controller.SetSpeakerphoneEndpoint();
                        }
                    }
                });

            },
            error =>
            {
                    
            });
        }

        public void OnSignalBarsChanged(int newSignal)
        {
            System.Diagnostics.Debug.WriteLine("  OnSignalBarsChanged newSignal=" + newSignal);
            _signal = newSignal;
            _eventAggregator.Publish(new SignalBarsChangedEventArgs { Signal = newSignal });
        }

        public void OnCallStateChanged(CallState newState)
        {
            System.Diagnostics.Debug.WriteLine("==OnCallStateChanged new_state=" + newState);
            DispatchStateChanged((PhoneCallState)newState);

            if (newState == CallState.Failed)
            {
                CallFailed();
                return;
            }

            if (newState == CallState.Established)
            {
                return;
            }
        }

        public void Dispose()
        {
            //_eventAggregator.Unsubscribe(this);
        }

        public void HangUp()
        {
            DeclineIncomingCall(_currentState == PhoneCallState.STATE_RINGING || (_currentState == PhoneCallState.STATE_WAITING && _outgoing) ? PhoneDiscardReason.DISCARD_REASON_MISSED : PhoneDiscardReason.DISCARD_REASON_HANGUP, null);
        }

        public string[] GetEmojis()
        {
            return _emojis;
        }

        private void DeclineIncomingCall(PhoneDiscardReason discardReason, Action callback)
        {
            if (_currentState == PhoneCallState.STATE_REQUESTING)
            {
                _endCallAfterRequest = true;
                return;
            }

            if (_currentState == PhoneCallState.STATE_HANGING_UP || _currentState == PhoneCallState.STATE_ENDED)
            {
                return;
            }

            DispatchStateChanged(PhoneCallState.STATE_HANGING_UP);
            if (_call == null)
            {
                callback.SafeInvoke();
                CallEnded();
                return;
            }

            if (_call != null)
            {
                DeclineIncomingCallInternal(discardReason, callback, _call);
            }
        }

        private void DeclineIncomingCallInternal(PhoneDiscardReason discardReason, Action callback, TLPhoneCallBase call)
        {
            var phoneCall = call as IInputPhoneCall;
            if (phoneCall != null)
            {
                var peer = phoneCall.ToInputPhoneCall();
                var duration = _controllerStarted ? (GetCallDuration()/1000) : 0;
                var connectionId = _controllerStarted ? BackgroundProcessController.Instance.CallController.GetPreferredRelayID() : 0;
                TLPhoneCallDiscardReasonBase reason;
                switch (discardReason)
                {
                    case PhoneDiscardReason.DISCARD_REASON_DISCONNECT:
                        reason = new TLPhoneCallDiscardReasonDisconnect();
                        break;
                    case PhoneDiscardReason.DISCARD_REASON_MISSED:
                        reason = new TLPhoneCallDiscardReasonMissed();
                        break;
                    case PhoneDiscardReason.DISCARD_REASON_LINE_BUSY:
                        reason = new TLPhoneCallDiscardReasonBusy();
                        break;
                    case PhoneDiscardReason.DISCARD_REASON_HANGUP:
                    default:
                        reason = new TLPhoneCallDiscardReasonHangup();
                        break;
                }

                _mtProtoService.DiscardCallAsync(peer, new TLInt(duration), reason, new TLLong(connectionId),
                    result =>
                    {
                        Telegram.Logs.Log.Write("phone.discardCall result " + result);

                        callback.SafeInvoke();
                    },
                    error =>
                    {
                        Telegram.Logs.Log.Write("phone.discardCall error " + error);

                        callback.SafeInvoke();
                    });
            }
        }

        public void SwitchSpeaker(bool external)
        {
            BackgroundProcessController.Instance.CallController.SwitchSpeaker(external);
        }

        public void Mute(bool muted)
        {
            BackgroundProcessController.Instance.CallController.SetMicMute(muted);
        }

        public string GetDebugString()
        {
            return BackgroundProcessController.Instance.CallController.GetDebugString() ?? string.Empty;
        }

        public string GetDebugLog(long callId)
        {
            return callId == _debugCallId ? _debugLog ?? string.Empty : string.Empty; 
        }

        public string GetVersion()
        {
            return BackgroundProcessController.Instance.CallController.GetVersion();
        }

        public int GetSignalBarsCount()
        {
            return BackgroundProcessController.Instance.CallController.GetSignalBarsCount();
        }

        public void OnCallStatusChanged(CallStatus newStatus)
        {

        }

        public void OnCallAudioRouteChanged(CallAudioRoute newRoute)
        {

        }

        public void OnMediaOperationsChanged(MediaOperations newOperations)
        {

        }

        public void OnCameraLocationChanged(CameraLocation newCameraLocation)
        {

        }
    }

    public class PhoneCallStartedEventArgs
    {
        public TLPhoneCall Call { get; set; }

        public string[] Emojis { get; set; }
    }

    public class PhoneCallDiscardedEventArgs
    {
        public TLPhoneCallBase Call { get; set; }

        public TLPhoneCallDiscarded61 DiscardedCall { get; set; }

        public bool Outgoing { get; set; }
    }

    public class PhoneCallRequestedEventArgs
    {
        public TLPhoneCallRequested64 RequestedCall { get; set; }
    }

    public class PhoneCallStateChangedEventArgs
    {
        public TLPhoneCallBase Call { get; set; }

        public PhoneCallState CallState { get; set; }
    }

    public class SignalBarsChangedEventArgs
    {
        public int Signal { get; set; }
    }

    public enum PhoneCallSound
    {
        Call,
        Ringback,
        Failed,
        End,
        Busy,
        Connecting
    }

    public enum PhoneCallState
    {
        STATE_WAIT_INIT = 1,
	    STATE_WAIT_INIT_ACK = 2,
	    STATE_ESTABLISHED = 3,
	    STATE_FAILED = 4,
	    STATE_HANGING_UP = 5,
	    STATE_ENDED = 6,
	    STATE_EXCHANGING_KEYS = 7,
	    STATE_WAITING = 8,
	    STATE_REQUESTING = 9,
	    STATE_WAITING_INCOMING = 10,
	    STATE_RINGING = 11,
	    STATE_BUSY = 12,
    }

    public enum PhoneDiscardReason
    {
        DISCARD_REASON_HANGUP = 1,
	    DISCARD_REASON_DISCONNECT = 2,
	    DISCARD_REASON_MISSED = 3,
	    DISCARD_REASON_LINE_BUSY = 4,
    }

    public class EncryptionKeyEmojifier
    {
        private static readonly string[] _emojis =
        {
            "\uD83D\uDE09","\uD83D\uDE0D","\uD83D\uDE1B","\uD83D\uDE2D","\uD83D\uDE31","\uD83D\uDE21","\uD83D\uDE0E","\uD83D\uDE34",
            "\uD83D\uDE35","\uD83D\uDE08","\uD83D\uDE2C","\uD83D\uDE07","\uD83D\uDE0F","\uD83D\uDC6E","\uD83D\uDC77","\uD83D\uDC82","\uD83D\uDC76","\uD83D\uDC68",
            "\uD83D\uDC69","\uD83D\uDC74","\uD83D\uDC75","\uD83D\uDE3B","\uD83D\uDE3D","\uD83D\uDE40","\uD83D\uDC7A","\uD83D\uDE48","\uD83D\uDE49","\uD83D\uDE4A",
            "\uD83D\uDC80","\uD83D\uDC7D","\uD83D\uDCA9","\uD83D\uDD25","\uD83D\uDCA5","\uD83D\uDCA4","\uD83D\uDC42","\uD83D\uDC40","\uD83D\uDC43","\uD83D\uDC45",
            "\uD83D\uDC44","\uD83D\uDC4D","\uD83D\uDC4E","\uD83D\uDC4C","\uD83D\uDC4A","✌","✋","\uD83D\uDC50","\uD83D\uDC46","\uD83D\uDC47","\uD83D\uDC49",
            "\uD83D\uDC48","\uD83D\uDE4F","\uD83D\uDC4F","\uD83D\uDCAA","\uD83D\uDEB6","\uD83C\uDFC3","\uD83D\uDC83","\uD83D\uDC6B","\uD83D\uDC6A","\uD83D\uDC6C",
            "\uD83D\uDC6D","\uD83D\uDC85","\uD83C\uDFA9","\uD83D\uDC51","\uD83D\uDC52","\uD83D\uDC5F","\uD83D\uDC5E","\uD83D\uDC60","\uD83D\uDC55","\uD83D\uDC57",
            "\uD83D\uDC56","\uD83D\uDC59","\uD83D\uDC5C","\uD83D\uDC53","\uD83C\uDF80","\uD83D\uDC84","\uD83D\uDC9B","\uD83D\uDC99","\uD83D\uDC9C","\uD83D\uDC9A",
            "\uD83D\uDC8D","\uD83D\uDC8E","\uD83D\uDC36","\uD83D\uDC3A","\uD83D\uDC31","\uD83D\uDC2D","\uD83D\uDC39","\uD83D\uDC30","\uD83D\uDC38","\uD83D\uDC2F",
            "\uD83D\uDC28","\uD83D\uDC3B","\uD83D\uDC37","\uD83D\uDC2E","\uD83D\uDC17","\uD83D\uDC34","\uD83D\uDC11","\uD83D\uDC18","\uD83D\uDC3C","\uD83D\uDC27",
            "\uD83D\uDC25","\uD83D\uDC14","\uD83D\uDC0D","\uD83D\uDC22","\uD83D\uDC1B","\uD83D\uDC1D","\uD83D\uDC1C","\uD83D\uDC1E","\uD83D\uDC0C","\uD83D\uDC19",
            "\uD83D\uDC1A","\uD83D\uDC1F","\uD83D\uDC2C","\uD83D\uDC0B","\uD83D\uDC10","\uD83D\uDC0A","\uD83D\uDC2B","\uD83C\uDF40","\uD83C\uDF39","\uD83C\uDF3B",
            "\uD83C\uDF41","\uD83C\uDF3E","\uD83C\uDF44","\uD83C\uDF35","\uD83C\uDF34","\uD83C\uDF33","\uD83C\uDF1E","\uD83C\uDF1A","\uD83C\uDF19","\uD83C\uDF0E",
            "\uD83C\uDF0B","⚡","☔","❄","⛄","\uD83C\uDF00","\uD83C\uDF08","\uD83C\uDF0A","\uD83C\uDF93","\uD83C\uDF86","\uD83C\uDF83","\uD83D\uDC7B","\uD83C\uDF85",
            "\uD83C\uDF84","\uD83C\uDF81","\uD83C\uDF88","\uD83D\uDD2E","\uD83C\uDFA5","\uD83D\uDCF7","\uD83D\uDCBF","\uD83D\uDCBB","☎","\uD83D\uDCE1","\uD83D\uDCFA",
            "\uD83D\uDCFB","\uD83D\uDD09","\uD83D\uDD14","⏳","⏰","⌚","\uD83D\uDD12","\uD83D\uDD11","\uD83D\uDD0E","\uD83D\uDCA1","\uD83D\uDD26","\uD83D\uDD0C",
            "\uD83D\uDD0B","\uD83D\uDEBF","\uD83D\uDEBD","\uD83D\uDD27","\uD83D\uDD28","\uD83D\uDEAA","\uD83D\uDEAC","\uD83D\uDCA3","\uD83D\uDD2B","\uD83D\uDD2A",
            "\uD83D\uDC8A","\uD83D\uDC89","\uD83D\uDCB0","\uD83D\uDCB5","\uD83D\uDCB3","✉","\uD83D\uDCEB","\uD83D\uDCE6","\uD83D\uDCC5","\uD83D\uDCC1","✂","\uD83D\uDCCC",
            "\uD83D\uDCCE","✒","✏","\uD83D\uDCD0","\uD83D\uDCDA","\uD83D\uDD2C","\uD83D\uDD2D","\uD83C\uDFA8","\uD83C\uDFAC","\uD83C\uDFA4","\uD83C\uDFA7","\uD83C\uDFB5",
            "\uD83C\uDFB9","\uD83C\uDFBB","\uD83C\uDFBA","\uD83C\uDFB8","\uD83D\uDC7E","\uD83C\uDFAE","\uD83C\uDCCF","\uD83C\uDFB2","\uD83C\uDFAF","\uD83C\uDFC8",
            "\uD83C\uDFC0","⚽","⚾","\uD83C\uDFBE","\uD83C\uDFB1","\uD83C\uDFC9","\uD83C\uDFB3","\uD83C\uDFC1","\uD83C\uDFC7","\uD83C\uDFC6","\uD83C\uDFCA","\uD83C\uDFC4",
            "☕","\uD83C\uDF7C","\uD83C\uDF7A","\uD83C\uDF77","\uD83C\uDF74","\uD83C\uDF55","\uD83C\uDF54","\uD83C\uDF5F","\uD83C\uDF57","\uD83C\uDF71","\uD83C\uDF5A",
            "\uD83C\uDF5C","\uD83C\uDF61","\uD83C\uDF73","\uD83C\uDF5E","\uD83C\uDF69","\uD83C\uDF66","\uD83C\uDF82","\uD83C\uDF70","\uD83C\uDF6A","\uD83C\uDF6B",
            "\uD83C\uDF6D","\uD83C\uDF6F","\uD83C\uDF4E","\uD83C\uDF4F","\uD83C\uDF4A","\uD83C\uDF4B","\uD83C\uDF52","\uD83C\uDF47","\uD83C\uDF49","\uD83C\uDF53",
            "\uD83C\uDF51","\uD83C\uDF4C","\uD83C\uDF50","\uD83C\uDF4D","\uD83C\uDF46","\uD83C\uDF45","\uD83C\uDF3D","\uD83C\uDFE1","\uD83C\uDFE5","\uD83C\uDFE6",
            "⛪","\uD83C\uDFF0","⛺","\uD83C\uDFED","\uD83D\uDDFB","\uD83D\uDDFD","\uD83C\uDFA0","\uD83C\uDFA1","⛲","\uD83C\uDFA2","\uD83D\uDEA2","\uD83D\uDEA4",
            "⚓","\uD83D\uDE80","✈","\uD83D\uDE81","\uD83D\uDE82","\uD83D\uDE8B","\uD83D\uDE8E","\uD83D\uDE8C","\uD83D\uDE99","\uD83D\uDE97","\uD83D\uDE95","\uD83D\uDE9B",
            "\uD83D\uDEA8","\uD83D\uDE94","\uD83D\uDE92","\uD83D\uDE91","\uD83D\uDEB2","\uD83D\uDEA0","\uD83D\uDE9C","\uD83D\uDEA6","⚠","\uD83D\uDEA7","⛽","\uD83C\uDFB0",
            "\uD83D\uDDFF","\uD83C\uDFAA","\uD83C\uDFAD","\uD83C\uDDEF\uD83C\uDDF5","\uD83C\uDDF0\uD83C\uDDF7","\uD83C\uDDE9\uD83C\uDDEA","\uD83C\uDDE8\uD83C\uDDF3",
            "\uD83C\uDDFA\uD83C\uDDF8","\uD83C\uDDEB\uD83C\uDDF7","\uD83C\uDDEA\uD83C\uDDF8","\uD83C\uDDEE\uD83C\uDDF9","\uD83C\uDDF7\uD83C\uDDFA","\uD83C\uDDEC\uD83C\uDDE7",
            "1⃣","2⃣","3⃣","4⃣","5⃣","6⃣","7⃣","8⃣","9⃣","0⃣","\uD83D\uDD1F","❗","❓","♥","♦","\uD83D\uDCAF","\uD83D\uDD17","\uD83D\uDD31","\uD83D\uDD34",
            "\uD83D\uDD35","\uD83D\uDD36","\uD83D\uDD37"
        };

        private static readonly int[] _offsets = { 0, 4, 8, 12, 16 };

        private static int BytesToInt(byte[] arr, int offset)
        {
            return (((int)arr[offset] & 0x7F) << 24) | (((int)arr[offset + 1] & 0xFF) << 16) | (((int)arr[offset + 2] & 0xFF) << 8) | ((int)arr[offset + 3] & 0xFF);
        }

        private static long BytesToLong(byte[] arr, int offset)
        {
            return (((long)arr[offset] & 0x7F) << 56) | (((long)arr[offset + 1] & 0xFF) << 48) | (((long)arr[offset + 2] & 0xFF) << 40) | (((long)arr[offset + 3] & 0xFF) << 32) |
            (((long)arr[offset + 4] & 0xFF) << 24) | (((long)arr[offset + 5] & 0xFF) << 16) | (((long)arr[offset + 6] & 0xFF) << 8) | (((long)arr[offset + 7] & 0xFF));

        }

        public static String[] Emojify(byte[] sha256)
        {
            if (sha256.Length != 32)
            {
                throw new ArgumentException("sha256 needs to be exactly 32 bytes", "sha256");
            }

            var result = new string[5];
            for (int i = 0; i < 5; i++)
            {
                result[i] = _emojis[BytesToInt(sha256, _offsets[i]) % _emojis.Length];
            }

            return result;
        }

        public static String[] EmojifyForCall(byte[] sha256)
        {
            var result = new string[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = _emojis[(int)(BytesToLong(sha256, 8 * i) % _emojis.Length)];
            }

            return result;
        }
    }
}
