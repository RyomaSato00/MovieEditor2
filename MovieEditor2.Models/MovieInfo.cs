using FFMpegCore;

namespace MovieEditor2.Models;

public record MovieInfo
{
    /// <summary> 動画長さ </summary>
    public TimeSpan Duration { get; init; } = TimeSpan.Zero;

    /// <summary> 動画横幅 </summary>
    public int Width { get; init; } = 0;

    /// <summary> 動画縦幅 </summary>
    public int Height { get; init; } = 0;

    /// <summary> 動画フレームレート </summary>
    public double FrameRate { get; init; } = 0;

    /// <summary> 動画コーデック </summary>
    public string VideoCodec { get; init; } = string.Empty;

    /// <summary> 動画ファイルサイズ[byte] </summary>
    public long FileSize { get; init; } = 0;


    // 「動画ファイル」として対応している拡張子
    public static readonly IReadOnlyList<string> MovieFileExtension =
    [
        ".mp4",
        ".MP4",
        ".mov",
        ".MOV",
        ".agm",
        ".avi",
        ".wmv"
    ];

    /// <summary>
    /// ファイルパスから動画情報オブジェクトを生成する
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static MovieInfo ToMovieInfo(string filePath)
    {
        // 動画ファイルとして対応した拡張子か確認する
        if (false == MovieFileExtension.Contains(Path.GetExtension(filePath)))
        {
            throw new ArgumentOutOfRangeException(Path.GetExtension(filePath), "この拡張子は対応していません");
        }

        // FFProbeを使用して動画ファイル情報を取得する
        var mediaInfo = FFProbe.Analyse(filePath);
        var video = mediaInfo.PrimaryVideoStream ?? throw new InvalidDataException("このファイルは動画ファイルではない可能性があります");

        // MovieInfoオブジェクトを生成
        return new MovieInfo
        {
            Duration = video.Duration,
            Width = video.Width,
            Height = video.Height,
            FrameRate = video.FrameRate,
            VideoCodec = video.CodecName,
            FileSize = new FileInfo(filePath).Length
        };
    }
}
