// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache.EventArgs;
#if WINDOWS_PHONE
using System.Windows.Media.Imaging;
#elif WIN_RT
using Windows.UI.Xaml.Media.Imaging;
#endif
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    [DataContract]
    public abstract class TLObject : TelegramPropertyChangedBase
    {
        public TLDialogBase Dialog { get; set; }

        public bool IsGlobalResult { get; set; }

        #region Flags

        public static byte[] ToBytes(TLObject obj, TLInt flags, int flag)
        {
            return obj != null && IsSet(flags, flag) ? obj.ToBytes() : new byte[] {};
        }

        public static void ToStream(Stream output, TLObject obj, TLInt flags, int flag)
        {
            if (IsSet(flags, flag))
            {
                obj.ToStream(output);
            }
        }

        public static void ToStream(Stream output, TLObject obj, TLLong customFlags, int flag)
        {
            if (IsSet(customFlags, flag))
            {
                if (obj == null)
                {
                    
                }

                obj.ToStream(output);
            }
        }

        protected static bool IsSet(TLLong flags, int flag)
        {
            var isSet = false;

            if (flags != null)
            {
                var intFlag = flag;
                isSet = (flags.Value & intFlag) == intFlag;
            }

            return isSet;
        }

        protected static bool IsSet(TLInt flags, int flag)
        {
            var isSet = false;

            if (flags != null)
            {
                var intFlag = flag;
                isSet = (flags.Value & intFlag) == intFlag;
            }

            return isSet;
        }

        protected static void Set(ref TLLong flags, int flag)
        {
            var intFlag = flag;

            if (flags != null)
            {
                flags.Value |= intFlag;
            }
            else
            {
                flags = new TLLong(intFlag);
            }
        }

        protected static void Set(ref TLInt flags, int flag)
        {
            var intFlag = flag;

            if (flags != null)
            {
                flags.Value |= intFlag;
            }
            else
            {
                flags = new TLInt(intFlag);
            }
        }

        protected static void Unset(ref TLInt flags, int flag)
        {
            var intFlag = flag;

            if (flags != null)
            {
                flags.Value &= ~intFlag;
            }
            else
            {
                flags = new TLInt(0);
            }
        }

        protected static void Unset(ref TLLong flags, int flag)
        {
            var intFlag = flag;

            if (flags != null)
            {
                flags.Value &= ~intFlag;
            }
            else
            {
                flags = new TLLong(0);
            }
        }

        protected static void SetUnset(ref TLInt flags, bool set, int flag)
        {
            if (set)
            {
                Set(ref flags, flag);
            }
            else
            {
                Unset(ref flags, flag);
            }
        }

        protected static void SetUnset(ref TLLong flags, bool set, int flag)
        {
            if (set)
            {
                Set(ref flags, flag);
            }
            else
            {
                Unset(ref flags, flag);
            }
        }


        protected static void SetField<T>(out T field, T value, ref TLInt flags, int flag) where T : TLObject
        {
            if (value != null)
            {
                Set(ref flags, flag);
                field = value;
            }
            else
            {
                Unset(ref flags, flag);
                field = null;
            }
        }

        protected static void SetField<T>(out T field, T value, ref TLLong flags, int flag) where T : TLObject
        {
            if (value != null)
            {
                Set(ref flags, flag);
                field = value;
            }
            else
            {
                Unset(ref flags, flag);
                field = null;
            }
        }
        #endregion

        public virtual TLObject FromBytes(byte[] bytes, ref int position)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public virtual TLObject FromStream(Stream input)
        {
            throw new NotImplementedException();
        }

        public virtual void ToStream(Stream output)
        {
            throw new NotImplementedException();
        }

        public static T GetObject<T>(byte[] bytes, ref int position) where T : TLObject
        {
            try
            {
                return (T)TLObjectGenerator.GetObject<T>(bytes, position).FromBytes(bytes, ref position);
            }
            catch (Exception e)
            {
                Execute.ShowDebugMessage(e.ToString());
                TLUtils.WriteLine(e.StackTrace, LogSeverity.Error);
            }

            return null;
        }

        public static T GetObject<T>(Stream input) where T : TLObject
        {
            //try
            //{
            return (T)TLObjectGenerator.GetObject<T>(input).FromStream(input);
            //}
            //catch (Exception e)
            //{
            //    TLUtils.WriteLine(e.StackTrace, LogSeverity.Error);
            //}

            //return null;
        }

        public static T GetObject<T>(TLInt flags, int flag, T defaultValue, byte[] bytes, ref int position) where T : TLObject
        {
            var value = IsSet(flags, flag) ? GetObject<T>(bytes, ref position) : defaultValue;
            //if (value != null)
            //{
            //    Set(ref flags, flag);
            //}
            return value;
        }

        public static T GetObject<T>(TLInt flags, int flag, T defaultValue, Stream inputStream) where T : TLObject
        {
            var value = IsSet(flags, flag) ? GetObject<T>(inputStream) : defaultValue;
            //if (value != null)
            //{
            //    Set(ref flags, flag);
            //}
            return value;
        }

        public static T GetObject<T>(TLLong customFlags, int flag, T defaultValue, Stream inputStream) where T : TLObject
        {
            var value = IsSet(customFlags, flag) ? GetObject<T>(inputStream) : defaultValue;
            //if (value != null)
            //{
            //    Set(ref customFlags, flag);
            //}
            return value;
        }

        public static T GetNullableObject<T>(Stream input) where T : TLObject
        {
            return TLObjectExtensions.NullableFromStream<T>(input);
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

        private WriteableBitmap _bitmap;

        public WriteableBitmap Bitmap
        {
            get { return _bitmap; }
            set
            {
                SetField(ref _bitmap, value, () => Bitmap);
            }
        }

        public void SetBitmap(WriteableBitmap bitmap)
        {
            //if (_bitmap == null)
            //{
            Bitmap = bitmap;
            //}
            //else
            //{
            //    _bitmap = bitmap;
            //}
        }

        public void ClearBitmap()
        {
            _bitmap = null;
        }
    }
}
