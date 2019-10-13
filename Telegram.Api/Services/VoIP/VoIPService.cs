using System;
using System.Text;
using Windows.Storage;
#if WINDOWS_PHONE
using libtgvoip;
#endif
using Org.BouncyCastle.Security;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;

namespace Telegram.Api.Services.VoIP
{
    public class VoIPService : IVoIPService, IHandle<TLUpdatePhoneCall>
#if WINDOWS_PHONE
        , IStateCallback
#endif
    {
        private const int CALL_MIN_LAYER = 65;
        private const int CALL_MAX_LAYER = 65;

        private readonly IMTProtoService _mtProtoService;

        private readonly ITelegramEventAggregator _eventAggregator;

        private readonly ICacheService _cacheService;

        public VoIPService(IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator, ICacheService cacheService)
        {
            _mtProtoService = mtProtoService;
            _eventAggregator = eventAggregator;
            _cacheService = cacheService;

            _eventAggregator.Subscribe(this);

#if WINDOWS_PHONE
            MicrosoftCryptoImpl.Init();

            var input = Encoding.UTF8.GetBytes("test");
            var result1 = MicrosoftCryptoImpl.SHA1_test(input, (uint) input.Length);
            var result2 = Utils.ComputeSHA1(input);
            for (int i = 0; i < result1.Length; i++)
            {
                if (result1[i] != result2[i])
                {
                    throw new Exception("sha1 i=" + i);
                }
            }
            var result3 = MicrosoftCryptoImpl.SHA256_test(input, (uint)input.Length);
            var result4 = Utils.ComputeSHA256(input);
            for (int i = 0; i < result1.Length; i++)
            {
                if (result3[i] != result4[i])
                {
                    throw new Exception("sha1 i=" + i);
                }
            }
            var keyString = "01234567890123456789012345678901";
            var ivString = "01234567890123456789012345678901";
            var key = Encoding.UTF8.GetBytes(keyString);
            var iv = Encoding.UTF8.GetBytes(ivString);
            var result5 = MicrosoftCryptoImpl.AesIgeEncrypt_test(input, (uint)input.Length, key, iv);
            key = Encoding.UTF8.GetBytes(keyString);
            iv = Encoding.UTF8.GetBytes(ivString);
            var result6 = MicrosoftCryptoImpl.AesIgeDecrypt_test(result5, (uint)input.Length, key, iv);
            key = Encoding.UTF8.GetBytes(keyString);
            iv = Encoding.UTF8.GetBytes(ivString);
            var result7 = Utils.AesIge(input, key, iv, true);
            var result8 = Utils.AesIge(result7, key, iv, false);
#endif
        }

        private TLString _secretP;

        private TLString _secretRandom;

        private TLInt _secretG;

        private TLInt _lastVersion;

        private byte[] _ga;

        private TLPhonePhoneCall _call;
        private byte[] _aOrB;
        private byte[] _authKey;
        private bool _outgoing = true;
#if WINDOWS_PHONE
        private VoIPControllerWrapper _controller;
#endif

        public void StartOutgoingCall(TLInputUserBase userId)
        {
            var salt = new Byte[256];
            var random = new SecureRandom();
            random.NextBytes(salt);

            var version = _lastVersion ?? new TLInt(0);
            var randomLength = new TLInt(256);

            _mtProtoService.GetDHConfigAsync(version, randomLength,
                result =>
                {
                    ConfigureDeviceForCall();
                    ShowNotifications();
                    StartConnectionSound();
                    DispatchStateChanged(PhoneCallState.STATE_REQUESTING);

                    _eventAggregator.Publish(new PhoneCallEventArgs("NotificationCenter.didStartedCall"));

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
                    var gaHash = Utils.ComputeSHA256(_ga);

                    _mtProtoService.RequestCallAsync(userId, TLInt.Random(), TLString.FromBigEndianData(gaHash), protocol,
                        result2 =>
                        {
                            _call = result2;
                            _aOrB = salt;
                            DispatchStateChanged(PhoneCallState.STATE_WAITING);
                            //if (_endCallAfterRequest)
                            //{
                            //    Hangup();
                            //    return;
                            //}
                        },
                        error2 =>
                        {
                            
                        });
                },
                error =>
                {
                    Helpers.Execute.ShowDebugMessage("messages.getDHConfig error " + error);
                    CallFailed();
                });

        }

        private void DispatchStateChanged(PhoneCallState phoneCallState)
        {
            
        }

        private void StartConnectionSound()
        {
            
        }

        private void ShowNotifications()
        {
            

        }

        private void ConfigureDeviceForCall()
        {
            
        }

        private void CallFailed()
        {
            
        }

        public void Handle(TLUpdatePhoneCall updatePhoneCall)
        {
            var phoneCall = updatePhoneCall.PhoneCall;
            var phoneCallAccepted = phoneCall as TLPhoneCallAccepted;
            if (phoneCallAccepted != null)
            {
                if (_authKey == null)
                {
                    ProcessAcceptedCall(phoneCallAccepted);
                }

                return;
            }

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
            var keyHash = Utils.ComputeSHA1(_authKey); 
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
                    _call = result;
                    InitiateActualEncryptedCall();
                },
                error =>
                {
                    CallFailed();
                });

        }

        private void InitiateActualEncryptedCall()
        {
#if WINDOWS_PHONE

            _mtProtoService.GetCallConfigAsync(result =>
            {
                VoIPControllerWrapper.UpdateServerConfig(result.Data.ToString());

                var logFile = ApplicationData.Current.LocalFolder.Path + "\\tgvoip.logFile.txt";
                var statsDumpFile = ApplicationData.Current.LocalFolder.Path + "\\tgvoip.statsDump.txt";


                if (_controller != null)
                {
                    //_controller.Dispose();
                    _controller = null;
                }

                _cacheService.GetConfigAsync(config =>
                {
                    var config60 = config as TLConfig60;
                    if (config60 != null)
                    {
                        _controller = new VoIPControllerWrapper();
                        _controller.SetConfig(config60.CallPacketTimeoutMs.Value / 1000.0, config60.CallConnectTimeoutMs.Value / 1000.0, DataSavingMode.Never, false, false, true, logFile, statsDumpFile);

                        _controller.SetStateCallback(this);
                        _controller.SetEncryptionKey(_authKey, _outgoing);

                        var phoneCall = _call.PhoneCall as TLPhoneCall;
                        if (phoneCall != null)
                        {
                            var connection = phoneCall.Connection;
                            var endpoints = new Endpoint[phoneCall.AlternativeConnections.Count + 1];
                            endpoints[0] = connection.ToEndpoint();

                            for (int i = 0; i < phoneCall.AlternativeConnections.Count; i++)
                            {
                                connection = phoneCall.AlternativeConnections[i];
                                endpoints[i + 1] = connection.ToEndpoint();
                            }

                            _controller.SetPublicEndpoints(endpoints, phoneCall.Protocol.UdpP2P);
                            _controller.Start();
                            _controller.Connect();
                        }
                    }
                });

            },
            error =>
            {
                    
            });

#endif
        }

#if WINDOWS_PHONE
        public void OnCallStateChanged(CallState newState)
        {
            Execute.ShowDebugMessage("OnCallStateChanged state=" + newState);
        }
#endif
    }

    public class PhoneCallEventArgs
    {
        public string Param { get; set; }

        public PhoneCallEventArgs(string param)
        {
            Param = param;
        }
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
}
