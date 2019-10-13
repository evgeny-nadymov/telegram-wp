// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLDifferenceBase : TLObject
    {
        public abstract TLDifferenceBase GetEmptyObject();
    }

    public class TLDifferenceEmpty : TLDifferenceBase
    {
        public const uint Signature = TLConstructors.TLDifferenceEmpty;

        public TLInt Date { get; set; }

        public TLInt Seq { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Seq = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Date.ToStream(output);
            Seq.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Date = GetObject<TLInt>(input);
            Seq = GetObject<TLInt>(input);

            return this;
        }

        public override TLDifferenceBase GetEmptyObject()
        {
            return new TLDifferenceEmpty { Date = Date, Seq = Seq };
        }

        public override string ToString()
        {
            return string.Format("TLDifferenceEmpty date={0} seq={1}", Date, Seq);
        }
    }

    public class TLDifference : TLDifferenceBase
    {
        public const uint Signature = TLConstructors.TLDifference;

        public TLVector<TLMessageBase> NewMessages { get; set; }
        public TLVector<TLEncryptedMessageBase> NewEncryptedMessages { get; set; }
        public TLVector<TLUpdateBase> OtherUpdates { get; set; }
        public TLVector<TLUserBase> Users { get; set; }
        public TLVector<TLChatBase> Chats { get; set; }
        public TLState State { get; set; }

        public override TLDifferenceBase GetEmptyObject()
        {
            return new TLDifference
            {
                NewMessages = new TLVector<TLMessageBase>(NewMessages.Count),
                NewEncryptedMessages = new TLVector<TLEncryptedMessageBase>(NewEncryptedMessages.Count),
                OtherUpdates = new TLVector<TLUpdateBase>(OtherUpdates.Count),
                Users = new TLVector<TLUserBase>(Users.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                State = State
            };
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NewMessages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            NewEncryptedMessages = GetObject<TLVector<TLEncryptedMessageBase>>(bytes, ref position);
            OtherUpdates = GetObject<TLVector<TLUpdateBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);
            State = GetObject<TLState>(bytes, ref position);

            ProcessReading();

            return this;
        }

        protected void ProcessReading()
        {
            var readUserInbox = new Dictionary<int, TLUpdateReadHistory>();
            var readUserOutbox = new Dictionary<int, TLUpdateReadHistory>();

            var readChatInbox = new Dictionary<int, TLUpdateReadHistory>();
            var readChatOutbox = new Dictionary<int, TLUpdateReadHistory>();

            var readChannelInbox = new Dictionary<int, TLUpdateReadChannelInbox>();
            var readChannelOutbox = new Dictionary<int, TLUpdateReadChannelOutbox>();

            var newMessages = new List<TLUpdateNewMessage>();
            var newChannelMessages = new List<TLUpdateNewChannelMessage>();

            foreach (var otherUpdate in OtherUpdates)
            {
                var updateNewChannelMessage = otherUpdate as TLUpdateNewChannelMessage;
                if (updateNewChannelMessage != null)
                {
                    newChannelMessages.Add(updateNewChannelMessage);
                    continue;
                }

                var updateNewMessage = otherUpdate as TLUpdateNewMessage;
                if (updateNewMessage != null)
                {
                    newMessages.Add(updateNewMessage);
                    continue;
                }

                var updateReadChannelInbox = otherUpdate as TLUpdateReadChannelInbox;
                if (updateReadChannelInbox != null)
                {
                    readChannelInbox[updateReadChannelInbox.ChannelId.Value] = updateReadChannelInbox;
                    continue;
                }

                var updateReadChannelOutbox = otherUpdate as TLUpdateReadChannelOutbox;
                if (updateReadChannelOutbox != null)
                {
                    readChannelOutbox[updateReadChannelOutbox.ChannelId.Value] = updateReadChannelOutbox;
                    continue;
                }

                var updateReadHistoryInbox = otherUpdate as TLUpdateReadHistoryInbox;
                if (updateReadHistoryInbox != null)
                {
                    if (updateReadHistoryInbox.Peer is TLPeerChat)
                    {
                        readChatInbox[updateReadHistoryInbox.Peer.Id.Value] = updateReadHistoryInbox;
                    }
                    else if (updateReadHistoryInbox.Peer is TLPeerUser)
                    {
                        readUserInbox[updateReadHistoryInbox.Peer.Id.Value] = updateReadHistoryInbox;
                    }
                    continue;
                }

                var updateReadHistoryOutbox = otherUpdate as TLUpdateReadHistoryOutbox;
                if (updateReadHistoryOutbox != null)
                {
                    if (updateReadHistoryOutbox.Peer is TLPeerChat)
                    {
                        readChatOutbox[updateReadHistoryOutbox.Peer.Id.Value] = updateReadHistoryOutbox;
                    }
                    else if (updateReadHistoryOutbox.Peer is TLPeerUser)
                    {
                        readUserOutbox[updateReadHistoryOutbox.Peer.Id.Value] = updateReadHistoryOutbox;
                    }
                    continue;
                }
            }

            for (var i = 0; i < newChannelMessages.Count; i++)
            {
                var messageCommon = newChannelMessages[i].Message as TLMessageCommon;
                if (messageCommon != null)
                {
                    if (IsReadMessage(messageCommon,
                        readChatOutbox, readChatInbox,
                        readUserOutbox, readUserInbox,
                        readChannelOutbox, readChannelInbox))
                    {
                        continue;
                    }

                    messageCommon.SetUnreadSilent(TLBool.True);
                }
            }

            for (var i = 0; i < newMessages.Count; i++)
            {
                var messageCommon = newMessages[i].Message as TLMessageCommon;
                if (messageCommon != null)
                {
                    if (IsReadMessage(messageCommon,
                        readChatOutbox, readChatInbox,
                        readUserOutbox, readUserInbox,
                        readChannelOutbox, readChannelInbox))
                    {
                        continue;
                    }

                    messageCommon.SetUnreadSilent(TLBool.True);
                }
            }

            for (var i = 0; i < NewMessages.Count; i++)
            {
                var messageCommon = NewMessages[i] as TLMessageCommon;
                if (messageCommon != null)
                {
                    if (IsReadMessage(messageCommon, 
                        readChatOutbox, readChatInbox, 
                        readUserOutbox, readUserInbox,
                        readChannelOutbox, readChannelInbox))
                    {
                        continue;
                    }

                    messageCommon.SetUnreadSilent(TLBool.True);
                }
            }
        }

        private static bool IsReadMessage(TLMessageCommon messageCommon, 
            Dictionary<int, TLUpdateReadHistory> readChatOutbox, Dictionary<int, TLUpdateReadHistory> readChatInbox,
            Dictionary<int, TLUpdateReadHistory> readUserOutbox, Dictionary<int, TLUpdateReadHistory> readUserInbox, 
            Dictionary<int, TLUpdateReadChannelOutbox> readChannelOutbox, Dictionary<int, TLUpdateReadChannelInbox> readChannelInbox)
        {
            if (messageCommon.ToId is TLPeerChat)
            {
                if (messageCommon.Out.Value)
                {
                    TLUpdateReadHistory updateReadHistory;
                    if (readChatOutbox.TryGetValue(messageCommon.ToId.Id.Value, out updateReadHistory)
                        && updateReadHistory.MaxId.Value >= messageCommon.Index)
                    {
                        return true;
                    }
                }
                else
                {
                    TLUpdateReadHistory updateReadHistory;
                    if (readChatInbox.TryGetValue(messageCommon.ToId.Id.Value, out updateReadHistory)
                        && updateReadHistory.MaxId.Value >= messageCommon.Index)
                    {
                        return true;
                    }
                }
            }
            else if (messageCommon.ToId is TLPeerUser)
            {
                if (messageCommon.Out.Value)
                {
                    TLUpdateReadHistory updateReadHistory;
                    if (readUserOutbox.TryGetValue(messageCommon.ToId.Id.Value, out updateReadHistory)
                        && updateReadHistory.MaxId.Value >= messageCommon.Index)
                    {
                        return true;
                    }
                }
                else
                {
                    TLUpdateReadHistory updateReadHistory;
                    if (readUserInbox.TryGetValue(messageCommon.FromId.Value, out updateReadHistory)
                        && updateReadHistory.MaxId.Value >= messageCommon.Index)
                    {
                        return true;
                    }
                }
            }
            else if (messageCommon.ToId is TLPeerChannel)
            {
                if (messageCommon.Out.Value)
                {
                    TLUpdateReadChannelOutbox updateReadHistory;
                    if (readChannelOutbox.TryGetValue(messageCommon.ToId.Id.Value, out updateReadHistory)
                        && updateReadHistory.MaxId.Value >= messageCommon.Index)
                    {
                        return true;
                    }
                }
                else
                {
                    TLUpdateReadChannelInbox updateReadHistory;
                    if (readChannelInbox.TryGetValue(messageCommon.ToId.Id.Value, out updateReadHistory)
                        && updateReadHistory.MaxId.Value >= messageCommon.Index)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            NewMessages.ToStream(output);
            NewEncryptedMessages.ToStream(output);
            OtherUpdates.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);
            State.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            NewMessages = GetObject<TLVector<TLMessageBase>>(input);
            NewEncryptedMessages = GetObject<TLVector<TLEncryptedMessageBase>>(input);
            OtherUpdates = GetObject<TLVector<TLUpdateBase>>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);
            State = GetObject<TLState>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLDifference state=[{0}] messages={1} other={2} users={3} chats={4} encrypted={5}", State, NewMessages.Count, OtherUpdates.Count, Users.Count, Chats.Count, NewEncryptedMessages.Count);
        }
    }

    public class TLDifferenceSlice : TLDifference
    {
        public new const uint Signature = TLConstructors.TLDifferenceSlice;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NewMessages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            NewEncryptedMessages = GetObject<TLVector<TLEncryptedMessageBase>>(bytes, ref position);
            OtherUpdates = GetObject<TLVector<TLUpdateBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);
            State = GetObject<TLState>(bytes, ref position);

            ProcessReading();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            NewMessages.ToStream(output);
            NewEncryptedMessages.ToStream(output);
            OtherUpdates.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);
            State.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            NewMessages = GetObject<TLVector<TLMessageBase>>(input);
            NewEncryptedMessages = GetObject<TLVector<TLEncryptedMessageBase>>(input);
            OtherUpdates = GetObject<TLVector<TLUpdateBase>>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);
            State = GetObject<TLState>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLDifferenceSlice state=[{0}] messages={1} other={2} users={3} chats={4} encrypted={5}", State, NewMessages.Count, OtherUpdates.Count, Users.Count, Chats.Count, NewEncryptedMessages.Count);
        }
    }

    public class TLDifferenceTooLong : TLDifferenceBase
    {
        public const uint Signature = TLConstructors.TLDifferenceTooLong;

        public TLInt Pts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Pts = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Pts.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Pts = GetObject<TLInt>(input);

            return this;
        }

        public override TLDifferenceBase GetEmptyObject()
        {
            return new TLDifferenceTooLong { Pts = Pts };
        }

        public override string ToString()
        {
            return string.Format("TLDifferenceTooLong pts={0}", Pts);
        }
    }
}
