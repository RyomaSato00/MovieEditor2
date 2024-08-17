using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.ViewModels;

internal partial class MainWindowViewModel : ObservableObject, IDisposable
{
    public MainWindowViewModel()
    {
        // MovieFilesUI.ItemsSource.Add(new ItemInfo{ Code = "A", ThumbnailPath=@"C:\Users\Ryoma\OneDrive\画像\スクリーンショット\001949.png", FileName = @"C:\Users\Ryoma\OneDrive\画像\スクリーンショット\001949.png"});
        // MovieFilesUI.ItemsSource.Add(new ItemInfo{ Code = "B", ThumbnailPath = @"C:\Users\Ryoma\OneDrive\画像\カメラ ロール\チェンソーマン\0.png", FileName = @"C:\Users\Ryoma\OneDrive\画像\カメラ ロール\チェンソーマン\0.png"});
        // MovieFilesUI.ItemsSource.Add(new ItemInfo{ Code = "C", ThumbnailPath = @"C:\Users\Ryoma\OneDrive\画像\カメラ ロール\チェンソーマン\2.png", FileName = @"C:\Users\Ryoma\OneDrive\画像\カメラ ロール\チェンソーマン\2.png"});
    }

    public void Dispose()
    {
    }

    /// <summary> DataGridにあるファイルの数 </summary>
    public int FilesCount => MovieFilesUI.ItemsSource.Count;


    public MovieFilesDataGridViewModel MovieFilesUI { get; } = new();

    public CommonSettingBoardViewModel CommonSettingUI { get; } = new();

    // 遷移画面インデックス
    [ObservableProperty] private int _slideIndex = 0;

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
