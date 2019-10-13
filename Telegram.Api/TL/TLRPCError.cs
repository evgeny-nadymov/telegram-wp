// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
#if !WIN_RT
using System.Net.Sockets;
#endif

namespace Telegram.Api.TL
{
    public enum ErrorType
    {
        PHONE_MIGRATE,
        NETWORK_MIGRATE, 
        FILE_MIGRATE,
        USER_MIGRATE,
        PHONE_NUMBER_INVALID,
        PHONE_CODE_EMPTY,
        PHONE_CODE_EXPIRED,
        PHONE_CODE_INVALID,
        PHONE_NUMBER_OCCUPIED,
        PHONE_NUMBER_UNOCCUPIED,
        FLOOD_WAIT,
        PEER_FLOOD,
        FIRSTNAME_INVALID,
        MIDDLENAME_INVALID,
        LASTNAME_INVALID,
        FIRSTNAMENATIVE_INVALID,
        MIDDLENAMENATIVE_INVALID,
        LASTNAMENATIVE_INVALID,
        QUERY_TOO_SHORT,
        USERNAME_INVALID,
        USERNAME_OCCUPIED,
        USERNAME_NOT_OCCUPIED,  // 400
        USERNAME_NOT_MODIFIED,  // 400
        CHANNELS_ADMIN_PUBLIC_TOO_MUCH, // 400
        CHANNEL_PRIVATE,        // 400
        PEER_ID_INVALID,        // 400    
        MESSAGE_EMPTY,          // 400
        MESSAGE_TOO_LONG,       // 400
        MSG_WAIT_FAILED,        // 400
        MESSAGE_ID_INVALID,     // 400
        MESSAGE_NOT_MODIFIED,   // 400
        MESSAGE_EDIT_TIME_EXPIRED, // 400

        PASSWORD_HASH_INVALID,  // 400
        NEW_PASSWORD_BAD,       // 400
        NEW_SALT_INVALID,       // 400
        EMAIL_INVALID,          // 400
        EMAIL_UNCONFIRMED,      // 400
        EMAIL_VERIFY_EXPIRED,   // 400

        CODE_EMPTY,             // 400
        CODE_INVALID,           // 400
        PASSWORD_EMPTY,         // 400
        PASSWORD_RECOVERY_NA,   // 400
        PASSWORD_RECOVERY_EXPIRED,  //400

        CHAT_INVALID,           // 400
        CHAT_ADMIN_REQUIRED,    // 400   
        CHAT_NOT_MODIFIED,      // 400
        CHAT_ABOUT_NOT_MODIFIED,// 400
        INVITE_HASH_EMPTY,      // 400
        INVITE_HASH_INVALID,    // 400
        INVITE_HASH_EXPIRED,    // 400
        USERS_TOO_MUCH,         // 400
        BOTS_TOO_MUCH,          // 400
        ADMINS_TOO_MUCH,        // 400
        CHANNELS_TOO_MUCH,      // 400
        USER_CHANNELS_TOO_MUCH, // 400
        USER_NOT_MUTUAL_CONTACT,    // 400
        USER_ALREADY_PARTICIPANT,   // 400
        USER_NOT_PARTICIPANT,   // 400

        STICKERSET_INVALID,     // 400
        LOCATION_INVALID,       // 400 upload.getFile
        VOLUME_LOC_NOT_FOUND,   // 400 upload.getFile

        SRP_ID_INVALID,
        SRP_PASSWORD_CHANGED,

        REQ_INFO_NAME_INVALID,
        REQ_INFO_PHONE_INVALID,
        REQ_INFO_EMAIL_INVALID,
        ADDRESS_COUNTRY_INVALID,
        ADDRESS_RESIDENCE_COUNTRY_INVALID,
        ADDRESS_CITY_INVALID,
        ADDRESS_POSTCODE_INVALID,
        ADDRESS_STATE_INVALID,
        ADDRESS_STREET_LINE1_INVALID,
        ADDRESS_STREET_LINE2_INVALID,
        SHIPPING_BOT_TIMEOUT,
        SHIPPING_NOT_AVAILABLE,

        BIRTHDATE_INVALID,
        GENDER_INVALID,
        DOCUMENT_NUMBER_INVALID,
        EXPIRYDATE_INVALID,

        PROVIDER_ACCOUNT_INVALID,
        PROVIDER_ACCOUNT_TIMEOUT,
        INVOICE_ALREADY_PAID,

        REQUESTED_INFO_INVALID,
        SHIPPING_OPTION_INVALID,
        PAYMENT_FAILED,
        PAYMENT_CREDENTIALS_INVALID,
        PAYMENT_CREDENTIALS_ID_INVALID,
        BOT_PRECHECKOUT_FAILED,

        FILE_TOKEN_INVALID,
        REQUEST_TOKEN_INVALID,

        FILES_EMPTY,
        FILES_TOO_MUCH,
        FILE_ERROR,
        TRANSLATION_ERROR,
        TRANSLATION_EMPTY,
        FRONT_SIDE_REQUIRED,
        REVERSE_SIDE_REQUIRED,
        SELFIE_REQUIRED,

        PHONE_VERIFICATION_NEEDED,
        EMAIL_VERIFICATION_NEEDED,

        APP_VERSION_OUTDATED,

        SESSION_PASSWORD_NEEDED,// 401
        SESSION_REVOKED,        // 401
        USER_PRIVACY_RESTRICTED,// 403

        //2FA_RECENT_CONFIRM,   // 420
        //2FA_CONFIRM_WAIT_XXX, // 420
        
        RPC_CALL_FAIL,          // 500

        CANTPARSE
    }

    public enum ErrorCode
    {
        ERROR_SEE_OTHER = 303,
        BAD_REQUEST = 400,
        UNAUTHORIZED = 401,
        FORBIDDEN = 403,
        NOT_FOUND = 404,
        FLOOD = 420,
        INTERNAL = 500,

        #region Additional
        TIMEOUT = 408,
        #endregion
    }

    public class TLRPCReqError : TLRPCError
    {
        public new const uint Signature = TLConstructors.TLRPCReqError;

        public TLLong QueryId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            QueryId = GetObject<TLLong>(bytes, ref position);
            Code = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                QueryId.ToBytes(),
                Code.ToBytes(),
                Message.ToBytes());
        }
    }

    public class TLRPCError : TLObject
    {
        public TLRPCError()
        {
            Code = new TLInt(0);
        }

        public TLRPCError(int errorCode)
        {
            Code = new TLInt(errorCode);
        }

        #region Additional
#if !WIN_RT
        public SocketError? SocketError { get; set; }
#endif
        
        public Exception Exception { get; set; }

        /// <summary>
        /// Await time before next request (ms)
        /// </summary>
        public int AwaitTime { get; set; }
        #endregion


        public bool CodeEquals(ErrorCode code)
        {
            if (Code != null && Enum.IsDefined(typeof (ErrorCode), Code.Value))
            {
                return (ErrorCode) Code.Value == code;
            }

            return false;
        }

        public static bool CodeEquals(TLRPCError error, ErrorCode code)
        {
            if (error.Code != null && Enum.IsDefined(typeof(ErrorCode), error.Code.Value))
            {
                return (ErrorCode)error.Code.Value == code;
            }

            return false;
        }

        public ErrorType GetErrorType()
        {
            var strings = Message.ToString().Split(':');
            var typeString = strings[0];
            if (Enum.IsDefined(typeof(ErrorType), typeString))
            {
                var value = (ErrorType) Enum.Parse(typeof(ErrorType), typeString, true);

                return value;
            }

            return ErrorType.CANTPARSE;
        }

        public string GetErrorTypeString()
        {
            var strings = Message.ToString().Split(':');
            return strings[0];
        }

        public bool TypeStarsWith(string type)
        {
            var strings = Message.ToString().Split(':');
            var typeString = strings[0];

            return typeString.StartsWith(type, StringComparison.OrdinalIgnoreCase);
        }

        public bool TypeStarsWith(ErrorType type)
        {
            var strings = Message.ToString().Split(':');
            var typeString = strings[0];

            return typeString.StartsWith(type.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool TypeEquals(string type)
        {
            if (Message == null) return false;

            var strings = Message.ToString().Split(':');
            var typeString = strings[0];

            return string.Equals(type, typeString, StringComparison.OrdinalIgnoreCase);
        }

        public bool TypeEquals(ErrorType type)
        {
            if (Message == null) return false;

            var strings = Message.ToString().Split(':');
            var typeString = strings[0];
            if (Enum.IsDefined(typeof(ErrorType), typeString))
            {
                var value = (ErrorType)Enum.Parse(typeof (ErrorType), typeString, true);

                return value == type;
            }

            return false;
        }

        public static bool TypeEquals(TLRPCError error, ErrorType type)
        {
            if (error == null) return false;

            return error.TypeEquals(type);
        }

        public const uint Signature = TLConstructors.TLRPCError;

        public TLInt Code { get; set; }

        public TLString Message { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Code = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Code.ToBytes(),
                Message.ToBytes());
        }

        public override string ToString()
        {
#if DEBUG
            return string.Format("{0} {1}{2}{3}", Code, Message, 
#if WINDOWS_PHONE
                SocketError != null ? "\nSocketError=" + SocketError : string.Empty, 
#else
                string.Empty,
#endif
                Exception != null ? "\nException=" : string.Empty);
#else
            return string.Format("{0} {1}", Code, Message);
#endif
        }
    }
}