using System;
using System.Globalization;
using System.Windows.Data;

namespace Lirui.TagLibrary.ValueConverter {
    [ValueConversion(typeof(long), typeof(string))]
    public class FileSizeValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var num = 1024.00; 
            var size = (long)value;
            if (size < num)
                return size + " Bytes";
            if (size < Math.Pow(num, 2))
                return (size / num).ToString("f2") + " KB";
            if (size < Math.Pow(num, 3))
                return (size / Math.Pow(num, 2)).ToString("f2") + " MB";
            if (size < Math.Pow(num, 4))
                return (size / Math.Pow(num, 3)).ToString("f2") + " GB";

            return (size / Math.Pow(num, 4)).ToString("f2") + " TB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
