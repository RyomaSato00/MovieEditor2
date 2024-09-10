using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using MovieEditor2.Models;

namespace MovieEditor2.MovieListUI.ViewModels;

public partial class MovieFilesDataGridViewModel : ObservableObject
{
    public static readonly string MovieFilesDataGridDialogIdentifier = "MovieFilesDataGridDialog";

    public MovieFilesDataGridViewModel(ObservableCollection<ItemInfo> items)
    {
        ItemsSource = items;

        // ItemsSourceの中身が変更されたときのイベントハンドラを設定
        ItemsSource.CollectionChanged += OnNewItemsAdded;
        ItemsSource.CollectionChanged += OnItemsCountChanged;
    }

    /// <summary> 個別編集画面遷移要求通知 </summary>
    public event Action<int>? OnEditIndividualRequested = null;

    /// <summary> 動画結合処理要求通知 </summary>
    public event Action<ItemInfo[]>? OnJoinFilesRequested = null;

    // ItemsSourceの要素数が0のときに表示するメッセージ
    [ObservableProperty] private string _defaultMessage = "ここに動画ファイルをドロップしてください";

    [ObservableProperty] private bool _isDataGridVisible = false;

    /// <summary> DataGridの中身 </summary>
    public ObservableCollection<ItemInfo> ItemsSource { get; }

    /// <summary> 選択中のアイテム </summary>
    private readonly List<ItemInfo> _selectedItems = [];

    /// <summary>
    ///
    /// </summary>
    /// <value>
    /// ItemsSourceのIsSelectedがすべてtrueのとき：true <br/>
    /// ItemsSourceのIsSelectedがすべてfalseのとき：false <br/>
    /// ItemsSourceのIsSelectedがtrue/falseどちらもあるとき：null
    /// </value>
    public bool? IsAllItems1Selected
    {
        get
        {
            // ItemsSourceの各IsSelectedの値について、重複しないリストを作成する
            var selected = ItemsSource.Select(item => item.IsSelected).Distinct().ToList();

            // リストの中身がひとつのとき（すべてtrueまたは、すべてfalse）その値を返す。それ以外はnullを返す
            return selected.Count == 1 ? selected.Single() : (bool?)null;
        }
        set
        {
            // valueの値をItemsSourceの各IsSelectedに適用する
            if (value.HasValue)
            {
                SelectAll(value.Value, ItemsSource);
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// ItemsSourceに新しい項目を追加または置き換えをしたときのイベントハンドラ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnNewItemsAdded(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
        // 要素の追加もしくは置き換えのとき
        case NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace:

            if (e.NewItems is null) break;

            // 各新しい項目について
            foreach (var item in e.NewItems)
            {
                // この項目はItemInfo型？
                if (item is ItemInfo itemInfo)
                {
                    // 新しい項目にイベント設定を行う
                    itemInfo.IsSelectedChanged += (_, _) => OnPropertyChanged(nameof(IsAllItems1Selected));
                }
            }
            break;

        default:
            break;
        }
    }

    /// <summary>
    /// ItemsSourceの項目数が変化したときのイベントハンドラ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnItemsCountChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
        // 要素の追加または要素の削除または要素の大幅変更
        case NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset:

            if(sender is ObservableCollection<ItemInfo> itemsSource)
            {
                if(itemsSource.Count == 0)
                {
                    // DataGridを非表示にしてDefaultMessageを表示させる
                    IsDataGridVisible = false;
                }
                else
                {
                    // DataGridを表示してDefaultMessageを消す
                    IsDataGridVisible = true;
                }
            }
            break;

        default:
            break;
        }
    }

    /// <summary>
    /// itemsのすべてのIsSelectedにselectの値を適用する
    /// </summary>
    /// <param name="select"></param>
    /// <param name="items"></param>
    private static void SelectAll(bool select, IEnumerable<ItemInfo> items)
    {
        foreach (var item in items)
        {
            item.IsSelected = select;
        }
    }

    /// <summary>
    /// 個別編集画面へ遷移する
    /// </summary>
    /// <param name="info"></param>
    [RelayCommand] private void EditIndividual(ItemInfo info)
    {
        var index = ItemsSource.IndexOf(info);
        OnEditIndividualRequested?.Invoke(index);
    }

    /// <summary>
    /// アイテムを削除する
    /// </summary>
    /// <param name="info"></param>
    [RelayCommand] private void DeleteItem()
    {
        // 削除中に選択状態が変更され、_selecteidItemsの中身が変更されるため、別の配列を作成して避難させる
        var selection = _selectedItems.ToArray();

        // 選択状態のアイテムをすべて削除する
        foreach(var item in selection)
        {
            ItemsSource.Remove(item);
        }
    }

    /// <summary>
    /// アイテムを複製する
    /// </summary>
    /// <param name="info"></param>
    [RelayCommand] private void CloneItem(ItemInfo info)
    {
        // 同アイテムの重複回数
        var cloneCount = 0;

        foreach(var item in ItemsSource)
        {
            // ファイルパスが重複する回数を数える
            if(info.FilePath == item.FilePath)
            {
                cloneCount++;
            }
        }

        // アイテム複製
        ItemsSource.Add(new ItemInfo(info.FilePath, cloneCount));
    }

    /// <summary>
    /// 動画結合処理
    /// </summary>
    /// <returns></returns>
    [RelayCommand] private async Task JoinItems()
    {
        var viewModel = new JoinFilesDataGridViewModel(_selectedItems);

        var view = new JoinFilesDataGrid { DataContext = viewModel };

        // ダイアログ表示
        object? result = await DialogHost.Show(view, MovieFilesDataGridDialogIdentifier, null, null, null);

        if(result is not IReadOnlyList<ItemInfo> files) return;

        // 動画結合処理
        OnJoinFilesRequested?.Invoke([.. files]);
    }

    /// <summary>
    /// DataGridの選択状態が変更されたときの処理
    /// </summary>
    /// <param name="items"></param>
    [RelayCommand] private void SelectionChanged(System.Collections.IList items)
    {
        // 選択中アイテムをクリア
        _selectedItems.Clear();

        // 選択中アイテムの更新
        foreach(var item in items)
        {
            if(item is ItemInfo itemInfo)
            {
                _selectedItems.Add(itemInfo);
            }
        }
    }
}
