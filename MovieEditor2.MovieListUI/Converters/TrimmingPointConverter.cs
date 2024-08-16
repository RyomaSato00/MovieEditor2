using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MovieEditor2.MovieListUI.Converters;

[ValueConversion(typeof(TimeSpan?), typeof(string))]
internal class TrimmingPointConverter : IValueConverter
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter">"Start"または"End"の文字列を指定する</param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var position = (string)parameter;

        // トリミング開始位置についての変換のとき
        if(position == "Start")
        {
            // StartPointがnullのときはTimeSpan.Zeroを、それ以外はそのままの値を返す
            var startPont = (value as TimeSpan?) ?? TimeSpan.Zero;

            return String.Format("{0:mm\\:ss\\.fff}", startPont);
        }
        // トリミング終了位置についての変換のとき
        else if(position == "End")
        {
            var endPoint = value as TimeSpan?;

            // EndPointがnullのときは文字"E"を、それ以外はそのままの値を返す
            if(endPoint is null)
            {
                return "E";
            }
            else
            {
                return String.Format("{0:mm\\:ss\\.fff}", endPoint);
            }
        }
        else
        {
            // ここにはこないはず
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
