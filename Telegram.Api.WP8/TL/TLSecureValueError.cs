// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLSecureValueErrorBase : TLObject 
    {
        public TLSecureValueTypeBase Type { get; set; }

        public TLString Text { get; set; }

        public abstract int Priority { get; }
    }

    public class TLSecureValueError : TLSecureValueErrorBase
    {
        public const uint Signature = TLConstructors.TLSecureValueError;

        public TLString Hash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            Hash = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 1; }
        }
    }

    public class TLSecureValueErrorData : TLSecureValueErrorBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorData;

        public TLString DataHash { get; set; }

        public TLString Field { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            DataHash = GetObject<TLString>(bytes, ref position);
            Field = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get
            {
                switch (Field.ToString())
                {
                    case "first_name":
                        return 801;
                    case "middle_name":
                        return 802;
                    case "last_name":
                        return 803;
                    case "birth_date":
                        return 804;
                    case "gender":
                        return 805;
                    //case "country_code":
                    //    return 806;
                    case "residence_country_code":
                        return 807;
                    case "document_no":
                        return 808;
                    case "expiry_date":
                        return 809;


                    case "street_line1":
                        return 851;
                    case "street_line2":
                        return 852;
                    case "post_code":
                        return 853;
                    case "state":
                        return 854;
                    case "city":
                        return 855;
                    case "country_code":
                        return 856;
                }
                
                return 800;
            }
        }
    }

    public abstract class TLSecureValueErrorFileBase : TLSecureValueErrorBase
    {
        public TLString FileHash { get; set; }
    }

    public class TLSecureValueErrorFrontSide : TLSecureValueErrorFileBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorFrontSide;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            FileHash = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 100; }
        }
    }

    public class TLSecureValueErrorReverseSide : TLSecureValueErrorFileBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorReverseSide;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            FileHash = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 200; }
        }
    }

    public class TLSecureValueErrorSelfie : TLSecureValueErrorFileBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorSelfie;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            FileHash = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 300; }
        }
    }

    public class TLSecureValueErrorFile : TLSecureValueErrorFileBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorFile;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            FileHash = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 400; }
        }
    }

    public class TLSecureValueErrorFiles : TLSecureValueErrorBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorFiles;

        public TLVector<TLString> FileHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            FileHash = GetObject<TLVector<TLString>>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 500; }
        }
    }

    public class TLSecureValueErrorTranslationFile : TLSecureValueErrorFileBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorTranslationFile;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            FileHash = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 600; }
        }
    }

    public class TLSecureValueErrorTranslationFiles : TLSecureValueErrorBase
    {
        public const uint Signature = TLConstructors.TLSecureValueErrorTranslationFiles;

        public TLVector<TLString> FileHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            FileHash = GetObject<TLVector<TLString>>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override int Priority
        {
            get { return 700; }
        }
    }
}