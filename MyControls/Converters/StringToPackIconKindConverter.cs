
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using MaterialDesignThemes.Wpf;

namespace Converters;

[ValueConversion(typeof(string), typeof(PackIconKind))]
public class StringToPackIconKindConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // アイコン指定なし
        if(string.Empty == (string)value) return DependencyProperty.UnsetValue;

        if(Enum.TryParse<PackIconKind>((string)value, false, out var result))
        {
            return result;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"{value}アイコンが見つかりません");
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
