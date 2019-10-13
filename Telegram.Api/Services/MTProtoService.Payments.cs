// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Payments;

namespace Telegram.Api.Services
{
    public partial class MTProtoService
    {
        public void GetPaymentFormAsync(TLInt msgId, Action<TLPaymentForm> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetPaymentForm { MsgId = msgId };

            const string caption = "payments.getPaymentForm";
            SendInformativeMessage<TLPaymentForm>(caption, obj,
                result => _cacheService.SyncUsers(result.Users, users =>
                {
                    result.Users = users;
                    callback.SafeInvoke(result);
                }), 
                faultCallback);
        }

        public void SendPaymentFormAsync(TLInt msgId, TLString requestedInfoId, TLString shippingOptionId, TLInputPaymentCredentialsBase credentials, Action<TLPaymentResultBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSendPaymentForm
            {
                Flags = new TLInt(0),
                MsgId = msgId,
                RequestedInfoId = requestedInfoId,
                ShippingOptionId = shippingOptionId,
                Credentials = credentials
            };

            const string caption = "payments.savePaymentForm";
            SendInformativeMessage<TLPaymentResultBase>(caption, obj,
                result =>
                {
                    var paymentResult = result as TLPaymentResult;
                    if (paymentResult != null)
                    {
                        var multiPts = paymentResult.Updates as IMultiPts;
                        if (multiPts != null)
                        {
                            _updatesService.SetState(multiPts, caption);
                        }
                        else
                        {
                            ProcessUpdates(paymentResult.Updates, null, true);
                        }
                    }

                    callback.SafeInvoke(result);
                },
                faultCallback);
        }

        public void ValidateRequestedInfoAsync(bool save, TLInt msgId, TLPaymentRequestedInfo info, Action<TLValidatedRequestedInfo> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLValidateRequestedInfo
            {
                Flags = new TLInt(0),
                Save = save,
                MsgId = msgId,
                Info = info
            };

            const string caption = "payments.validateRequestedInfo";
            SendInformativeMessage<TLValidatedRequestedInfo>(caption, obj, callback.SafeInvoke, faultCallback);
        }

        public void GetSavedInfoAsync(Action<TLSavedInfo> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetSavedInfo();

            const string caption = "payments.getSavedInfo";
            SendInformativeMessage<TLSavedInfo>(caption, obj, callback.SafeInvoke, faultCallback);
        }

        public void ClearSavedInfoAsync(bool credentials, bool info, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLClearSavedInfo{ Flags = new TLInt(0), Credentials = credentials, Info = info };

            const string caption = "payments.clearSavedInfo";
            SendInformativeMessage<TLBool>(caption, obj, callback.SafeInvoke, faultCallback);
        }

        public void GetPaymentReceiptAsync(TLInt msgId, Action<TLPaymentReceipt> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetPaymentReceipt{ MsgId = msgId };

            const string caption = "payments.getGetReceipt";
            SendInformativeMessage<TLPaymentReceipt>(caption, obj,
                result => _cacheService.SyncUsers(result.Users, users =>
                {
                    result.Users = users;
                    callback.SafeInvoke(result);
                }),
                faultCallback);
        }
    }
}
