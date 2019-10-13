// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Windows.Storage;
using Caliburn.Micro;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Passport;

namespace TelegramClient.Converters
{
    public class SecureFilePreviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var secureFile = value as TLSecureFile;
            if (secureFile != null)
            {
                var inputFileLocation = new TLInputDocumentFileLocation54
                {
                    Id = secureFile.Id,
                    AccessHash = secureFile.AccessHash,
                    Version = new TLInt(0)
                };

                var localFileName = string.Format("secureid_{0}.dat", secureFile.Id);
                var previewFileName = string.Format("secureid_preview_{0}.dat", secureFile.Id);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(previewFileName))
                    {
                        BitmapImage imageSource;

                        try
                        {
                            using (var stream = store.OpenFile(previewFileName, FileMode.Open, FileAccess.Read))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                var image = new BitmapImage();
                                image.CreateOptions |= BitmapCreateOptions.BackgroundCreation;
                                image.SetSource(stream);
                                imageSource = image;
                            }
                        }
                        catch (Exception)
                        {
                            return null;
                        }

                        return imageSource;
                    }
                    if (store.FileExists(inputFileLocation.GetFileName()))
                    {
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            DecryptFile(secureFile, inputFileLocation, localFileName, previewFileName);
                        });

                        return null;
                    }
                }

                IoC.Get<IDocumentFileManager>().DownloadFileAsync(
                    new TLString(localFileName), 
                    secureFile.DCId, 
                    inputFileLocation, 
                    secureFile, 
                    secureFile.Size,
                    progress =>
                    {
                        
                    },
                    item =>
                    {
                        DecryptFile(secureFile, inputFileLocation, localFileName, previewFileName);
                    });
            }

            var secureFileUploaded = value as TLSecureFileUploaded;
            if (secureFileUploaded != null)
            {
                var previewFileName = string.Format("secureid_preview_{0}.dat", secureFileUploaded.Id);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(previewFileName))
                    {
                        BitmapImage imageSource;

                        try
                        {
                            using (var stream = store.OpenFile(previewFileName, FileMode.Open, FileAccess.Read))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                var image = new BitmapImage();
                                image.CreateOptions |= BitmapCreateOptions.BackgroundCreation;
                                image.SetSource(stream);
                                imageSource = image;
                            }
                        }
                        catch (Exception)
                        {
                            return null;
                        }

                        return imageSource;
                    }
                }
            }

            return null;
        }

        private static async void DecryptFile(TLSecureFile secureFile, TLInputFileLocationBase inputFileLocation, string localFileName, string previewFileName)
        {
            var fileSecret = Passport.DecryptValueSecret(
                secureFile.Secret,
                EnterPasswordViewModel.Secret,
                secureFile.FileHash);

            var encryptedFile = await ApplicationData.Current.LocalFolder.GetFileAsync(inputFileLocation.GetFileName("document"));
            
            var decryptedTuple = await Passport.DecryptFile(localFileName, encryptedFile, fileSecret, secureFile.FileHash);

            var stream = await decryptedTuple.Item1.OpenReadAsync();

            await DialogDetailsViewModel.ResizeJpeg(stream, 180, localFileName, previewFileName);

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                secureFile.NotifyOfPropertyChange(() => secureFile.Self);
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
