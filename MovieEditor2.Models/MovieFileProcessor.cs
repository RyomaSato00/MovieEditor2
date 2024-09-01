using System.Diagnostics;

namespace MovieEditor2.Models;

public class MovieFileProcessor
{
    /// <summary> サムネイル画像を置いておくディレクトリ </summary>
    public static readonly string ThumbnailCacheDir = @"cache\thumbnails";

    /// <summary>
    /// 動画からサムネイル用の画像(jpg)を生成し、そのパスを返す
    /// </summary>
    /// <remarks>
    /// ※この関数の使用にはffmpegが必要です
    /// </remarks>
    /// <param name="filePath"></param>
    /// <param name="position">サムネイルに採用する動画再生位置</param>
    /// <returns></returns>
    public static string GetThumbnailPath(string filePath, TimeSpan position)
    {
        // サムネイル画像を置いておくディレクトリが未作成のときはディレクトリを作成する
        if (false == Directory.Exists(ThumbnailCacheDir))
        {
            Directory.CreateDirectory(ThumbnailCacheDir);
        }

        // サムネイル画像のパス
        var thumbnailImagePath = Path.Combine(ThumbnailCacheDir, $"{Path.GetFileNameWithoutExtension(filePath)}.jpg");

        // ファイルパスの重複を回避する
        ToNonDuplicatePath(ref thumbnailImagePath);

        // ffmpegを使用して動画から画像を生成するためのコマンド
        var startInfo = new ProcessStartInfo("ffmpeg")
        {
            Arguments = $"-y -i \"{filePath}\" -ss {position:hh\\:mm\\:ss\\.fff} -vframes 1 -q:v 0 \"{thumbnailImagePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // キャッシュフォルダにサムネイル用のjpgファイルを生成する
        using var process = new Process() { StartInfo = startInfo };
        process.Start();
        process.WaitForExit();

        // フルパスを返す
        return Path.GetFullPath(thumbnailImagePath);
    }

    /// <summary>
    /// 重複しないファイルパスに書き換える
    /// </summary>
    /// <param name="filePath"></param>
    public static void ToNonDuplicatePath(ref string filePath)
    {
        // ファイルパスが重複しないならば何もしない
        if (false == File.Exists(filePath)) return;

        // ファイルパスが重複するならば新しいファイル名に書き換える

        // ファイルが置かれているディレクトリパス
        var directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;

        // 拡張子を除いたファイル名
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        // 拡張子
        var extension = Path.GetExtension(filePath);

        // 「重複回避番号」
        int duplicateCount = 1;

        // 重複しないファイルパスが見つかるまで番号を増やす
        do
        {
            // ファイルパス ＝ ディレクトリパス/ファイル名(重複回避番号).拡張子
            filePath = Path.Combine(
                directoryPath,
                $"{fileName}({duplicateCount}){extension}"
            );
            duplicateCount++;

        // ファイルパスが存在するなら重複回避番号をインクリメントして再試行
        } while (File.Exists(filePath));
    }

    /// <summary>
    /// サムネイル用に生成した画像ファイルをすべて削除する
    /// </summary>
    public static void DeleteThumbnailCaches()
    {
        // キャッシュディレクトリがないなら、何もしない
        if (false == Directory.Exists(ThumbnailCacheDir)) return;

        // キャッシュディレクトリ内のファイルパスをすべて取得
        string[] files = Directory.GetFiles(ThumbnailCacheDir);

        // キャッシュファイルを削除
        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (FileNotFoundException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
        }
    }
}
