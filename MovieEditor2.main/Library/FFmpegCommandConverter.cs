using System.IO;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.Models;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.Library;

internal class FFmpegCommandConverter
{
    public static string ToCompressCommand(ItemInfo itemInfo, CommonSettingBoardViewModel setting, string outputDirectory)
    {
        var argList = new List<string>();

        // 入力ファイルパス
        argList.Add($"-y -i \"{itemInfo.FilePath}\"");

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
        var output = Path.Combine(outputDirectory, itemInfo.FileName);

        // 重複しないファイルパスに書き換える
        MovieFileProcessor.ToNonDuplicatePath(ref output);

        argList.Add($"\"{output}\"");

        return string.Join(" ", argList);
    }
}
