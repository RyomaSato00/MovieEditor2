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

        // vfコマンドを取得
        var vf = ToVf(itemInfo, setting);

        if (vf is not null)
        {
            argList.Add(vf);
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
    /// VFコマンド生成処理
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="setting"></param>
    /// <param name="outputDirectory"></param>
    /// <returns></returns>
    private static string? ToVf(ItemInfo itemInfo, CommonSettingBoardViewModel setting)
    {
        // スケール指定あり？
        var isScaleReserved = setting.Width >= 0 || setting.Height >= 0;

        // クリッピング指定あり？
        var isClippingReserved = itemInfo.Clipping != Rect.Empty;

        // 回転指定あり？
        var isRotationReserved = itemInfo.Rotation != RotationID.Default;

        // 速度倍率指定あり？
        var isSpeedReserved = itemInfo.Speed is not null && itemInfo.Speed > 0;

        // すべて指定なしのとき
        if (isScaleReserved == false && isClippingReserved == false && isRotationReserved == false && isSpeedReserved == false)
        {
            return null;
        }
        else
        {
            var vfList = new List<string>();
            var vfSubList = new List<string>();
            double clipWidth = 0, clipHeight = 0;

            // クリッピング指定あり
            if (isClippingReserved)
            {
                var cropArg = ToCrop(itemInfo, out clipWidth, out clipHeight);

                if (cropArg is not null)
                {
                    vfList.Add(cropArg);
                }
            }

            // スケール指定あり
            if (isScaleReserved)
            {
                // 元動画のサイズを取得
                var originWidth = itemInfo.OriginalInfo.Width;
                var originHeight = itemInfo.OriginalInfo.Height;

                // クリッピング指定あり？
                if (isClippingReserved)
                {
                    // 元動画のサイズはクリッピング後のサイズとする
                    originWidth = (int)clipWidth;
                    originHeight = (int)clipHeight;
                }

                var scaleArg = ToScale(setting.Width, setting.Height, originWidth, originHeight);

                if (scaleArg is not null)
                {
                    vfList.Add(scaleArg);
                }
            }

            // 回転指定あり
            if (isRotationReserved)
            {
                var rotateArg = ToRotate(itemInfo.Rotation);

                if (rotateArg is not null)
                {
                    vfList.Add(rotateArg);

                    // 回転後の角度を0度と定義する
                    vfSubList.Add("-metadata:s:v:0 rotate=0");
                }
            }

            // 速度倍率指定あり
            if (isSpeedReserved)
            {
                vfList.Add($"setpts=PTS/{itemInfo.Speed}");

                // 音声の速度も同時に変更
                vfSubList.Add($"-af atempo={itemInfo.Speed}");
            }

            return $"-vf \"{string.Join(',', vfList)}\" {string.Join(' ', vfSubList)}";
        }
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
    private static string? ToScale(int width, int height, int originWidth, int originHeight)
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
            if (0 != originHeight)
            {
                autoWidth = height * originWidth / originHeight;
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
            if (0 != originWidth)
            {
                autoHeight = width * originHeight / originWidth;
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

    /// <summary>
    /// クリッピングのコマンドを作成する
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private static string? ToCrop(ItemInfo info, out double clipWidth, out double clipHeight)
    {
        clipWidth = 0;
        clipHeight = 0;

        // クリッピング指定なしのときはnullを返す
        if (info.Clipping == Rect.Empty) return null;

        var x = info.Clipping.X >= 0 ? info.Clipping.X : 0;

        var y = info.Clipping.Y >= 0 ? info.Clipping.Y : 0;

        clipWidth = info.Clipping.Width + x <= info.OriginalInfo.Width ? info.Clipping.Width : info.OriginalInfo.Width - x;

        clipHeight = info.Clipping.Height + y <= info.OriginalInfo.Height ? info.Clipping.Height : info.OriginalInfo.Height - y;

        return $"crop={clipWidth:F2}:{clipHeight:F2}:{x:F2}:{y:F2}";
    }

    /// <summary>
    /// 回転のコマンドを生成する
    /// </summary>
    /// <param name="rotate"></param>
    /// <returns></returns>
    private static string? ToRotate(RotationID rotate)
    {
        return rotate switch
        {
            RotationID.Default => null,
            RotationID.R90 => "transpose=1",
            RotationID.R180 => "transpose=1,transpose=1",
            RotationID.L90 => "transpose=2",
            _ => null
        };
    }
}
