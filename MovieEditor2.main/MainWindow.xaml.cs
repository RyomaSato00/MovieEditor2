using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MovieEditor2.main.Library;
using MovieEditor2.main.ViewModels;
using MovieEditor2.Models;

namespace MovieEditor2.main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    /// <summary> 設定値 </summary>
    private readonly UserSetting _userSetting = new();

    /// <summary> 設定値ファイルパス </summary>
    private static readonly string JsonFilePath = "UserSetting.json";

    public MainWindow()
    {
        // キャッシュ削除
        MovieFileProcessor.DeleteThumbnailCaches();

        // 設定ファイルロード
        JsonHandler.LoadSettingFromJson(ref _userSetting, JsonFilePath);

        _mainWindowViewModel = new MainWindowViewModel(_userSetting);

        DataContext = _mainWindowViewModel;

        // クローズ処理を設定
        Closing += MainWindow_Closing;

        InitializeComponent();
    }

    /// <summary>
    /// クローズ処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        // 設定値の変更を取得
        _mainWindowViewModel.SaveSetting(_userSetting);

        // 設定値セーブ
        JsonHandler.SaveSettingToJson(_userSetting, JsonFilePath);
    }
}
