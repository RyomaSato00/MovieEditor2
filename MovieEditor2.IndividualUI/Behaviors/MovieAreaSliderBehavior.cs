using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace MovieEditor2.IndividualUI;

internal class MovieAreaSliderBehavior : Behavior<Slider>
{
    public static readonly DependencyProperty ValueChangedCommandProperty =
    DependencyProperty.Register(nameof(ValueChangedCommand), typeof(ICommand), typeof(MovieAreaSliderBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty DragStartedCommandProperty =
    DependencyProperty.Register(nameof(DragStartedCommand), typeof(ICommand), typeof(MovieAreaSliderBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty DragCompletedCommandProperty =
    DependencyProperty.Register(nameof(DragCompletedCommand), typeof(ICommand), typeof(MovieAreaSliderBehavior), new PropertyMetadata(null));


    public ICommand ValueChangedCommand
    {
        get => (ICommand)GetValue(ValueChangedCommandProperty);
        set => SetValue(ValueChangedCommandProperty, value);
    }

    public ICommand DragStartedCommand
    {
        get => (ICommand)GetValue(DragStartedCommandProperty);
        set => SetValue(DragStartedCommandProperty, value);
    }

    public ICommand DragCompletedCommand
    {
        get => (ICommand)GetValue(DragCompletedCommandProperty);
        set => SetValue(DragCompletedCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.ValueChanged += ValueChanged;

        // この時点ではThumbがまだ読み込まれていないため、Loadedの中でThumbのイベントを定義する
        AssociatedObject.Loaded += Slider_Loaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.ValueChanged -= ValueChanged;

        var track = (Track)AssociatedObject.Template.FindName("PART_Track", AssociatedObject);
        track.Thumb.DragStarted -= DragStarted;
        track.Thumb.DragCompleted -= DragCompleted;
    }

    private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ValueChangedCommand?.Execute(e);

    private void DragStarted(object sender, RoutedEventArgs e) => DragStartedCommand?.Execute(null);

    private void DragCompleted(object sender, RoutedEventArgs e) => DragCompletedCommand?.Execute(null);

    private void Slider_Loaded(object sender, RoutedEventArgs e)
    {
        var track = (Track)AssociatedObject.Template.FindName("PART_Track", AssociatedObject);
        track.Thumb.DragStarted += DragStarted;
        track.Thumb.DragCompleted += DragCompleted;
    }
}
