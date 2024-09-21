using System.Globalization;
using System.Windows.Data;

namespace MovieEditor2.MovieListUI;

[ValueConversion(typeof(long), typeof(float))]
internal class ByteToMByteConverter : IValueConverter
{
    /// <summary>
    /// long型のファイルサイズ値をfloat型のMbyte単位に変換する
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var fileSize = (long)value;

        return (float)fileSize / 1000000;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
