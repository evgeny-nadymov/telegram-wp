// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.TL;

namespace Telegram.Api.Services.Location
{
    public class LiveLocationService : ILiveLocationService, IHandle<TLUpdateEditMessage>, IHandle<TLUpdateEditChannelMessage>
    {
        private readonly TLVector<TLMessageBase> _messages;

        private readonly IMTProtoService _mtProtoService;

        private readonly ITelegramEventAggregator _eventAggregator;

        private readonly object _liveLocationsSyncRoot = new object();

        private readonly Timer _timer;

        public LiveLocationService(IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
        {
            _timer = new Timer(OnTick);
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _mtProtoService = mtProtoService;
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe(this);

            _messages = new TLVector<TLMessageBase>();
        }

        private void OnTick(object state)
        {
            UpdateAll();

            SetNextTimer();
        }

        public void LoadAndUpdateAllAsync()
        {
            Execute.BeginOnThreadPool(() =>
            {
                Load();

                UpdateAll();

                SetNextTimer();
            });
        }

        public void StopAllAsync()
        {
            Execute.BeginOnThreadPool(() =>
            {
                lock (_liveLocationsSyncRoot)
                {
                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var message = _messages[i] as TLMessage70;
                        if (message != null)
                        {
                            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                            if (mediaGeoLive != null)
                            {
                                mediaGeoLive.Date = message.Date;
                                mediaGeoLive.EditDate = message.EditDate;
                                if (!mediaGeoLive.Active)
                                {
                                    _messages.RemoveAt(i--);
                                }
                            }
                        }
                    }
                }

                TLUtils.SaveObjectToMTProtoFile(_liveLocationsSyncRoot, Constants.LiveLocationsFileName, _messages);

                var messages = new List<TLMessage70>();
                lock (_liveLocationsSyncRoot)
                {
                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var message = _messages[i] as TLMessage70;
                        if (message != null)
                        {
                            messages.Add(message);
                        }
                    }
                }

                if (messages.Count > 0)
                {
                    var handles = new List<WaitHandle>();

                    foreach (var message in messages)
                    {
                        var m = message;
                        var waitHandle = new ManualResetEvent(false);
                        handles.Add(waitHandle);
                        UpdateAsync(m,
                            new TLGeoPointEmpty(),
                            result =>
                            {
                                waitHandle.Set();
                            },
                            error =>
                            {
                                waitHandle.Set();
                            });
                    }

#if DEBUG
                    var timeout = Timeout.InfiniteTimeSpan;
#else
                var timeout = TimeSpan.FromSeconds(30.0);
#endif

                    var noTimeout = WaitHandle.WaitAll(handles.ToArray(), timeout);

                    if (noTimeout)
                    {
                        lock (_liveLocationsSyncRoot)
                        {
                            _messages.Clear();
                        }

                        TLUtils.SaveObjectToMTProtoFile(_liveLocationsSyncRoot, Constants.LiveLocationsFileName, _messages);

                        _eventAggregator.Publish(new LiveLocationClearedEventArgs());
                    }
                }
                else
                {
                    lock (_liveLocationsSyncRoot)
                    {
                        _messages.Clear();
                    }

                    TLUtils.SaveObjectToMTProtoFile(_liveLocationsSyncRoot, Constants.LiveLocationsFileName, _messages);

                    _eventAggregator.Publish(new LiveLocationClearedEventArgs());
                }
            });
        }

        public void UpdateAll()
        {
            var removedMessages = new List<TLMessage>();
            lock (_liveLocationsSyncRoot)
            {
                for (var i = 0; i < _messages.Count; i++)
                {
                    var message = _messages[i] as TLMessage70;
                    if (message != null)
                    {
                        var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null)
                        {
                            mediaGeoLive.Date = message.Date;
                            mediaGeoLive.EditDate = message.EditDate;
                            if (!mediaGeoLive.Active)
                            {
                                removedMessages.Add(message);
                                _messages.RemoveAt(i--);
                            }
                        }
                    }
                }
            }

            if (removedMessages.Count > 0)
            {
                _eventAggregator.Publish(new LiveLocationRemovedEventArgs { Messages = removedMessages });
            }

            TLUtils.SaveObjectToMTProtoFile(_liveLocationsSyncRoot, Constants.LiveLocationsFileName, _messages);

            var messages = new List<TLMessage70>();
            lock (_liveLocationsSyncRoot)
            {
                for (var i = 0; i < _messages.Count; i++)
                {
                    var message = _messages[i] as TLMessage70;
                    if (message != null)
                    {
                        var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null && mediaGeoLive.Active)
                        {
                            var period = mediaGeoLive.Period.Value > 3600 ? 180 : 90;

                            var clientDelta = _mtProtoService.ClientTicksDelta;
                            //var utc0SecsLong = message.EditDate.Value * 4294967296 - clientDelta;
                            var utc0SecsInt = message.EditDate.Value - clientDelta / 4294967296.0;

                            var nextTime = Utils.UnixTimestampToDateTime(utc0SecsInt + period);

                            if (nextTime <= DateTime.Now.AddSeconds(30.0))
                            {
                                messages.Add(message);
                            }
                        }
                    }
                }
            }

            if (messages.Count > 0)
            {
                GeoCoordinate location;
                using (var coordinateWatcher = new GeoCoordinateWatcher())
                {
                    coordinateWatcher.TryStart(false, TimeSpan.FromMilliseconds(1000));

                    location = coordinateWatcher.Position.Location;

                    coordinateWatcher.Stop();
                }

                if (!location.IsUnknown)
                {
                    var handles = new List<WaitHandle>();

                    foreach (var message in messages)
                    {
                        var m = message;
                        var waitHandle = new ManualResetEvent(false);
                        handles.Add(waitHandle);
                        UpdateAsync(m,
                            new TLGeoPoint
                            {
                                Lat = new TLDouble(location.Latitude),
                                Long = new TLDouble(location.Longitude)
                            },
                            result =>
                            {
                                waitHandle.Set();
                            },
                            error =>
                            {
                                waitHandle.Set();
                            });
                    }

#if DEBUG
                    var timeout = Timeout.InfiniteTimeSpan;
#else
                    var timeout = TimeSpan.FromSeconds(30.0);
#endif


                    var noTimeout = WaitHandle.WaitAll(handles.ToArray(), timeout);
                }
            }
        }

        private void SetNextTimer()
        {
            var timeSpan = GetTimeSpan();
            _timer.Change(timeSpan, Timeout.InfiniteTimeSpan);
        }

        public IList<TLMessage> Get()
        {
            var list = new List<TLMessage>();

            lock (_liveLocationsSyncRoot)
            {
                for (var index = _messages.Count - 1; index >= 0; index--)
                {
                    var messageBase = _messages[index];
                    var message = messageBase as TLMessage48;
                    if (message != null)
                    {
                        var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null)
                        {
                            mediaGeoLive.EditDate = message.EditDate;
                            mediaGeoLive.Date = message.Date;
                            if (mediaGeoLive.Active)
                            {
                                list.Add(message);
                            }
                        }
                    }
                }
            }

            return list;
        }

        public TLMessage Get(TLPeerBase peer, TLInt fromId)
        {
            try
            {
                lock (_liveLocationsSyncRoot)
                {
                    for (var index = _messages.Count - 1; index >= 0; index--)
                    {
                        var messageBase = _messages[index];
                        var message = messageBase as TLMessage48;
                        if (message != null
                            && message.FromId.Value == fromId.Value)
                        {
                            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                            if (mediaGeoLive != null)
                            {
                                mediaGeoLive.EditDate = message.EditDate;
                                mediaGeoLive.Date = message.Date;
                                if (mediaGeoLive.Active)
                                {
                                    if (peer is TLPeerUser
                                        && message.ToId is TLPeerUser
                                        && !message.Out.Value
                                        && peer.Id.Value == message.FromId.Value)
                                    {
                                        return message;
                                    }

                                    if (peer is TLPeerUser
                                        && message.ToId is TLPeerUser
                                        && message.Out.Value
                                        && peer.Id.Value == message.ToId.Id.Value)
                                    {
                                        return message;
                                    }

                                    if (peer is TLPeerChat
                                        && message.ToId is TLPeerChat
                                        && peer.Id.Value == message.ToId.Id.Value)
                                    {
                                        return message;
                                    }

                                    if (peer is TLPeerChannel
                                        && message.ToId is TLPeerChannel
                                        && peer.Id.Value == message.ToId.Id.Value)
                                    {
                                        return message;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public void Load()
        {
            if (_messages == null) return;

            var messages = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLMessageBase>>(_liveLocationsSyncRoot, Constants.LiveLocationsFileName) ?? new TLVector<TLMessageBase>();

            var m = new List<TLMessage>();
            lock (_liveLocationsSyncRoot)
            {
                _messages.Clear();
                foreach (var messageBase in messages)
                {
                    var message = messageBase as TLMessage70;
                    if (message != null)
                    {
                        if (message.InputPeer == null)
                        {
                            return;
                        }

                        var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null)
                        {
                            mediaGeoLive.EditDate = message.EditDate;
                            mediaGeoLive.Date = message.Date;
                            if (!mediaGeoLive.Active)
                            {
                                continue;
                            }
                        }

                        _messages.Add(messageBase);
                        m.Add(message);
                    }
                }
            }

            _eventAggregator.Publish(new LiveLocationLoadedEventArgs { Messages = m });
        }

        public void Clear()
        {
            lock (_liveLocationsSyncRoot)
            {
                _messages.Clear();
            }

            FileUtils.Delete(_liveLocationsSyncRoot, Constants.LiveLocationsFileName);

            _eventAggregator.Publish(new LiveLocationClearedEventArgs());
        }

        private TimeSpan GetTimeSpan()
        {
            var timeSpan = Timeout.InfiniteTimeSpan;
            lock (_liveLocationsSyncRoot)
            {
                for (var i = 0; i < _messages.Count; i++)
                {
                    var message = _messages[i] as TLMessage70;
                    if (message != null)
                    {
                        var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null && mediaGeoLive.Active)
                        {
                            var period = mediaGeoLive.Period.Value > 3600 ? 180 : 90;

                            var editDate = message.EditDate != null && message.EditDate.Value > message.Date.Value
                                ? message.EditDate.Value
                                : message.Date.Value;

                            // edit date + update interval
                            var clientDelta = _mtProtoService.ClientTicksDelta;
                            //var utc0SecsLong = editDate * 4294967296 - clientDelta;
                            var utc0SecsInt = editDate - clientDelta / 4294967296.0;

                            var nextEditDate1 = Utils.UnixTimestampToDateTime(utc0SecsInt + period);
                            var timeSpan1 = nextEditDate1 - DateTime.Now;
                            if (timeSpan1.Ticks < 0) timeSpan1 = new TimeSpan(0);

                            // date + period
                            //utc0SecsLong = message.Date.Value * 4294967296 - clientDelta;
                            utc0SecsInt = message.Date.Value - clientDelta / 4294967296.0;

                            var nextEditDate2 = Utils.UnixTimestampToDateTime(utc0SecsInt + mediaGeoLive.Period.Value);
                            var timeSpan2 = nextEditDate2 - DateTime.Now;
                            if (timeSpan2.Ticks < 0) timeSpan2 = new TimeSpan(0);

                            var currentTimeSpan = timeSpan1.Ticks < timeSpan2.Ticks ? timeSpan1 : timeSpan2;
                            if (currentTimeSpan < timeSpan || timeSpan == Timeout.InfiniteTimeSpan)
                            {
                                timeSpan = currentTimeSpan;
                            }
                        }
                    }
                }
            }

            return timeSpan;
        }

        public void Add(TLMessage70 message)
        {
            message.InputPeer = _mtProtoService.PeerToInputPeer(message.ToId);

            lock (_liveLocationsSyncRoot)
            {
                _messages.Add(message);
            }

            SetNextTimer();

            TLUtils.SaveObjectToMTProtoFile(_liveLocationsSyncRoot, Constants.LiveLocationsFileName, _messages);

            _eventAggregator.Publish(new LiveLocationAddedEventArgs { Message = message });
        }

        public void Remove(TLMessage message)
        {
            lock (_liveLocationsSyncRoot)
            {
                _messages.Remove(message);
            }

            TLUtils.SaveObjectToMTProtoFile(_liveLocationsSyncRoot, Constants.LiveLocationsFileName, _messages);

            SetNextTimer();

            _eventAggregator.Publish(new LiveLocationRemovedEventArgs { Messages = new List<TLMessage> { message } });
        }

        public void UpdateAsync(TLMessage70 message, TLGeoPointBase geoPointBase, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null)
        {
            TLMessage70 m;
            lock (_liveLocationsSyncRoot)
            {
                m = _messages.FirstOrDefault(
                    x => x.Index == message.Index
                    && ((TLMessage)x).ToId.GetType() == message.ToId.GetType()
                    && ((TLMessage)x).ToId.Id.Value == message.ToId.Id.Value) as TLMessage70;
            }
            if (m == null || m.InputPeer == null)
            {
                faultCallback.SafeInvoke(null);
                return;
            };

            var stopGeoLive = false;
            TLInputGeoPoint inputGeoPoint = null;
            var geoPoint = geoPointBase as TLGeoPoint;
            if (geoPoint != null)
            {
                inputGeoPoint = new TLInputGeoPoint { Lat = geoPoint.Lat, Long = geoPoint.Long };
            }
            else
            {
                stopGeoLive = true;
            }

            _mtProtoService.EditMessageAsync(m.InputPeer, message.Id, null, null, null, null, false, stopGeoLive, inputGeoPoint,
                result =>
                {
                    m.EditDate = TLUtils.DateToUniversalTimeTLInt(_mtProtoService.ClientTicksDelta, DateTime.Now);
                    var mediaGeoLive = m.Media as TLMessageMediaGeoLive;
                    if (mediaGeoLive != null)
                    {
                        mediaGeoLive.EditDate = m.EditDate;
                    }

                    if (stopGeoLive)
                    {
                        Remove(m);
                    }

                    callback.SafeInvoke(result);
                },
                error =>
                {
                    // handle 400 MESSAGE_EDIT_TIME_EXPIRED, MESSAGE_ID_INVALID, ...
                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        if (error.TypeEquals(ErrorType.MESSAGE_NOT_MODIFIED))
                        {
                            m.EditDate = TLUtils.DateToUniversalTimeTLInt(_mtProtoService.ClientTicksDelta, DateTime.Now);
                            var mediaGeoLive = m.Media as TLMessageMediaGeoLive;
                            if (mediaGeoLive != null)
                            {
                                mediaGeoLive.EditDate = m.EditDate;
                            }
                        }
                        else
                        {
                            Remove(m);
                        }
                    }

                    faultCallback.SafeInvoke(error);
                    Execute.ShowDebugMessage("messages.editMessage error " + error);
                });
        }

        private void UpdateAndRemoveAt(int i, TLMessage message, TLMessage updatedMessage)
        {
            message.Update(updatedMessage);

            var messageMediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (messageMediaGeoLive != null && !messageMediaGeoLive.Active)
            {
                _messages.RemoveAt(i);

                TLUtils.SaveObjectToMTProtoFile(_liveLocationsSyncRoot, Constants.LiveLocationsFileName, _messages);

                SetNextTimer();

                _eventAggregator.Publish(new LiveLocationRemovedEventArgs { Messages = new List<TLMessage> { message } });
            }
        }

        public void Handle(TLUpdateEditMessage update)
        {
            var updatedMessage = update.Message as TLMessage;
            if (updatedMessage != null)
            {
                lock (_liveLocationsSyncRoot)
                {
                    if (_messages == null)
                    {
                        Load();
                    }

                    if (_messages == null) return;

                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var message = _messages[i] as TLMessage;
                        if (message != null && message.Index == updatedMessage.Index)
                        {
                            var peer = updatedMessage.ToId;
                            if (peer is TLPeerUser
                                && message.ToId is TLPeerUser
                                && !message.Out.Value
                                && peer.Id.Value == message.FromId.Value)
                            {
                                UpdateAndRemoveAt(i, message, updatedMessage);
                                return;
                            }

                            if (peer is TLPeerUser
                                && message.ToId is TLPeerUser
                                && message.Out.Value
                                && peer.Id.Value == message.ToId.Id.Value)
                            {
                                UpdateAndRemoveAt(i, message, updatedMessage);
                                return;
                            }

                            if (peer is TLPeerChat
                                && message.ToId is TLPeerChat
                                && peer.Id.Value == message.ToId.Id.Value)
                            {
                                UpdateAndRemoveAt(i, message, updatedMessage);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void Handle(TLUpdateEditChannelMessage update)
        {
            var updatedMessage = update.Message as TLMessage;
            if (updatedMessage != null)
            {
                lock (_liveLocationsSyncRoot)
                {
                    if (_messages == null)
                    {
                        Load();
                    }

                    if (_messages == null) return;

                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var message = _messages[i] as TLMessage;
                        if (message != null && message.Index == updatedMessage.Index)
                        {
                            var peer = updatedMessage.ToId;
                            if (peer is TLPeerChannel
                                && message.ToId is TLPeerChannel
                                && peer.Id.Value == message.ToId.Id.Value)
                            {
                                UpdateAndRemoveAt(i, message, updatedMessage);

                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    public class LiveLocationAddedEventArgs
    {
        public TLMessage Message { get; set; }
    }

    public class LiveLocationRemovedEventArgs
    {
        public IList<TLMessage> Messages { get; set; }
    }

    public class LiveLocationClearedEventArgs
    {

    }

    public class LiveLocationLoadedEventArgs
    {
        public IList<TLMessage> Messages { get; set; }
    }
}
