using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.IndividualUI.ViewModels;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.ViewModels;

internal partial class MainWindowViewModel : ObservableObject, IDisposable
{
    public MainWindowViewModel()
    {
        MovieFilesUI.ItemsSource.Add(new ItemInfo(@"C:\Users\Ryoma\Videos\Captures\AGDRec\AGDRec_20231009_012700.mp4"));
        MovieFilesUI.ItemsSource.Add(new ItemInfo(@"C:\Users\Ryoma\Videos\Captures\AGDRec\AGDRec_20231009_012601.mp4"));
        IndividualSlideUI.Item = MovieFilesUI.ItemsSource[0];
        IndividualSlideUI.Item = MovieFilesUI.ItemsSource[1];
    }

    public void Dispose()
    {
    }

    /// <summary> DataGridにあるファイルの数 </summary>
    public int FilesCount => MovieFilesUI.ItemsSource.Count;


    public MovieFilesDataGridViewModel MovieFilesUI { get; } = new();

    public CommonSettingBoardViewModel CommonSettingUI { get; } = new();

    public IndividualSlideViewModel IndividualSlideUI { get; } = new();

    // 遷移画面インデックス
    [ObservableProperty] private int _slideIndex = 1;

    /// <summary>
    /// 個別編集画面へ遷移する
    /// </summary>
    [RelayCommand] private void IndividualSlide()
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
}
