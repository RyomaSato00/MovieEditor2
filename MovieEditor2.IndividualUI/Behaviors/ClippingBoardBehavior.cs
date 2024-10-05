using System.Windows;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

using MovieEditor2.IndividualUI;

namespace MovieEditor2.IndividualUI;

/// <summary>
/// ClippingBoard → ClippingBoardViewModelの方向の制御を実現するためのBehavior
/// </summary>
internal class ClippingBoardBehavior : Behavior<ClippingBoard>
{
    public static readonly DependencyProperty RefreshOffsetCommandProperty =
    DependencyProperty.Register(nameof(RefreshOffsetCommand), typeof(ICommand), typeof(ClippingBoardBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty MouseMovedCommandProperty =
    DependencyProperty.Register(nameof(MouseMovedCommand), typeof(ICommand), typeof(ClippingBoardBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty ClipRectChangedCommandProperty =
    DependencyProperty.Register(nameof(ClipRectChangedCommand), typeof(ICommand), typeof(ClippingBoardBehavior), new PropertyMetadata(null));

    public ICommand RefreshOffsetCommand
    {
        get => (ICommand)GetValue(RefreshOffsetCommandProperty);
        set => SetValue(RefreshOffsetCommandProperty, value);
    }

    public ICommand MouseMovedCommand
    {
        get => (ICommand)GetValue(MouseMovedCommandProperty);
        set => SetValue(MouseMovedCommandProperty, value);
    }

    public ICommand ClipRectChangedCommand
    {
        get => (ICommand)GetValue(ClipRectChangedCommandProperty);
        set => SetValue(ClipRectChangedCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.OnOffsetRefreshed += RefreshOffset;
        AssociatedObject.OnMouseMoved += MouseMoved;
        AssociatedObject.OnClipRectChanged += ClippingRectChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.OnOffsetRefreshed -= RefreshOffset;
        AssociatedObject.OnMouseMoved -= MouseMoved;
        AssociatedObject.OnClipRectChanged -= ClippingRectChanged;
    }

    private void RefreshOffset(Point point) => RefreshOffsetCommand?.Execute(point);
    private void MouseMoved(Point point) => MouseMovedCommand?.Execute(point);
    private void ClippingRectChanged(Rect rect) => ClipRectChangedCommand?.Execute(rect);
}
