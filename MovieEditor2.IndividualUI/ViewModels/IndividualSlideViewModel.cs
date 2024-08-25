using System.Collections.ObjectModel;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MovieEditor2.Models;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.IndividualUI.ViewModels;

public partial class IndividualSlideViewModel : ObservableObject
{
    public IndividualSlideViewModel(ObservableCollection<ItemInfo> items)
    {
        ItemsSource = items;
    }

    /// <summary> 動画ファイルリスト </summary>
    public ObservableCollection<ItemInfo> ItemsSource { get; }

    // ターゲット動画ファイル
    [ObservableProperty] private ItemInfo? _item = null;

    // ターゲット動画ファイルのインデックス
    [ObservableProperty] private int? _itemIndex = null;

    // トリミング設定

    [ObservableProperty] private string _trimStartImage = string.Empty;

    [ObservableProperty] private string _trimEndImage = string.Empty;

    /// <summary>
    /// indexによりItemを選択する
    /// </summary>
    /// <param name="index"></param>
    public void SelectItem(int index)
    {
        Item = ItemsSource[index];
        ItemIndex = index;
    }

    /// <summary>
    /// トリミング開始位置を指定する
    /// </summary>
    /// <param name="point"></param>
    [RelayCommand] private void PinStartPoint(TimeSpan point)
    {
        if(Item is null) return;

        Item.Trimming.StartPoint = point;

        // 開始位置のサムネイル取得
        TrimStartImage = MovieFileProcessor.GetThumbnailPath(Item.FilePath, point);
    }

    /// <summary>
    /// トリミング終了位置を指定する
    /// </summary>
    /// <param name="point"></param>
    [RelayCommand] private void PinEndPoint(TimeSpan point)
    {
        if(Item is null) return;

        Item.Trimming.EndPoint = point;

        //　終了位置のサムネイル取得
        TrimEndImage = MovieFileProcessor.GetThumbnailPath(Item.FilePath, point);
    }

    /// <summary>
    /// 前の動画に移動する
    /// </summary>
    [RelayCommand] private void PreviousMovie()
    {
        if(ItemIndex is null) return;

        if(0 < ItemIndex)
        {
            ItemIndex--;
            Item = ItemsSource[ItemIndex.Value];
        }
    }

    /// <summary>
    /// 次の動画に移動する
    /// </summary>
    [RelayCommand] private void NextMovie()
    {
        if(ItemIndex is null) return;

        if(ItemIndex < ItemsSource.Count - 1)
        {
            ItemIndex++;
            Item = ItemsSource[ItemIndex.Value];
        }
    }

    /// <summary>
    /// Infoが変更されたときの処理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    partial void OnItemChanged(ItemInfo? value)
    {
        // Infoがnullのときは何もしない
        if (value is null) return;

        // 開始位置のサムネイル取得
        TrimStartImage = MovieFileProcessor.GetThumbnailPath(value.FilePath, TimeSpan.Zero);

        // 終了位置の時刻（動画長さ - 1フレーム）を取得
        var endPosition = value.OriginalInfo.Duration.TotalSeconds - 1 / value.OriginalInfo.FrameRate;

        // 終了位置のサムネイル取得
        TrimEndImage = MovieFileProcessor.GetThumbnailPath(value.FilePath, TimeSpan.FromSeconds(endPosition));
    }
}
