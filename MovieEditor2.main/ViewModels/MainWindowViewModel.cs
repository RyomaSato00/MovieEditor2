using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.IndividualUI.ViewModels;
using MovieEditor2.main.Library;
using MovieEditor2.Models;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.ViewModels;

internal partial class MainWindowViewModel : ObservableObject, IDisposable
{
    public MainWindowViewModel()
    {
        MovieFilesUI = new MovieFilesDataGridViewModel(Items);
        IndividualSlideUI = new IndividualSlideViewModel(Items);

        // リストから個別編集に画面へ遷移するイベント定義
        MovieFilesUI.OnEditIndividual += index =>
        {
            IndividualSlideUI.SelectItem(index);
            IndividualSlide();
        };

        Items.Add(new ItemInfo(@"C:\Users\Ryoma\Videos\Captures\AGDRec\AGDRec_20231009_012700.mp4"));
        Items.Add(new ItemInfo(@"C:\Users\Ryoma\Videos\Captures\AGDRec\AGDRec_20231009_012601.mp4"));
        // IndividualSlideUI.Item = Items[0];
        IndividualSlideUI.SelectItem(0);
        // IndividualSlideUI.Item = Items[1];
        IndividualSlideUI.SelectItem(1);
    }

    public void Dispose()
    {
    }

    /// <summary> メディアファイルリスト </summary>
    public ObservableCollection<ItemInfo> Items { get; } = [];

    public MovieFilesDataGridViewModel MovieFilesUI { get; }

    public CommonSettingBoardViewModel CommonSettingUI { get; } = new();

    public IndividualSlideViewModel IndividualSlideUI { get; }

    // 遷移画面インデックス
    [ObservableProperty] private int _slideIndex = 1;

    // ファイル出力先ディレクトリ
    [ObservableProperty] private string _outputDirectory = string.Empty;

    /// <summary>
    /// 個別編集画面へ遷移する
    /// </summary>
    private void IndividualSlide()
    {
        SlideIndex = 1;
    }

    /// <summary>
    /// ホーム画面へ遷移する
    /// </summary>
    [RelayCommand] private void HomeSlide()
    {
        SlideIndex = 0;
    }

    /// <summary>
    /// 実行コマンド
    /// </summary>
    [RelayCommand] private void Run()
    {
        using var process = new ParallelCommandProcessor();

        process.RunParallelly(
            Items
                .Where(item => item.IsSelected)
                .Select(item => FFmpegCommandConverter.ToCompressCommand(item, CommonSettingUI, OutputDirectory))
                .ToArray()
        );
    }
}
