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

namespace MovieEditor2.IndividualUI;

/// <summary>
/// Interaction logic for MovieArea.xaml
/// </summary>
public partial class MovieArea : UserControl
{
    public static readonly DependencyProperty CurrentTimeProperty =
    DependencyProperty.Register(nameof(CurrentTime), typeof(TimeSpan), typeof(MovieArea), new PropertyMetadata(TimeSpan.Zero));

    public static readonly DependencyProperty FilePathProperty =
    DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(MovieArea), new PropertyMetadata(string.Empty, OnFilePathChanged));

    public static readonly DependencyProperty MaxTimeProperty =
    DependencyProperty.Register(nameof(MaxTime), typeof(TimeSpan), typeof(MovieArea), new PropertyMetadata(TimeSpan.Zero));

    /// <summary> 動画を0から再スタートするときのSliderのMaximumからみたValueの閾値 </summary>
    private static readonly double ResetEnableMsec = 10.0;

    /// <summary> ValueChangedイベントが呼ばれたときにSeekを実行するValue変化量の閾値 </summary>
    private static readonly double ValueChangedEnableMsec = 10.0;

    /// <summary> 動画の再生制御を行うStoryboard </summary>
    private readonly Storyboard _movieStory;

    /// <summary> オーディオボリューム量を一時保管するための変数 </summary>
    private double _audioVolume;

    /// <summary> 動画が最終時刻まで進んだときに立たせるフラグ </summary>
    private bool _movieCompleted = false;

    /// <summary> ValueChangedイベント時にSeekの実行可否を制御するためのフラグ </summary>
    private bool _isValueChangedEnabled = false;

    /// <summary>
    /// 動画の現在時刻<br/>
    /// ※依存プロパティにしないとUIに都度反映されない
    /// </summary>
    /// <value></value>
    public TimeSpan CurrentTime
    {
        get => (TimeSpan)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    /// <summary> 動画のファイルパス </summary>
    public string FilePath
    {
        get => (string)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    /// <summary> MediaElementで取得した動画の再生時間 </summary>
    public TimeSpan MaxTime
    {
        get => (TimeSpan)GetValue(MaxTimeProperty);
        set => SetValue(MaxTimeProperty, value);
    }

    public MovieArea()
    {
        InitializeComponent();

        // Storyboard取得
        _movieStory = (Storyboard)FindResource("MovieStory");
    }

    /// <summary>
    /// 動画をロードする(Begin)
    /// </summary>
    public void LoadMovie()
    {
        _movieStory.Begin();
        _movieStory.Pause();

        MyClippingBoard.Load();
    }

    /// <summary>
    /// 動画のソースとなるファイルパスが変更されたときの処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnFilePathChanged(DependencyObject sender,  DependencyPropertyChangedEventArgs e)
    {
        if(sender is not MovieArea movieArea) return;

        // if(e.NewValue is not string filePath) return;
        // System.Diagnostics.Debug.WriteLine($"file path:{filePath}");

        // この関数を抜けてからLoadMovieを実行する（この関数内で実行すると、1つ前のFilePathを参照してしまう）
        movieArea.Dispatcher.BeginInvoke(new Action(movieArea.LoadMovie));
    }

    /// <summary>
    /// MediaElementが動画を開いたとき（Beginしたとき）に行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoviePlayer_MediaOpened(object sender, EventArgs e)
    {
        if(sender is not MediaElement mediaElement) return;

        // 動画再生時間を取得
        MaxTime = mediaElement.NaturalDuration.TimeSpan;
    }

    /// <summary>
    /// 動画再生・停止トグルを「再生」にしたときに行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Toggle_Checked(object sender, RoutedEventArgs e)
    {
        // 動画がCompletedした？（Completedすると、SeekしてからでないとResumeできない）
        if(_movieCompleted)
        {
            // スライドバーの位置がMaxから10msec未満のとき、動画を0から再生する
            if(TimeSlider.Maximum - TimeSlider.Value < ResetEnableMsec)
            {
                _movieStory.Seek(TimeSpan.Zero);
            }
            // スライドバーの位置がMaxから10msec以上離れているとき、その場所から動画を再生する
            else
            {
                _movieStory.Seek(TimeSpan.FromMilliseconds(TimeSlider.Value));
            }

            // フラグリセット
            _movieCompleted = false;
        }

        // 動画再生
        _movieStory.Resume();
    }

    /// <summary>
    /// 動画再生・停止トグルを「停止」にしたときに行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Toggle_Unchecked(object sender, RoutedEventArgs e)
    {
        _movieStory.Pause();
    }

    /// <summary>
    /// 動画更新時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timeline_CurrentTimeInvalidated(object sender, EventArgs e)
    {
        // 動画の現在時刻時刻をMediaElementから取得し、CurrentTimeに反映する
        CurrentTime = MoviePlayer.Position;

        // System.Diagnostics.Debug.WriteLine($"position {CurrentTime}");
    }

    /// <summary>
    /// 動画Completed時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timeline_Completed(object sender, EventArgs e)
    {
        // 動画再生・停止トグルを「停止」にする
        PlayToggle.IsChecked = false;

        // Completedしたことを記録する
        _movieCompleted = true;

        // System.Diagnostics.Debug.WriteLine($"completed {MoviePlayer.Position}");
    }

    /// <summary>
    /// スライダードラッグ開始時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeSlider_DragStarted(object sender, DragStartedEventArgs e)
    {
        // 動画のオーディオボリュームを記録する
        _audioVolume = MoviePlayer.Volume;

        // 動画のオーディオを消音する
        MoviePlayer.Volume = 0;

        // 動画が再生中であれば、一時停止にする
        if(PlayToggle.IsChecked == true)
        {
            _movieStory.Pause();
        }

        // ValueChangedイベント時にSeekを有効にする
        _isValueChangedEnabled = true;
    }

    /// <summary>
    /// スライダードラッグ終了時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeSlider_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        // ValueChangedイベント時にSeekを無効にする
        _isValueChangedEnabled = false;

        // 動画が再生中であれば、再生を再開する
        if(PlayToggle.IsChecked == true)
        {
            _movieStory.Resume();
        }

        // 動画のオーディオボリュームを元に戻す
        MoviePlayer.Volume = _audioVolume;
    }

    /// <summary>
    /// スライダーのValue変更時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // （スライダードラッグ中 OR 動画停止中）AND（Value変化量が10msecより大きい）とき
        if((_isValueChangedEnabled || PlayToggle.IsChecked == false) && Math.Abs(e.NewValue - e.OldValue) > ValueChangedEnableMsec)
        {
            // スライダーのValueに合わせてSeekする
            _movieStory.Seek(TimeSpan.FromMilliseconds(Math.Round(e.NewValue)));
            // System.Diagnostics.Debug.WriteLine($"new value:{e.NewValue}, {Math.Round(e.NewValue)}");
        }
    }

    /// <summary>
    /// リロードボタンクリック時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        LoadMovie();
    }
}

