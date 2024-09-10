using System.IO;

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
    [ObservableProperty] private string _trimStartImage = string.Empty;

    /// <summary> トリミング終了位置のサムネイル </summary>
    [ObservableProperty] private string _trimEndImage = string.Empty;

    /// <summary> 元動画の動画長さ </summary>
    private readonly TimeSpan _originalDuration;

    /// <summary> ファイルパス </summary>
    private readonly string _filePath;

    /// <summary>
    ///
    /// </summary>
    /// <param name="originalDuration">元動画の動画長さ</param>
    public TrimmingInfo(TimeSpan originalDuration, double frameRate, string filePath)
    {
        _originalDuration = originalDuration;
        Duration = originalDuration;
        _filePath = filePath;
        var name = Path.GetFileNameWithoutExtension(filePath);

        // 開始位置のサムネイル取得
        UpdateStartImage(filePath, name, TimeSpan.Zero);


        // 終了位置の時刻（動画長さ - 1フレーム）を取得
        var endPosition = originalDuration.TotalSeconds - 1 / frameRate;

        // 終了位置のサムネイル取得
        UpdateEndImage(filePath, name, TimeSpan.FromSeconds(endPosition));
    }

    /// <summary>
    /// トリミング開始位置のサムネイルを更新する
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="point"></param>
    public void UpdateStartImage(string filePath, string fileName, TimeSpan point)
    {
        var name = $"{fileName}_{point:hhmmssfff}";
        TrimStartImage = MovieFileProcessor.GetThumbnailPath(filePath, name, point);
    }

    /// <summary>
    /// トリミング終了位置のサムネイルを更新する
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="point"></param>
    public void UpdateEndImage(string filePath, string fileName, TimeSpan point)
    {
        var name =$"{fileName}_{point:hhmmssfff}";
        TrimEndImage = MovieFileProcessor.GetThumbnailPath(filePath, name, point);
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
