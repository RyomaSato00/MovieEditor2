using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.IndividualUI.ViewModels;

public partial class MovieAreaViewModel : ObservableObject
{
    /// <summary> 動画を0から再スタートするときのSliderのMaximumからみたValueの閾値 </summary>
    private static readonly double ResetEnableMsec = 10.0;

    /// <summary> Va lueChangedイベントが呼ばれたときにSeekを実行するValue変化量の閾値 </summary>
    private static readonly double ValueChangedEnableMsec = 10.0;

    /// <summary> 動画の再生制御を行うStoryboard </summary>
    public Storyboard Story { get; private set; } = new();

    public ClippingBoardViewModel ClippingBoardUI { get; } = new();

    [ObservableProperty] private ItemInfo? _item = null;

    // 動画の現在時刻（テキスト表示用）
    [ObservableProperty] private TimeSpan _currentTime;

    /// <summary> MediaElementで取得した動画の再生時間 </summary>
    [ObservableProperty] private TimeSpan _maxTime;

    /// <summary> スライダーの現在値 </summary>
    [ObservableProperty] private double _timeSliderValue;

    /// <summary> 動画再生中？ </summary>
    [ObservableProperty] private bool _isPlaying = false;

    /// <summary> 動画の回転角 </summary>
    [ObservableProperty] private double _movieAngle = 0;

    /// <summary> 動画の音量 </summary>
    // [ObservableProperty] private double _audioVolume;

    /// <summary> オーディオボリューム量を一時保管するための変数 </summary>
    // private double _audioVolumeStore;

    /// <summary> 動画が最終時刻まで進んだときに立たせるフラグ </summary>
    private bool _movieCompleted = false;

    /// <summary> Sliderドラッグ中？ </summary>
    private bool _isSliderDragging = false;

    /// <summary>
    /// 動画をロードする(Begin)
    /// </summary>
    /// <param name="item"></param>
    public void LoadMovie(ItemInfo item)
    {
        Item = item;
        ClippingBoardUI.UpdateItem(item);
        LoadMovie();
        MovieAngle = 0;
    }

    /// <summary>
    /// 動画をロードする(Begin)
    /// </summary>
    public void LoadMovie()
    {
        // フラグリセット
        _movieCompleted = false;

        if(Story is null)
        {
            System.Diagnostics.Debug.WriteLine("movie story is null");
            return;
        }

        // 動画読み込み＆再生
        Story.Begin();

        // 動画停止中状態ならここでポーズする
        if(false == IsPlaying)
        {
            Story.Pause();
        }
    }

    /// <summary>
    /// Storyboard取得処理（MovieArea Load後、一度のみ行われる）
    /// </summary>
    /// <param name="story"></param>
    [RelayCommand] private void StoryboardLoaded(Storyboard story)
    {
        Story = story;
    }

    /// <summary>
    /// MediaElementが動画を開いたとき（Beginしたとき）に行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void MediaOpened(Duration duration)
    {
        if(duration.HasTimeSpan)
        {
            // 動画再生時間を取得
            MaxTime = duration.TimeSpan;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("has no timespan");
        }
    }

    /// <summary>
    /// 動画再生・停止トグルを「再生」にしたときに行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void Play()
    {
        // 動画がCompletedした？（Completedすると、SeekしてからでないとResumeできない）
        if(_movieCompleted)
        {
            // スライドバーの位置がMaxから10msec未満のとき、動画を0から再生する
            if(MaxTime.TotalMilliseconds - TimeSliderValue < ResetEnableMsec)
            {
                Story.Seek(TimeSpan.Zero);
            }
            // スライドバーの位置がMaxから10msec以上離れているとき、その場所から動画を再生する
            else
            {
                Story.Seek(TimeSpan.FromMilliseconds(TimeSliderValue));
            }

            // フラグリセット
            _movieCompleted = false;
        }

        // 動画再生
        Story.Resume();
    }

    /// <summary>
    /// 動画再生・停止トグルを「停止」にしたときに行う処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void Pause()
    {
        Story.Pause();
    }

    /// <summary>
    /// 動画フレーム更新時処理
    /// </summary>
    /// <param name="position">Media Elementから取得した現在時間</param>
    [RelayCommand] private void FrameUpdated(TimeSpan position)
    {
        // 動画の現在時刻時刻をMediaElementから取得し、CurrentTimeに反映する
        CurrentTime = position;

        // スライダーをドラッグしていないときだけ、このタイミングでスライダーのValueを更新する
        if (_isSliderDragging == false)
        {
            TimeSliderValue = position.TotalMilliseconds;
        }
    }

    /// <summary>
    /// 動画Completed時処理
    /// </summary>
    [RelayCommand] private void Completed()
    {
        // 動画再生・停止を「停止」にする
        IsPlaying = false;

        // Completedしたことを記録する
        _movieCompleted = true;
    }

    /// <summary>
    /// スライダードラッグ開始時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void DragStarted()
    {
        // 動画のオーディオボリュームを記録する
        // _audioVolume = MoviePlayer.Volume;
        // _audioVolumeStore = AudioVolume;

        // 動画のオーディオを消音する
        // MoviePlayer.Volume = 0;
        // AudioVolume = 0;

        // 動画が再生中であれば、一時停止にする
        if(IsPlaying == true)
        {
            Pause();
        }

        // ValueChangedイベント時にSeekを有効にする
        _isSliderDragging = true;
    }

    /// <summary>
    /// スライダードラッグ終了時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void DragCompleted()
    {
        // ValueChangedイベント時にSeekを無効にする
        _isSliderDragging = false;

        // 動画が再生中であれば、再生を再開する
        if(IsPlaying == true)
        {
            Story.Resume();
        }

        // 動画のオーディオボリュームを元に戻す
        // MoviePlayer.Volume = _audioVolume;
        // AudioVolume = _audioVolumeStore;
    }

    /// <summary>
    /// スライダーのValue変更時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void ValueChanged(RoutedPropertyChangedEventArgs<double> e)
    {
        // （スライダードラッグ中 OR 動画停止中）AND（Value変化量が10msecより大きい）とき
        if((_isSliderDragging || IsPlaying == false) && Math.Abs(e.NewValue - CurrentTime.TotalMilliseconds) > ValueChangedEnableMsec)
        {
            // スライダーのValueに合わせてSeekする
            Story.Seek(TimeSpan.FromMilliseconds(Math.Round(e.NewValue)));
            // System.Diagnostics.Debug.WriteLine($"new value:{e.NewValue}, {Math.Round(e.NewValue)}");
        }
    }

    /// <summary>
    /// リロードボタンクリック時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void Reload()
    {
        LoadMovie();
    }

    /// <summary>
    /// 動画の再生・停止を切り替える
    /// </summary>
    [RelayCommand] private void TogglePlay()
    {
        IsPlaying = !IsPlaying;
    }
}
