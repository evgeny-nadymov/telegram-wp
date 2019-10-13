using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Controls.VirtualizedView;

namespace Telegram.EmojiPanel.Controls.Emoji
{
    //class StickerToImageSourceConverter : IValueConverter
    //{

    //    private static ImageSource ReturnOrEnqueueSticker(TLDocument22 document, TLStickerItem sticker)
    //    {
    //        if (document == null) return null;

    //        var documentLocalFileName = document.GetFileName();

    //        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
    //        {
    //            if (!store.FileExists(documentLocalFileName))
    //            {
    //                TLObject owner = document;
    //                if (sticker != null)
    //                {
    //                    owner = sticker;
    //                }

    //                // 1. download full size
    //                IoC.Get<IDocumentFileManager>().DownloadFileAsync(document.FileName, document.DCId, document.ToInputFileLocation(), owner, document.Size, progress => { });

    //                // 2. download preview
    //                var thumbCachedSize = document.Thumb as TLPhotoCachedSize;
    //                if (thumbCachedSize != null)
    //                {
    //                    var fileName = "cached" + document.GetFileName();
    //                    var buffer = thumbCachedSize.Bytes.Data;
    //                    if (buffer == null) return null;

    //                    return DecodeWebPImage(fileName, buffer, () => { });
    //                }

    //                var thumbPhotoSize = document.Thumb as TLPhotoSize;
    //                if (thumbPhotoSize != null)
    //                {
    //                    var location = thumbPhotoSize.Location as TLFileLocation;
    //                    if (location != null)
    //                    {
    //                        return ReturnOrEnqueueStickerPreview(location, sticker, thumbPhotoSize.Size);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                if (document.DocumentSize > 0
    //                    && document.DocumentSize < Telegram.Api.Constants.StickerMaxSize)
    //                {
    //                    byte[] buffer;
    //                    using (var file = store.OpenFile(documentLocalFileName, FileMode.Open))
    //                    {
    //                        buffer = new byte[file.Length];
    //                        file.Read(buffer, 0, buffer.Length);
    //                    }

    //                    return DecodeWebPImage(documentLocalFileName, buffer,
    //                        () =>
    //                        {
    //                            using (var localStore = IsolatedStorageFile.GetUserStoreForApplication())
    //                            {
    //                                localStore.DeleteFile(documentLocalFileName);
    //                            }
    //                        });
    //                }
    //            }
    //        }

    //        return null;
    //    }

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var document = value as TLDocument22;
    //        if (document == null) return null;

    //        document.
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //class StickerSpriteItem : VListItemBase
    //{
    //    public override double FixedHeight
    //    {
    //        get { return 180.0; } 
    //        set { }
    //    }

    //    public double StickerWidth
    //    {
    //        get { return 120.0; }
    //    }

    //    public double StickerHeight
    //    {
    //        get { return 180.0; }
    //    }

    //    public StickerSpriteItem(IList<TLDocument> stickers)
    //    {
    //        var stackPanel = new StackPanel();

    //        for (var i = 0; i < stickers.Count; i++)
    //        {
    //            var binding = new Binding();
    //            binding.Source = stickers[i];
    //            //binding.Path = new PropertyPath("");
    //            binding.Mode = BindingMode.OneWay;
    //            binding.Converter = new StickerToImageSourceConverter();

    //            var image = new Image
    //            {
    //                Width = StickerWidth,
    //                Height = StickerHeight,
    //                //Source = new BitmapImage
    //                //{
    //                //    //DecodePixelWidth = decodePixelWidth,
    //                //    //DecodePixelType = DecodePixelType.Physical
    //                //    //UriSource = spriteUri
    //                //},
    //                Margin = new Thickness(0, 0, 0, 0),
    //                VerticalAlignment = VerticalAlignment.Top
    //            };
    //            image.SetBinding(Image.SourceProperty, binding);
    //            image.Tap += Sticker_OnTap;

    //            stackPanel.Children.Add(image);
    //        }

    //        Children.Add(stackPanel);
    //    }

    //    private void Sticker_OnTap(object sender, GestureEventArgs e)
    //    {
            
    //    }
    //}
}
