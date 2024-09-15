using System.IO;
using System.Windows;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.Models;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.Library;

internal class FFmpegCommandConverter
{
    /// <summary>
    /// 動画圧縮用コマンド生成処理
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="setting"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="isOnly">出力ファイルの重複を回避する？</param>
    /// <returns></returns>
    public static string ToCompressCommand(ItemInfo itemInfo, CommonSettingBoardViewModel setting, string outputDirectory, bool isOnly = true)
    {
        var argList = new List<string>
        {
            // 入力ファイルパス
            $"-y -i \"{itemInfo.FilePath}\""
        };

        // スケールのコマンドを取得
        var scaleArg = ToScaleArg(itemInfo.OriginalInfo, setting.Width, setting.Height);

        // スケール指定
        if(scaleArg is not null)
        {
            argList.Add($"-vf {scaleArg}");
        }

        // クリッピング指定
        if(itemInfo.Clipping != Rect.Empty)
        {
            argList.Add($"-vf crop={itemInfo.Clipping.Width:F2}:{itemInfo.Clipping.Height:F2}:{itemInfo.Clipping.X:F2}:{itemInfo.Clipping.Y:F2}");
        }

        // フレームレート
        if(0 < setting.FrameRate)
        {
            argList.Add($"-r {setting.FrameRate}");
        }

        // 動画コーデック
        if(string.IsNullOrWhiteSpace(setting.Codec) == false)
        {
            argList.Add($"-c:v {setting.Codec}");
        }

        // 音声削除
        if(setting.AudioDisable)
        {
            argList.Add("-an");
        }

        // トリミング開始位置
        if(itemInfo.Trimming.StartPoint is not null)
        {
            argList.Add($"-ss {itemInfo.Trimming.StartPoint:hh\\:mm\\:ss\\.fff}");
        }

        // トリミング終了位置
        if(itemInfo.Trimming.EndPoint is not null)
        {
            argList.Add($"-to {itemInfo.Trimming.EndPoint:hh\\:mm\\:ss\\.fff}");
        }

        // 速度倍率
        if(itemInfo.Speed is not null && itemInfo.Speed > 0)
        {
            argList.Add($"-vf setpts=PTS/{itemInfo.Speed} -af atempo={itemInfo.Speed}");
        }

        // 出力先指定
        var output = Path.Combine(outputDirectory, $"{itemInfo.FileNameWithoutExtension}C{itemInfo.CloneCount}{setting.Extension}");

        if(isOnly)
        {
            // 重複しないファイルパスに書き換える
            MovieFileProcessor.ToNonDuplicatePath(ref output);
        }

        argList.Add($"\"{output}\"");

        return string.Join(" ", argList);
    }

    /// <summary>
    /// 動画圧縮用コマンド生成処理
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="setting"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="isOnly">出力ファイルの重複を回避する？</param>
    /// <returns></returns>
    public static string ToCompressCommand(ItemInfo itemInfo, CommonSettingBoardViewModel setting, string outputDirectory)
    {
        return ToCompressCommand(itemInfo, setting, outputDirectory, true);
    }

    /// <summary>
    /// 画像出力用コマンド生成処理
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="setting"></param>
    /// <param name="outputDirectory"></param>
    /// <returns></returns>
    public static string ToImagesCommand(ItemInfo itemInfo, CommonSettingBoardViewModel setting, string outputDirectory)
    {
        var argList = new List<string>
        {
            // 入力ファイルパス
            $"-y -i \"{itemInfo.FilePath}\""
        };

        // 1秒間のフレーム数
        if(setting.FramePerSecond >= 1)
        {
            argList.Add($"-r {setting.FramePerSecond}");
        }

        // 総フレーム数
        if(setting.FrameSum >= 1)
        {
            argList.Add($"-vframes {setting.FrameSum}");
        }

        // 画像品質（数が大きいほど品質が低下し、容量が少なくなる）
        if(setting.Quality >= 0)
        {
            argList.Add($"-q:v {setting.Quality}");
        }

        // トリミング開始位置
        if(itemInfo.Trimming.StartPoint is not null)
        {
            argList.Add($"-ss {itemInfo.Trimming.StartPoint:hh\\:mm\\:ss\\.fff}");
        }

        // トリミング終了位置
        if(itemInfo.Trimming.EndPoint is not null)
        {
            argList.Add($"-to {itemInfo.Trimming.EndPoint:hh\\:mm\\:ss\\.fff}");
        }

        // 画像出力フォルダの作成
        Directory.CreateDirectory(Path.Combine(outputDirectory, itemInfo.FileNameWithoutExtension));

        // 出力先指定
        var output = Path.Combine(outputDirectory, itemInfo.FileNameWithoutExtension, $"{itemInfo.FileNameWithoutExtension}C{itemInfo.CloneCount}_%06d.{setting.ImageFormat}");

        // 重複しないファイルパスに書き換える
        MovieFileProcessor.ToNonDuplicatePath(ref output);

        argList.Add($"\"{output}\"");

        return string.Join(" ", argList);
    }

    /// <summary>
    /// スケールのコマンドを作成する
    /// </summary>
    /// <remarks>
    /// 値が0以下のときはAutoとする
    /// </remarks>
    /// <param name="info"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    private static string? ToScaleArg(MovieInfo info, int width, int height)
    {
        // どちらも正値のときはそのまま
        if (0 < width && 0 < height)
        {
            return $"scale={width}:{height}";
        }
        // widthはAutoでheight指定のとき
        else if (0 >= width && 0 < height)
        {
            // 動画ファイルの解像度情報からWidthを自動計算
            int autoWidth = 0;

            // 0除算回避
            if (0 != info.Height)
            {
                autoWidth = height * info.Width / info.Height;
            }

            // 解像度は偶数にする必要がある。
            if (0 != autoWidth % 2)
            {
                autoWidth++;
            }

            return $"scale={autoWidth}:{height}";
        }
        // width指定でheightはAutoのとき
        else if (0 < width && 0 >= height)
        {
            // 動画ファイルの解像度情報からHeightを自動計算
            int autoHeight = 0;

            // 0除算回避
            if (0 != info.Width)
            {
                autoHeight = width * info.Height / info.Width;
            }

            // 解像度は偶数にする必要がある。
            if (0 != autoHeight % 2)
            {
                autoHeight++;
            }

            return $"scale={width}:{autoHeight}";
        }
        else
        {
            return null;
        }
    }
}
