using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieEditor2.IndividualUI;

internal class DoubleToSizeConverter : IMultiValueConverter
{
    /// <summary>
    /// double型のwidth, heightをまとめてSize型に変換する
    /// </summary>
    /// <param name="values"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if(values.Length >= 2 && values[0] is double width && values[1] is double height)
        {
            return new Size(width, height);
        }

        return Size.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
