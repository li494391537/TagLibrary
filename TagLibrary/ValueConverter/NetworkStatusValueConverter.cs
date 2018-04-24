using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Lirui.TagLibrary.ValueConverter {
    [ValueConversion(typeof(bool), typeof(string))]
    public class NetworkStatusValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var isOK = (bool) value;

            if (isOK) {
                return "服务已开启";
            } else {
                return "服务开启失败";
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
