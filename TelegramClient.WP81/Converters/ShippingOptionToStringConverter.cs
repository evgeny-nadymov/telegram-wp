// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Telegram.Api.TL;

namespace TelegramClient.Converters
{
    public class ShippingOptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var shippingOption = value as TLShippingOption;
            if (shippingOption != null)
            {
                if (shippingOption.Prices.Count > 0)
                {
                    var currency = shippingOption.Prices[0].Currency ?? TLString.Empty;
                    return string.Format("{0} - {1}", Telegram.Controls.Utils.Currency.GetString(shippingOption.Prices.Sum(x => x.Amount.Value), currency.ToString()), shippingOption.Title);
                }

                return shippingOption.Title.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LabeledPriceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var labeledPrice = value as TLLabeledPrice;
            if (labeledPrice != null)
            {
                var currency = labeledPrice.Currency ?? TLString.Empty;
                return Telegram.Controls.Utils.Currency.GetString(labeledPrice.Amount.Value, currency.ToString());
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
