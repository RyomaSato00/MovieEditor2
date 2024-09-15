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

namespace MovieEditor2.IndividualUI;

/// <summary>
/// Interaction logic for ClippingBoard.xaml
/// </summary>
public partial class ClippingBoard : UserControl
{
    public static readonly Point Zero = new(0, 0);

    public static readonly DependencyProperty MousePositionProperty
    = DependencyProperty.Register(nameof(MousePosition), typeof(Point), typeof(ClippingBoard), new PropertyMetadata(Zero));

    public static readonly DependencyProperty ClipRectProperty
    = DependencyProperty.Register(nameof(ClipRect), typeof(Rect), typeof(ClippingBoard), new PropertyMetadata(Rect.Empty));

    public static readonly DependencyProperty OriginWidthProperty
    = DependencyProperty.Register(nameof(OriginWidth), typeof(int), typeof(ClippingBoard), new PropertyMetadata(0));

    public static readonly DependencyProperty OriginHeightProperty
    = DependencyProperty.Register(nameof(OriginHeight), typeof(int), typeof(ClippingBoard), new PropertyMetadata(0));

    public static readonly DependencyProperty MediaElementSizeProperty
    = DependencyProperty.Register(nameof(MediaElementSize), typeof(Size), typeof(ClippingBoard), new PropertyMetadata(Size.Empty));

    public static readonly DependencyProperty TargetProperty
    = DependencyProperty.Register(nameof(Target), typeof(Visual), typeof(ClippingBoard), new PropertyMetadata(null));

    public ClippingBoard()
    {
        InitializeComponent();
    }

    /// <summary> 今ドラッグしているエッジまたはnull </summary>
    private FrameworkElement? _currentDragged = null;

    /// <summary> ドラッグしたエッジを基準にしたマウスクリック座標 </summary>
    private Point _relativePosition = Zero;

    /// <summary> このUserControlを基準にしたMediaElementの座標 </summary>
    public Point Offset { get; private set; } = Zero;

    /// <summary> 動画のサイズに合わせて補正したマウスの現在座標 </summary>
    public Point MousePosition
    {
        get => (Point)GetValue(MousePositionProperty);
        set => SetValue(MousePositionProperty, value);
    }

    /// <summary> 動画のクリッピング範囲 </summary>
    public Rect ClipRect
    {
        get => (Rect)GetValue(ClipRectProperty);
        set => SetValue(ClipRectProperty, value);
    }

    /// <summary> 動画の実幅 </summary>
    public int OriginWidth
    {
        get => (int)GetValue(OriginWidthProperty);
        set => SetValue(OriginWidthProperty, value);
    }

    /// <summary> 動画の実高さ </summary>
    public int OriginHeight
    {
        get => (int)GetValue(OriginHeightProperty);
        set => SetValue(OriginHeightProperty, value);
    }

    /// <summary> MediaElementのサイズ </summary>
    public Size MediaElementSize
    {
        get => (Size)GetValue(MediaElementSizeProperty);
        set => SetValue(MediaElementSizeProperty, value);
    }

    /// <summary> MediaElementオブジェクト </summary>
    public Visual Target
    {
        get => (Visual)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public void Load()
    {
        Offset = Target.TransformToVisual(this).Transform(Zero);

        UpdateArea(new Rect(Offset, MediaElementSize));

        ClipRect = new Rect(Zero, new Size(OriginWidth, OriginHeight));
    }

    private void ClippingBoard_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        BaseGeometry.Rect = new Rect(0, 0, ((Canvas)sender).ActualWidth, ((Canvas)sender).ActualHeight);

        Dispatcher.BeginInvoke(new Action(Load));
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

        // マウス座標を動画のサイズとMediaElementの位置に合わせて補正
        MousePosition = new Point
        {
            X = (current.X - Offset.X) * OriginWidth / MediaElementSize.Width,
            Y = (current.Y - Offset.Y) * OriginHeight / MediaElementSize.Height
        };

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

        // クリッピング範囲を計算する
        var areaRect = CalculateClip(destinationX, destinationY, _currentDragged.Name);

        // クリッピング範囲UI表示更新
        UpdateArea(areaRect);

        // 動画の実サイズに合わせて補正
        var position = new Point
        {
            X = (areaRect.X - Offset.X) * OriginWidth / MediaElementSize.Width,
            Y = (areaRect.Y - Offset.Y) * OriginHeight / MediaElementSize.Height
        };

        var size = new Size
        {
            Width = areaRect.Width * OriginWidth / MediaElementSize.Width,
            Height = areaRect.Height * OriginHeight / MediaElementSize.Height
        };

        // 動画のクリッピング実範囲を取得
        ClipRect = new Rect(position, size);
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
        Point location;
        Size areaSize;
        double topLeftX;
        double topLeftY;

        switch(edgeName)
        {
        case "TopLeftEdge":
            location = new Point(destinationX, destinationY);
            areaSize = new Size
            {
                Width = Canvas.GetLeft(TopRightEdge) - destinationX,
                Height = Canvas.GetTop(BottomLeftEdge) - destinationY
            };

            break;

        case "TopRightEdge":
            topLeftX = Canvas.GetLeft(TopLeftEdge);
            location = new Point(topLeftX, destinationY);
            areaSize = new Size
            {
                Width = destinationX - topLeftX,
                Height = Canvas.GetTop(BottomLeftEdge) - destinationY
            };

            break;

        case "BottomRightEdge":
            topLeftX = Canvas.GetLeft(TopLeftEdge);
            topLeftY = Canvas.GetTop(TopLeftEdge);
            location = new Point(topLeftX, topLeftY);
            areaSize = new Size
            {
                Width = destinationX - topLeftX,
                Height = destinationY - topLeftY
            };

            break;

        case "BottomLeftEdge":
            topLeftY = Canvas.GetTop(TopLeftEdge);
            location = new Point(destinationX, topLeftY);
            areaSize = new Size
            {
                Width = Canvas.GetLeft(TopRightEdge) - destinationX,
                Height = destinationY - topLeftY
            };

            break;

        default:
            break;
        }

        return new Rect(location, areaSize);
    }


    // private static void OnMediaElementSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    // {
    //     System.Diagnostics.Debug.WriteLine("media element size changed");

    //     if (sender is not ClippingBoard clippingBoard) return;

    //     if(e.NewValue is not Size size) return;

    //     // System.Diagnostics.Debug.WriteLine($"origin size:{size}");

    //     clippingBoard.Offset = clippingBoard.Target.TransformToVisual(clippingBoard).Transform(Zero);

    //     clippingBoard.UpdateArea(new Rect(clippingBoard.Offset, size));

    //     clippingBoard.ClipRect = new Rect(Zero, new Size(clippingBoard.OriginWidth, clippingBoard.OriginHeight));
    // }
}

