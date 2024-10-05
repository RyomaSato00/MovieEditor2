using System.Windows;

using Microsoft.Xaml.Behaviors;

namespace MovieEditor2.IndividualUI;

/// <summary>
/// クリッピング範囲の表示を更新するTriggerAction
/// </summary>
internal class UpdateEdgesAction : TriggerAction<ClippingBoard>
{
    protected override void Invoke(object parameter)
    {
        if (parameter is DependencyPropertyChangedEventArgs e
        && e.NewValue is UpdateEdgesRequest request)
        {
            AssociatedObject.UpdateArea(request.AreaRect);
        }
    }
}

/// <summary>
/// Offsetを更新するTriggerAction
/// </summary>
internal class RefreshOffsetAction : TriggerAction<ClippingBoard>
{
    protected override void Invoke(object parameter)
    {
        if (parameter is DependencyPropertyChangedEventArgs e
        && e.NewValue is RefreshOffsetRequest)
        {
            AssociatedObject.RefreshOffset();
        }
    }
}

/// <summary>
/// クリッピング範囲の表示の更新を要求するためのRequest
/// </summary>
public class UpdateEdgesRequest
{
    /// <summary> クリッピング範囲 </summary>
    public Rect AreaRect { get; init; } = Rect.Empty;
}

/// <summary>
/// Offsetの更新を要求するためのRequest
/// </summary>
public class RefreshOffsetRequest
{

}

