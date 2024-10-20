using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.MovieListUI.ViewModels;
using MovieEditor2.main.Library;

namespace MovieEditor2.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void コーデック指定のみ()
    {
        var item = new ItemInfo(@"C:\Users\Ryoma\Videos\Captures\AGDRec\AGDRec_20240728_121650.mp4");
        var setting = new CommonSettingBoardViewModel { Codec = "h264", AudioDisable = false};
        var command = FFmpegCommandConverter.ToCompressCommand(item, setting, @"C:\Users\Ryoma\Downloads");
        var answer = "-y -i \"C:\\Users\\Ryoma\\Videos\\Captures\\AGDRec\\AGDRec_20240728_121650.mp4\" -c:v h264 \"C:\\Users\\Ryoma\\Downloads\\AGDRec_20240728_121650C0.mp4\"";
        Assert.AreEqual(answer, command);
    }

    [TestMethod]
    public void クリッピングとスケール変更()
    {
        var item = new ItemInfo(@"C:\Users\Ryoma\Videos\Captures\AGDRec\AGDRec_20240905_232533.mp4");
        item.Clipping = new System.Windows.Rect
        {
            X = -32.96, Y = 154.27, Width = 684.39, Height = 862.35
        };
        var setting = new CommonSettingBoardViewModel { Height = 720, Codec = "hevc", AudioDisable = true};
        var command = FFmpegCommandConverter.ToCompressCommand(item, setting, @"C:\Users\Ryoma\Downloads");
        var answer = "-y -i \"C:\\Users\\Ryoma\\Videos\\Captures\\AGDRec\\AGDRec_20240905_232533.mp4\" -vf \"crop=600.00:862.35:0.00:154.27,scale=502:720\" -c:v hevc -an \"C:\\Users\\Ryoma\\Downloads\\AGDRec_20240905_232533C0.mp4\"";
        Assert.AreEqual(answer, command);
    }
}
