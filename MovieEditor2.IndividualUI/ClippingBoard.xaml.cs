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

using Microsoft.Xaml.Behaviors;

namespace MovieEditor2.IndividualUI;

/// <summary>
/// Interaction logic for ClippingBoard.xaml
/// </summary>
public partial class ClippingBoard : UserControl
{
    public static readonly Point Zero = new(0, 0);

    /// <summary> クリッピング範囲（UserControl基準）の最小サイズ </summary>
    public static readonly Size MinimumSize = new(80, 80);

    /// <summary> Offset変更イベント </summary>
    public event Action<Point>? OnOffsetRefreshed = null;

    /// <summary> マウス移動イベント </summary>
    public event Action<Point>? OnMouseMoved = null;

    /// <summary> ClipRect変更イベント </summary>
    public event Action<Rect>? OnClipRectChanged = null;


    public static readonly DependencyProperty TargetProperty
    = DependencyProperty.Register(nameof(Target), typeof(Visual), typeof(ClippingBoard), new PropertyMetadata(null));

    /// <summary> MediaElementオブジェクト </summary>
    public Visual Target
    {
        get => (Visual)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary> このUserControlを基準にしたMediaElementの座標 </summary>
    private Point _offset = Zero;

    /// <summary> 今ドラッグしているエッジまたはnull </summary>
    private FrameworkElement? _currentDragged = null;

    /// <summary> ドラッグしたエッジを基準にしたマウスクリック座標 </summary>
    private Point _relativePosition = Zero;

    public ClippingBoard()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Offset再取得処理
    /// </summary>
    public void RefreshOffset()
    {
        _offset = Target.TransformToVisual(this).Transform(Zero);

        // Offset再取得通知
        OnOffsetRefreshed?.Invoke(_offset);
    }

    /// <summary>
    /// キャンバスのサイズ変更時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        BaseGeometry.Rect = new Rect(0, 0, ((Canvas)sender).ActualWidth, ((Canvas)sender).ActualHeight);
    }

    /// <summary>
    /// エッジ、キャンバス等のUI表示を更新する
    /// </summary>
    /// <param name="areaRect">クリッピング範囲（UserControl基準）</param>
    public void UpdateArea(Rect areaRect)
    {
        ClipGeometry.Rect = areaRect;

        Canvas.SetLeft(TopLeftEdge, areaRect.TopLeft.X);
        Canvas.SetTop(TopLeftEdge, areaRect.TopLeft.Y);

        Canvas.SetLeft(TopRightEdge, areaRect.TopRight.X);
        Canvas.SetTop(TopRightEdge, areaRect.TopRight.Y);

        Canvas.SetLeft(BottomRightEdge, areaRect.BottomRight.X);
        Canvas.SetTop(BottomRightEdge, areaRect.BottomRight.Y);

        Canvas.SetLeft(BottomLeftEdge, areaRect.BottomLeft.X);
        Canvas.SetTop(BottomLeftEdge, areaRect.BottomLeft.Y);
    }

    /// <summary>
    /// エッジ上でマウスダウンしたときの処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Edge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement edge) return;

        // ドラッグするエッジを記憶
        _currentDragged = edge;

        // エッジ基準のマウス座標を取得
        _relativePosition = e.GetPosition(edge);
    }

    /// <summary>
    /// エッジ上でマウスアップしたときの処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Edge_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // ドラッグ中オブジェクトを開放
        _currentDragged = null;
    }

    /// <summary>
    /// マウス移動時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Edge_MouseMove(object sender, MouseEventArgs e)
    {
        // UserControl基準のマウス座標を取得
        var current = e.GetPosition(this);

        // マウス座標変更通知
        OnMouseMoved?.Invoke(current);

        // エッジをドラッグ中でなければここで終了
        if(_currentDragged is null) return;

        // マウスを左クリックしていないなら終了
        if(e.LeftButton == MouseButtonState.Released) return;

        // エッジの移動先を取得
        var destinationX = current.X - _relativePosition.X;
        var destinationY = current.Y - _relativePosition.Y;

        // エッジの移動先がCanvasの範囲外になる場合は境界に移動先を指定する
        if(destinationX < 0) destinationX = 0;
        if(destinationY < 0) destinationY = 0;
        if(destinationX > EdgeCanvas.ActualWidth) destinationX = EdgeCanvas.ActualWidth;
        if(destinationY > EdgeCanvas.ActualHeight) destinationY = EdgeCanvas.ActualHeight;

        // クリッピング範囲を計算する
        var areaRect = CalculateClip(destinationX, destinationY, _currentDragged.Name);

        // クリッピング範囲UI表示更新
        UpdateArea(areaRect);

        // クリッピング範囲変更通知
        OnClipRectChanged?.Invoke(areaRect);
    }

    /// <summary>
    /// クリッピング範囲（UserControl基準）を計算する
    /// </summary>
    /// <remarks>
    /// 動画の実範囲に合わせたクリッピング範囲は別途計算が必要
    /// </remarks>
    /// <param name="destinationX">エッジの移動先X座標</param>
    /// <param name="destinationY">エッジの移動先Y座標</param>
    /// <param name="edgeName">今ドラッグ中のエッジの名前</param>
    /// <returns></returns>
    private Rect CalculateClip(double destinationX, double destinationY, string edgeName)
    {
        double topLeftX;
        double topLeftY;
        double bottomRightX;
        double bottomRightY;
        Point topLeft;
        Point bottomRight;

        switch(edgeName)
        {
        case "TopLeftEdge":

            bottomRightX = Canvas.GetLeft(BottomRightEdge);
            bottomRightY = Canvas.GetTop(BottomRightEdge);

            // クリッピング範囲が最小範囲を下回らないように設定
            if(bottomRightX - destinationX < MinimumSize.Width)
            {
                destinationX = bottomRightX - MinimumSize.Width;
            }

            if(bottomRightY - destinationY < MinimumSize.Height)
            {
                destinationY = bottomRightY - MinimumSize.Height;
            }

            topLeft = new Point(destinationX, destinationY);
            bottomRight = new Point(bottomRightX, bottomRightY);
            break;

        case "TopRightEdge":

            topLeftX = Canvas.GetLeft(TopLeftEdge);
            bottomRightY = Canvas.GetTop(BottomRightEdge);

            // クリッピング範囲が最小範囲を下回らないように設定
            if(destinationX - topLeftX < MinimumSize.Width)
            {
                destinationX = topLeftX + MinimumSize.Width;
            }

            if(bottomRightY - destinationY < MinimumSize.Height)
            {
                destinationY = bottomRightY - MinimumSize.Height;
            }

            topLeft = new Point(topLeftX, destinationY);
            bottomRight = new Point(destinationX, bottomRightY);
            break;

        case "BottomRightEdge":

            topLeftX = Canvas.GetLeft(TopLeftEdge);
            topLeftY = Canvas.GetTop(TopLeftEdge);

            // クリッピング範囲が最小範囲を下回らないように設定
            if(destinationX - topLeftX < MinimumSize.Width)
            {
                destinationX = topLeftX + MinimumSize.Width;
            }

            if(destinationY - topLeftY < MinimumSize.Height)
            {
                destinationY = topLeftY + MinimumSize.Height;
            }

            topLeft = new Point(topLeftX, topLeftY);
            bottomRight = new Point(destinationX, destinationY);
            break;

        case "BottomLeftEdge":

            topLeftY = Canvas.GetTop(TopLeftEdge);
            bottomRightX = Canvas.GetLeft(BottomRightEdge);

            // クリッピング範囲が最小範囲を下回らないように設定
            if(bottomRightX - destinationX < MinimumSize.Width)
            {
                destinationX = bottomRightX - MinimumSize.Width;
            }

            if(destinationY - topLeftY < MinimumSize.Height)
            {
                destinationY = topLeftY + MinimumSize.Height;
            }

            topLeft = new Point(destinationX, topLeftY);
            bottomRight = new Point(bottomRightX, destinationY);
            break;

        default:
            break;
        }

        return new Rect(topLeft, bottomRight);
    }
}

