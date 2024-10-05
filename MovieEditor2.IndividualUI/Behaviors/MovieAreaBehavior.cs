using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

using Microsoft.Xaml.Behaviors;

namespace MovieEditor2.IndividualUI;

/// <summary>
/// MovieArea → MovieAreaViewModelの方向の制御を実現するためのBehavior
/// </summary>
internal class MovieAreaBehavior : Behavior<MovieArea>
{
    public static readonly DependencyProperty StoryboardLoadedCommandProperty =
    DependencyProperty.Register(nameof(StoryboardLoadedCommand), typeof(ICommand), typeof(MovieAreaBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty CurrentTimeInvalidatedCommandProperty =
    DependencyProperty.Register(nameof(CurrentTimeInvalidatedCommand), typeof(ICommand), typeof(MovieAreaBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty CompletedCommandProperty =
    DependencyProperty.Register(nameof(CompletedCommand), typeof(ICommand), typeof(MovieAreaBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty MediaOpenedCommandProperty =
    DependencyProperty.Register(nameof(MediaOpenedCommand), typeof(ICommand), typeof(MovieAreaBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty MediaElementSizeChangedCommandProperty =
    DependencyProperty.Register(nameof(MediaElementSizeChangedCommand), typeof(ICommand), typeof(MovieAreaBehavior), new PropertyMetadata(null));


    public ICommand StoryboardLoadedCommand
    {
        get => (ICommand)GetValue(StoryboardLoadedCommandProperty);
        set => SetValue(StoryboardLoadedCommandProperty, value);
    }

    public ICommand CurrentTimeInvalidatedCommand
    {
        get => (ICommand)GetValue(CurrentTimeInvalidatedCommandProperty);
        set => SetValue(CurrentTimeInvalidatedCommandProperty, value);
    }

    public ICommand CompletedCommand
    {
        get => (ICommand)GetValue(CompletedCommandProperty);
        set => SetValue(CompletedCommandProperty, value);
    }

    public ICommand MediaOpenedCommand
    {
        get => (ICommand)GetValue(MediaOpenedCommandProperty);
        set => SetValue(MediaOpenedCommandProperty, value);
    }

    public ICommand MediaElementSizeChangedCommand
    {
        get => (ICommand)GetValue(MediaElementSizeChangedCommandProperty);
        set => SetValue(MediaElementSizeChangedCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.OnStoryboardLoaded += StoryboardLoaded;
        AssociatedObject.OnMediaOpened += Mediaopened;
        AssociatedObject.OnCurrentTimeInvalidated += CurrenttimeInvalidated;
        AssociatedObject.OnCompleted += Completed;
        AssociatedObject.OnMediaElementSizeChanged += MediaElementSizeChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.OnStoryboardLoaded -= StoryboardLoaded;
        AssociatedObject.OnMediaOpened -= Mediaopened;
        AssociatedObject.OnCurrentTimeInvalidated -= CurrenttimeInvalidated;
        AssociatedObject.OnCompleted -= Completed;
        AssociatedObject.OnMediaElementSizeChanged -= MediaElementSizeChanged;
    }

    private void StoryboardLoaded(Storyboard storyboard) => StoryboardLoadedCommand?.Execute(storyboard);

    private void Mediaopened(Duration duration) => MediaOpenedCommand?.Execute(duration);

    private void CurrenttimeInvalidated(TimeSpan position) => CurrentTimeInvalidatedCommand?.Execute(position);

    private void Completed() => CompletedCommand?.Execute(null);

    private void MediaElementSizeChanged(Size size) => MediaElementSizeChangedCommand?.Execute(size);

}
