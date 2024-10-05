using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using MovieEditor2.IndividualUI.ViewModels;
using MovieEditor2.Models;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.IndividualUI;

/// <summary>
/// Interaction logic for MovieArea.xaml
/// </summary>
public partial class MovieArea : UserControl
{
    /// <summary> 動画Storyboard読み込みイベント </summary>
    public event Action<Storyboard>? OnStoryboardLoaded = null;

    /// <summary> 動画読み込み完了イベント </summary>
    public event Action<Duration>? OnMediaOpened = null;

    /// <summary> 動画フレーム更新イベント </summary>
    public event Action<TimeSpan>? OnCurrentTimeInvalidated = null;

    /// <summary> 動画再生終了イベント </summary>
    public event Action? OnCompleted = null;

    /// <summary> MediaElementサイズ変更イベント </summary>
    public event Action<Size>? OnMediaElementSizeChanged = null;

    public static readonly DependencyProperty UpVisibilityProperty =
    DependencyProperty.Register(nameof(UpVisibility), typeof(Visibility), typeof(MovieArea), new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty DownVisibilityProperty =
    DependencyProperty.Register(nameof(DownVisibility), typeof(Visibility), typeof(MovieArea), new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty UpCommandProperty =
    DependencyProperty.Register(nameof(UpCommand), typeof(ICommand), typeof(MovieArea), new PropertyMetadata(null));

    public static readonly DependencyProperty DownCommandProperty =
    DependencyProperty.Register(nameof(DownCommand), typeof(ICommand), typeof(MovieArea), new PropertyMetadata(null));


    /// <summary> 「前へ」ボタンの表示・非表示 </summary>
    public Visibility UpVisibility
    {
        get => (Visibility)GetValue(UpVisibilityProperty);
        set => SetValue(UpVisibilityProperty, value);
    }

    /// <summary> 「次へ」ボタンの表示・非表示 </summary>
    public Visibility DownVisibility
    {
        get => (Visibility)GetValue(DownVisibilityProperty);
        set => SetValue(DownVisibilityProperty, value);
    }

    /// <summary> 「前へ」ボタン押下コマンド </summary>
    public ICommand UpCommand
    {
        get => (ICommand)GetValue(UpCommandProperty);
        set => SetValue(UpCommandProperty, value);
    }

    /// <summary> 「次へ」ボタン押下コマンド </summary>
    public ICommand DownCommand
    {
        get => (ICommand)GetValue(DownCommandProperty);
        set => SetValue(DownCommandProperty, value);
    }

    /// <summary> 動画のStoryboard保持用フィールド </summary>
    private readonly Storyboard _story;

    public MovieArea()
    {
        InitializeComponent();

        // Storyboard取得
        _story = (Storyboard)FindResource("MovieStory");
    }

    /// <summary>
    /// このUserControlをLoadしたときに行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MovieArea_Loaded(object sender, RoutedEventArgs e)
    {
        // StoryboardをViewModelに渡す
        OnStoryboardLoaded?.Invoke(_story);
    }

    /// <summary>
    /// 動画読み込み完了時に行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoviePlayer_MediaOpened(object sender, EventArgs e)
    {
        if(sender is not MediaElement mediaElement) return;

        // 動画再生時間をViewModelに渡す
        OnMediaOpened?.Invoke(MoviePlayer.NaturalDuration);
    }

    /// <summary>
    /// 動画更新時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timeline_CurrentTimeInvalidated(object sender, EventArgs e)
    {
        // 動画の現在時間をViewModelに渡す
        OnCurrentTimeInvalidated?.Invoke(MoviePlayer.Position);
    }

    /// <summary>
    /// 動画Completed時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timeline_Completed(object sender, EventArgs e)
    {
        // 動画の再生完了をViewModelに通知
        OnCompleted?.Invoke();
    }

    /// <summary>
    /// MediaElementサイズ変更時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoviePlayer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // MediaElementの新しいサイズを通知
        OnMediaElementSizeChanged?.Invoke(e.NewSize);
    }

}

