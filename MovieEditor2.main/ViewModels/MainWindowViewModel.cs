using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.IndividualUI.ViewModels;
using MovieEditor2.main.Library;
using MovieEditor2.Models;
using MovieEditor2.MovieListUI.ViewModels;

using MyControls;

namespace MovieEditor2.main.ViewModels;

internal partial class MainWindowViewModel : ObservableObject, IDisposable
{
    /// <summary> 処理進捗ダイアログの識別文字列 </summary>
    public static readonly string ProgressDialogIdentifier = "ProgressDialog";

    /// <summary> 処理済みファイル削除確認ダイアログの識別文字列 </summary>
    public static readonly string DeleteCompletedDialogIdentifier = "DeleteCompletedDialog";

    public MainWindowViewModel(UserSetting userSetting)
    {
        // 設定値反映
        OutputDirectory = userSetting.OutputDirectory;

        // サブViewModelの初期化
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
        // IndividualSlideUI.SelectItem(1);
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// 設定値の変更を設定値ファイルに反映する
    /// </summary>
    /// <param name="setting"></param>
    public void SaveSetting(UserSetting setting)
    {
        setting.OutputDirectory = OutputDirectory;
    }

    /// <summary> メディアファイルリスト </summary>
    public ObservableCollection<ItemInfo> Items { get; } = [];

    public MovieFilesDataGridViewModel MovieFilesUI { get; }

    public CommonSettingBoardViewModel CommonSettingUI { get; } = new();

    public IndividualSlideViewModel IndividualSlideUI { get; }

    // 遷移画面インデックス
    [ObservableProperty] private int _slideIndex = 0;

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
    [RelayCommand]
    private void HomeSlide()
    {
        SlideIndex = 0;
    }

    /// <summary>
    /// 実行コマンド
    /// </summary>
    [RelayCommand]
    private async Task Run()
    {
        // チェックが入っているアイテムだけを入力とする
        var sources = Items.Where(item => item.IsSelected).ToArray();

        using var process = new ParallelCommandProcessor();

        // 処理進捗ダイアログの表示
        _ = OpenProgressDialog(process, sources.Length);

        // 並列処理実行
        await Task.Run(() => process.RunParallelly(sources, CommonSettingUI, OutputDirectory));

        // 並列処理が終了したらダイアログを閉じる
        DialogHost.Close(ProgressDialogIdentifier);

        // 「処理済みファイルをリストから削除しますか？」
        var isDeleteRequested = await OpenDeleteCompletedDialog();

        // 処理済みファイルをリストから削除する場合
        if(isDeleteRequested)
        {
            foreach(var file in process.CompletedFiles)
            {
                Items.Remove(file);
            }
        }
    }

    /// <summary>
    /// 処理進捗ダイアログを表示する
    /// </summary>
    /// <param name="processor"></param>
    /// <param name="processCount"></param>
    /// <returns></returns>
    private static async Task OpenProgressDialog(ParallelCommandProcessor processor, int processCount)
    {
        // ダイアログのViewModel生成
        var viewModel = new ProgressDialogViewModel(processCount);

        // ダイアログのキャンセルボタンをおしたら並列処理をキャンセルさせる
        viewModel.OnCanceled += processor.Cancel;

        // 並列処理の進捗状況をダイアログに反映させる
        processor.OnProgressed += viewModel.UpdateProgress;

        // ダイアログのViewを生成
        object? view = new ProcessDialog { DataContext = viewModel };

        // ダイアログ表示
        await DialogHost.Show(view, ProgressDialogIdentifier, null, null, null);
    }

    /// <summary>
    /// 処理済みファイル削除確認ダイアログを表示する
    /// </summary>
    /// <returns>「はい」ならばtrue、「いいえ」ならばfalse</returns>
    private static async Task<bool> OpenDeleteCompletedDialog()
    {
        // ダイアログのView生成
        object? view = new YesNoMessageBox { Text = "処理済みファイルをリストから削除しますか？" };

        // ダイアログ表示
        object? result = await DialogHost.Show(view, DeleteCompletedDialogIdentifier, null, null, null);

        // 「はい」か「いいえ」を押せばここにはこないはず
        if (result is null)
        {
            System.Diagnostics.Debug.WriteLine("result is null");
            throw new NullReferenceException();
        }
        else
        {
            // 結果を返す
            return (bool)result;
        }

    }
}
