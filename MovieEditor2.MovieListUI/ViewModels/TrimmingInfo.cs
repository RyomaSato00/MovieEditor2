using CommunityToolkit.Mvvm.ComponentModel;

namespace MovieEditor2.MovieListUI.ViewModels;

public partial class TrimmingInfo : ObservableObject
{
    // トリミング開始位置
    [ObservableProperty] private TimeSpan? _startPoint = null;

    // トリミング終了位置
    [ObservableProperty] private TimeSpan? _endPoint = null;

    /// <summary> 動画長さ </summary>
    [ObservableProperty] private TimeSpan _duration;

    /// <summary> 元動画の動画長さ </summary>
    private readonly TimeSpan _originalDuration;

    /// <summary>
    ///
    /// </summary>
    /// <param name="originalDuration">元動画の動画長さ</param>
    public TrimmingInfo(TimeSpan originalDuration)
    {
        _originalDuration = originalDuration;
        Duration = originalDuration;
    }

    /// <summary>
    /// StartPointが変更されたときの処理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    partial void OnStartPointChanged(TimeSpan? value)
    {
        // トリミング開始位置。nullのときは0
        var startPoint = value ?? TimeSpan.Zero;

        // トリミング終了位置。nullのときは元動画の終了位置
        var endPoint = EndPoint ?? _originalDuration;

        // 動画長さを取得
        Duration = endPoint - startPoint;
    }

    /// <summary>
    /// EndPointが変更されたときの処理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    partial void OnEndPointChanged(TimeSpan? value)
    {
        // トリミング開始位置。nullのときは0
        var startPoint = StartPoint ?? TimeSpan.Zero;

        // トリミング終了位置。nullのときは元動画の終了位置
        var endPoint = value ?? _originalDuration;

        // 動画長さを取得
        Duration = endPoint - startPoint;
    }
}
