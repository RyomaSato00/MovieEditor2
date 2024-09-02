
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieEditor2.IndividualUI;

/// <summary>
/// TimeSpanとstring（TextBox)間の変換を行うコンバータ
/// </summary>
internal class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is TimeSpan point)
        {
            return point.ToString("mm\\:ss\\.fff");
        }
        else
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = (string)value;

        try
        {
            var parts = text.Split(':');
            var seconds = 0.0;

            // 後ろ（秒）から足していく。分は60×valueを足し合わせる。時は60^2×value…
            for(var i = 0; i < parts.Length; i++)
            {
                seconds += Math.Pow(60, i) * double.Parse(parts[parts.Length - 1 - i]);
            }

            return TimeSpan.FromSeconds(seconds);
        }
        catch(Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{e}");
            return null;
        }
    }
}
