using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class EqualsConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            return values is not null && values.Distinct().Count() == 1;
        }
    }
}
