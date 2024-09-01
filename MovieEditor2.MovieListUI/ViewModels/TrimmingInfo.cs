using CommunityToolkit.Mvvm.ComponentModel;

using MovieEditor2.Models;

namespace MovieEditor2.MovieListUI.ViewModels;

public partial class TrimmingInfo : ObservableObject
{
    // トリミング開始位置
    [ObservableProperty] private TimeSpan? _startPoint = null;

    // トリミング終了位置
    [ObservableProperty] private TimeSpan? _endPoint = null;

    /// <summary> 動画長さ </summary>
    [ObservableProperty] private TimeSpan _duration;

    /// <summary> トリミング開始位置のサムネイル </summary>
    [ObservableProperty] private string _trimStartImage;

    /// <summary> トリミング終了位置のサムネイル </summary>
    [ObservableProperty] private string _trimEndImage;

    /// <summary> 元動画の動画長さ </summary>
    private readonly TimeSpan _originalDuration;

    /// <summary>
    ///
    /// </summary>
    /// <param name="originalDuration">元動画の動画長さ</param>
    public TrimmingInfo(TimeSpan originalDuration, double frameRate, string filePath)
    {
        _originalDuration = originalDuration;
        Duration = originalDuration;

        // 開始位置のサムネイル取得
        TrimStartImage = MovieFileProcessor.GetThumbnailPath(filePath, TimeSpan.Zero);

        // 終了位置の時刻（動画長さ - 1フレーム）を取得
        var endPosition = originalDuration.TotalSeconds - 1 / frameRate;

        // 終了位置のサムネイル取得
        TrimEndImage = MovieFileProcessor.GetThumbnailPath(filePath, TimeSpan.FromSeconds(endPosition));
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
