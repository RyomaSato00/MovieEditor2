
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;

using MovieEditor2.Models;

namespace MovieEditor2.MovieListUI.ViewModels;

/// <summary>
/// ItemsSource各々の型定義
/// </summary>
public partial class ItemInfo : ObservableObject
{
    /// <summary> IsSelectedが変更されたときのイベントハンドラ </summary>
    public event EventHandler? IsSelectedChanged;

    [ObservableProperty] private bool _isSelected = false;

    /// <summary> 動画ファイルパス </summary>
    public string FilePath { get; }

    /// <summary> 動画ファイル名 </summary>
    public string FileName { get; }

    /// <summary> サムネイル用画像ファイルのパス </summary>
    public string ThumbnailPath { get; }

    /// <summary> 元動画情報 </summary>
    public MovieInfo OriginalInfo { get; }

    /// <summary> 動画トリミング情報 </summary>
    public TrimmingInfo Trimming { get; }

    public ItemInfo(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileNameWithoutExtension(filePath);

        OriginalInfo = MovieInfo.ToMovieInfo(filePath);

        ThumbnailPath = MovieFileProcessor.GetThumbnailPath(filePath, TimeSpan.Zero);

        Trimming = new TrimmingInfo(OriginalInfo.Duration);
    }

    /// <summary> IsSelectedが変更されたときの処理 </summary>
    partial void OnIsSelectedChanged(bool value)
    {
        IsSelectedChanged?.Invoke(this, new EventArgs());
    }
}
