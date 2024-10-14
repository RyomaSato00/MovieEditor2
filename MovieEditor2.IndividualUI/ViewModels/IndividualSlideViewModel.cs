using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

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

    /// <summary> 動画再生エリアのViewModel </summary>
    public MovieAreaViewModel MovieAreaUI { get; } = new();

    // ターゲット動画ファイル
    [ObservableProperty] private ItemInfo? _item = null;

    // ターゲット動画ファイルのインデックス
    [ObservableProperty] private int? _itemIndex = null;

    /// <summary>
    /// Item変更時処理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    partial void OnItemChanged(ItemInfo? value)
    {
        if (value is null) return;

        // 動画の再生準備
        MovieAreaUI.LoadMovie(value);
    }

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
    [RelayCommand] private void PinStartPoint()
    {
        if(Item is null) return;

        Item.Trimming.StartPoint = MovieAreaUI.CurrentTime;
    }

    /// <summary>
    /// トリミング終了位置を指定する
    /// </summary>
    /// <param name="point"></param>
    [RelayCommand] private void PinEndPoint()
    {
        if(Item is null) return;

        Item.Trimming.EndPoint = MovieAreaUI.CurrentTime;
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
    /// 左に90度回転させる
    /// </summary>
    [RelayCommand] private void LeftRotate()
    {
        if (Item is null) return;

        // 動画UIを左に90度回転
        MovieAreaUI.MovieAngle -= 90;

        // 回転状態を取得
        var angle = (int)Item.Rotation - 1;
        if (angle < 0) angle = (int)RotationID.Count - 1;

        Item.Rotation = (RotationID)angle;
    }

    /// <summary>
    /// 右に90度回転させる
    /// </summary>
    [RelayCommand] private void RightRotate()
    {
        if (Item is null) return;

        // 動画UIを右に90度回転
        MovieAreaUI.MovieAngle += 90;

        // 回転状態を取得
        var angle = (int)Item.Rotation + 1;
        if (angle >= (int)RotationID.Count) angle = 0;

        Item.Rotation = (RotationID)angle;
    }
}
