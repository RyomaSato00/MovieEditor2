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

    // 動画の現在時刻
    [ObservableProperty] private TimeSpan _currentTime;

    /// <summary> MediaElementで取得した動画の再生時間 </summary>
    [ObservableProperty] private TimeSpan _maxTime;

    /// <summary> スライダーの現在値 </summary>
    [ObservableProperty] private double _timeSliderValue;

    /// <summary> 動画再生中？ </summary>
    [ObservableProperty] private bool _isPlaying = false;

    /// <summary> 動画の音量 </summary>
    [ObservableProperty] private double _audioVolume;

    /// <summary> オーディオボリューム量を一時保管するための変数 </summary>
    private double _audioVolumeStore;

    /// <summary> 動画が最終時刻まで進んだときに立たせるフラグ </summary>
    private bool _movieCompleted = false;

    /// <summary> ValueChangedイベント時にSeekの実行可否を制御するためのフラグ </summary>
    private bool _isValueChangedEnabled = false;

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
        System.Diagnostics.Debug.WriteLine("load movie");
        Story.Begin();

        if(false == IsPlaying)
        {
            Story.Pause();
        }

        // System.Diagnostics.Debug.WriteLine($"duration:{Story.Duration}");

        // MyClippingBoard.Load();
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
        _audioVolumeStore = AudioVolume;

        // 動画のオーディオを消音する
        // MoviePlayer.Volume = 0;
        AudioVolume = 0;

        // 動画が再生中であれば、一時停止にする
        if(IsPlaying == true)
        {
            Pause();
        }

        // ValueChangedイベント時にSeekを有効にする
        _isValueChangedEnabled = true;
    }

    /// <summary>
    /// スライダードラッグ終了時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void DragCompleted()
    {
        // ValueChangedイベント時にSeekを無効にする
        _isValueChangedEnabled = false;

        // 動画が再生中であれば、再生を再開する
        if(IsPlaying == true)
        {
            Story.Resume();
        }

        // 動画のオーディオボリュームを元に戻す
        // MoviePlayer.Volume = _audioVolume;
        AudioVolume = _audioVolumeStore;
    }

    /// <summary>
    /// スライダーのValue変更時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [RelayCommand] private void ValueChanged(RoutedPropertyChangedEventArgs<double> e)
    {
        // （スライダードラッグ中 OR 動画停止中）AND（Value変化量が10msecより大きい）とき
        if((_isValueChangedEnabled || IsPlaying == false) && Math.Abs(e.NewValue - e.OldValue) > ValueChangedEnableMsec)
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
}
