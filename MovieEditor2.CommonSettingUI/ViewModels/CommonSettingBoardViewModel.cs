using CommunityToolkit.Mvvm.ComponentModel;

namespace MovieEditor2.CommonSettingUI.ViewModels;

public partial class CommonSettingBoardViewModel : ObservableObject
{
    // 動画圧縮の共通設定

    [ObservableProperty] private int _width = -1;

    [ObservableProperty] private int _height = -1;

    [ObservableProperty] private float _frameRate = -1;

    [ObservableProperty] private string _codec = "hevc";

    [ObservableProperty] private string _extension = ".mp4";

    [ObservableProperty] private bool _audioDisable = true;


    // 画像出力の共通設定

    [ObservableProperty] private string _imageFormat = "png";

    [ObservableProperty] private int _framePerSecond = 5;

    [ObservableProperty] private int _frameSum = -1;

    [ObservableProperty] private int _quality = 0;
}
