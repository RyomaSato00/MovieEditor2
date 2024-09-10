using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MovieEditor2.MovieListUI.ViewModels;

internal partial class JoinFilesDataGridViewModel : ObservableObject
{
    public JoinFilesDataGridViewModel(IEnumerable<ItemInfo> items)
    {
        ItemsSource = new(items);
    }

    /// <summary> DataGridの中身 </summary>
    public ObservableCollection<ItemInfo> ItemsSource { get; }

    /// <summary> 選択中のアイテムのインデックス </summary>
    [ObservableProperty] private int _selectedIndex = -1;

    /// <summary>
    /// アイテムを削除する
    /// </summary>
    /// <param name="info"></param>
    [RelayCommand] private void DeleteItem(ItemInfo info)
    {
        ItemsSource.Remove(info);
    }

    /// <summary>
    /// 対象の動画をひとつ前へ並び替える
    /// </summary>
    [RelayCommand] private void MoveBefore()
    {
        System.Diagnostics.Debug.WriteLine($"before:{SelectedIndex}");
        if(SelectedIndex > 0)
        {
            // この時点でのSelectedIndexを記憶
            var index = SelectedIndex;

            // indexが1つ少ない要素と入れ替える
            (ItemsSource[index - 1], ItemsSource[index]) = (ItemsSource[index], ItemsSource[index - 1]);

            // 移動させた要素を選択状態にする
            SelectedIndex = index - 1;
        }
    }

    /// <summary>
    /// 対象の動画をひとつ後ろへ並び替える
    /// </summary>
    [RelayCommand] private void MoveAfter()
    {
        System.Diagnostics.Debug.WriteLine($"after:{SelectedIndex}");
        if(SelectedIndex < ItemsSource.Count - 1 && SelectedIndex >= 0)
        {
            // この時点でのSelectedIndexを記憶
            var index = SelectedIndex;

            // indexが1つ多い要素と入れ替える
            (ItemsSource[index + 1], ItemsSource[index]) = (ItemsSource[index], ItemsSource[index + 1]);

            // 移動させた要素を選択状態にする
            SelectedIndex = index + 1;
        }
    }
}
