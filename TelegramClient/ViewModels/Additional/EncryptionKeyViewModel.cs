// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class EncryptionKeyViewModel  : TelegramPropertyChangedBase
    {
        private readonly TLString _key;
        private readonly TLUserBase _contact;
        private readonly TLEncryptedChatBase _chat;

        public string VisualizationTime { get; set; }

        public WriteableBitmap Bitmap { get; set; }

        public string EncryptionKeyDescription1
        {
            get
            {
                var chat17 = _chat as TLEncryptedChat17;
                var extended = chat17 != null
                    && chat17.Layer.Value >= Constants.MinSecretChatWithExtendedKeyVisualizationLayer
                    && chat17.ExtendedKey != null;
                
                if (extended)
                {
                    return string.Format(AppResources.EncryptionKeyExtendedDescription1, _contact.FirstName);
                }

                return string.Format(AppResources.EncryptionKeyDescription1, _contact.FirstName);
            }
        }

        public string EncryptionKeyDescription2
        {
            get
            {
                var chat17 = _chat as TLEncryptedChat17;
                var extended = chat17 != null
                    && chat17.Layer.Value >= Constants.MinSecretChatWithExtendedKeyVisualizationLayer
                    && chat17.ExtendedKey != null;

                if (extended)
                {
                    return string.Format(AppResources.EncryptionKeyExtendedDescription2, _contact.FirstName);
                }

                return string.Format(AppResources.EncryptionKeyDescription2, _contact.FirstName);
            }
        }

        public string EncriptionKeyString
        {
            get
            {
                var chat17 = _chat as TLEncryptedChat17;
                var extended = chat17 != null 
                    && chat17.Layer.Value >= Constants.MinSecretChatWithExtendedKeyVisualizationLayer
                    && chat17.ExtendedKey != null;

                if (!extended) return string.Empty;

                var key = _chat.OriginalKey ?? _chat.Key;
                var extendedKey = _chat.ExtendedKey;

                if (key == null || key.Data == null) return string.Empty;

                var sha1Hash = Telegram.Api.Helpers.Utils.ComputeSHA1(key.Data);
                var sha256Hash = Telegram.Api.Helpers.Utils.ComputeSHA256(extendedKey.Data);

                return string.Format("{0}  {1}\n{2}  {3}\n{4}  {5}\n{6}  {7}",
                    BitConverter.ToString(sha1Hash.SubArray(0, 4)).Replace('-', ' '),
                    BitConverter.ToString(sha1Hash.SubArray(4, 4)).Replace('-', ' '), 
                    BitConverter.ToString(sha1Hash.SubArray(8, 4)).Replace('-', ' '),
                    BitConverter.ToString(sha1Hash.SubArray(12, 4)).Replace('-', ' '),
                    BitConverter.ToString(sha256Hash.SubArray(0, 4)).Replace('-', ' '),
                    BitConverter.ToString(sha256Hash.SubArray(4, 4)).Replace('-', ' '), 
                    BitConverter.ToString(sha256Hash.SubArray(8, 4)).Replace('-', ' '),
                    BitConverter.ToString(sha256Hash.SubArray(12, 4)).Replace('-', ' '));
            }
        }

        public EncryptionKeyViewModel(IStateService stateService)
        {
            _key = stateService.CurrentKey;
            stateService.CurrentKey = null;

            _contact = stateService.CurrentContact;
            stateService.CurrentContact = null;

            _chat = stateService.CurrentEncryptedChat;
            stateService.CurrentEncryptedChat = null;

            var timer = Stopwatch.StartNew();
            Bitmap = CreateKeyBitmap(_chat);
            VisualizationTime = timer.Elapsed.ToString();
        }

        public void AnimationComplete()
        {
            
            //NotifyOfPropertyChange(() => VisualizationTime);
            //NotifyOfPropertyChange(() => Visualization);
        }

        public static void DrawFilledRectangle(WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
            int[] pixels = bmp.Pixels;

            // Check boundaries
            if (x1 < 0) { x1 = 0; }
            if (y1 < 0) { y1 = 0; }
            if (x2 < 0) { x2 = 0; }
            if (y2 < 0) { y2 = 0; }
            if (x1 >= w) { x1 = w - 1; }
            if (y1 >= h) { y1 = h - 1; }
            if (x2 >= w) { x2 = w; }
            if (y2 >= h) { y2 = h; }

            int i = y1 * w;
            for (int y = y1; y < y2; y++)
            {
                int i2 = i + x1;
                while (i2 < i + x2)
                {
                    pixels[i2++] = color;
                }
                i += w;
            }
        }

        private WriteableBitmap CreateKeyBitmap(TLEncryptedChatBase chat)
        {
            var key = chat.OriginalKey ?? chat.Key;
            if (key == null) return null;
            var data = key.Data;

            var chat17 = chat as TLEncryptedChat17;

            var extended = chat17 != null 
                && chat17.Layer.Value >= Constants.MinSecretChatWithExtendedKeyVisualizationLayer
                && chat17.ExtendedKey != null;

            var count = extended ? 12 : 8;
            var length = extended ? 25 : 40;
            var bitmap = new WriteableBitmap(count * length, count * length);
            
            var hash = Telegram.Api.Helpers.Utils.ComputeSHA1(data);
            var colors = new []{
                    0xffffffff,
                    0xffd5e6f3,
                    0xff2d5775,
                    0xff2f99c9};

            if (extended)
            {
                var extendedData = chat17.ExtendedKey.Data;

                for (var i = 0; i < 64; i++)
                {
                    var index = (hash[i / 4] >> (2 * (i % 4))) & 0x3;

                    var x = i % 12;
                    var y = i / 12;
                    DrawFilledRectangle(bitmap, x * length, y * length, (x + 1) * length, (y + 1) * length, (int)colors[index]);
                }

                var hash256 = Telegram.Api.Helpers.Utils.ComputeSHA256(extendedData);
                for (var i = 0; i < 80; i++)
                {
                    var index = (hash256[i/4] >> (2*(i%4))) & 0x3;

                    var local = i + 64;
                    var x = local % 12;
                    var y = local / 12;
                    DrawFilledRectangle(bitmap, x*length, y*length, (x + 1)*length, (y + 1)*length, (int) colors[index]);
                }
            }
            else
            {
                for (int i = 0; i < 64; i++)
                {
                    int index = (hash[i / 4] >> (2 * (i % 4))) & 0x3;

                    var x = i % 8;
                    var y = i / 8;
                    DrawFilledRectangle(bitmap, x * length, y * length, (x + 1) * length, (y + 1) * length, (int)colors[index]);
                }
            }


            return bitmap;
        }
    }
}
