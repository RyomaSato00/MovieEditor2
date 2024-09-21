using System.Globalization;
using System.Windows;
using System.Windows.Data;

using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.MovieListUI;

/// <summary>
/// ItemIndexから、移動ボタンの表示・非表示を取得するためのコンバータ
/// </summary>
/// <remarks>
/// ※ItemIndex, ItemsSource.Countの順でMultiBindingすること<br/>
/// ※パラメータにはボタンの方向によって"previous", "next"を指定すること
/// </remarks>
internal class IndexToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if(values.Length == 2
        && values[0] is int itemIndex
        && values[1] is int count)
        {
            var direction = (string)parameter;

            // 前方向のボタンのとき
            if(direction == "previous")
            {
                // インデックスが0以下のときはこれ以上前に戻れないため、ボタンを表示しない
                return itemIndex <= 0 ? Visibility.Hidden : Visibility.Visible;
            }
            // 次方向のボタンのとき
            else if(direction == "next")
            {
                // インデックスが最大値のときはこれ以上次に進めないため、ボタンを表示しない
                // インデックスが0より小さいとき（非選択状態）も進めないため、ボタンを表示しない
                return (itemIndex >= count - 1) || (itemIndex < 0) ? Visibility.Hidden : Visibility.Visible;
            }
            else
            {
                // ここにはこないはず
            }
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
