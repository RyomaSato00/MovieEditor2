
using System.IO;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

using MovieEditor2.Models;

namespace MovieEditor2.MovieListUI.ViewModels;

public partial class ItemInfo : ObservableObject
{
    /// <summary> IsSelectedが変更されたときのイベントハンドラ </summary>
    public event EventHandler? IsSelectedChanged;

    [ObservableProperty] private bool _isSelected = false;

    /// <summary> 動画ファイルパス </summary>
    public string FilePath { get; }

    /// <summary> 動画ファイル名 </summary>
    public string FileName { get; }

    /// <summary> 動画ファイル名（拡張子なし） </summary>
    public string FileNameWithoutExtension { get; }

    /// <summary> 複製回数 </summary>
    public int CloneCount { get; }

    /// <summary> 元動画情報 </summary>
    public MovieInfo OriginalInfo { get; }

    /// <summary> 動画トリミング情報 </summary>
    public TrimmingInfo Trimming { get; }

    /// <summary> 動画クリッピング情報 </summary>
    [ObservableProperty] private Rect _clipping = Rect.Empty;

    /// <summary> 動画回転情報 </summary>
    public RotationID Rotation { get; set; } = RotationID.Default;

    // 速度倍率
    [ObservableProperty] private double? _speed = null;

    public ItemInfo(string filePath, int cloneCount = 0)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

        OriginalInfo = MovieInfo.ToMovieInfo(filePath);

        Trimming = new TrimmingInfo(OriginalInfo.Duration, OriginalInfo.FrameRate, filePath);

        CloneCount = cloneCount;
    }

    /// <summary> IsSelectedが変更されたときの処理 </summary>
    partial void OnIsSelectedChanged(bool value)
    {
        IsSelectedChanged?.Invoke(this, new EventArgs());
    }
}

public enum RotationID
{
    /// <summary> 回転なし </summary>
    Default,

    /// <summary> 右に90度回転 </summary>
    R90,

    /// <summary> 右に180度回転 </summary>
    R180,

    /// <summary> 左に90度回転 </summary>
    L90,

    Count
}
