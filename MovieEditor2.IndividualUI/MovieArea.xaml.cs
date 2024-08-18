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

    /// <summary> スライダードラッグ中にスライダー位置と動画再生位置を同期させるためのDispatcherTimer </summary>
    private readonly DispatcherTimer _dispatcherTimer = new();
    private readonly Storyboard _timelineStory;

    private double _audioVolume;

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


    public MovieArea()
    {

        InitializeComponent();
        _timelineStory = (Storyboard)FindResource("TimelineStory");
        _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
        _dispatcherTimer.Tick += (_, _) => _timelineStory.Seek(TimeSpan.FromMilliseconds(TimeSlider.Value));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoviePlayer_Loaded(object sender, RoutedEventArgs e)
    {
        // サムネイル読み込み
        _timelineStory.Begin();
        _timelineStory.Pause();
    }


    private void Toggle_Checked(object sender, RoutedEventArgs e)
    {
        _timelineStory.Resume();
    }


    private void Toggle_Unchecked(object sender, RoutedEventArgs e)
    {
        _timelineStory.Pause();
    }


    private void Timeline_CurrentTimeInvalidated(object sender, EventArgs e)
    {
        CurrentTime = MoviePlayer.Position;

        System.Diagnostics.Debug.WriteLine($"position {CurrentTime}");
    }


    private void TimeSlider_DragStarted(object sender, DragStartedEventArgs e)
    {
        _audioVolume = MoviePlayer.Volume;

        MoviePlayer.Volume = 0;

        if(PlayToggle.IsChecked == true)
        {
            // 動画を一時停止状態にする
            _timelineStory.Pause();
        }

        // SliderのValueプロパティのBindingを一時的に解除する
        // BindingOperations.ClearBinding(TimeSlider, Slider.ValueProperty);

        _dispatcherTimer.Start();
    }


    private void TimeSlider_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        _dispatcherTimer.Stop();

        // SliderのValueプロパティのBindingを復帰させる
        // var binding = (Binding)FindResource("Time2SliderBinding");
        // TimeSlider.SetBinding(Slider.ValueProperty, binding);

        if(PlayToggle.IsChecked == true)
        {
            _timelineStory.Resume();
        }

        MoviePlayer.Volume = _audioVolume;
    }
}

