using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;

using MovieEditor2.Models;

namespace MovieEditor2.MovieListUI.ViewModels;

public partial class MovieFilesDataGridViewModel : ObservableObject
{
    public MovieFilesDataGridViewModel()
    {
        // ItemsSourceの中身が変更されたときのイベントハンドラを設定
        ItemsSource.CollectionChanged += OnNewItemsAdded;
        ItemsSource.CollectionChanged += OnItemsCountChanged;
    }

    // ItemsSourceの要素数が0のときに表示するメッセージ
    [ObservableProperty] private string _defaultMessage = "ここに動画ファイルをドロップしてください";

    [ObservableProperty] private bool _isDataGridVisible = false;

    /// <summary> DataGridの中身 </summary>
    public ObservableCollection<ItemInfo> ItemsSource { get; } = [];


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

}
