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

    /// <summary> クリッピング範囲（UserControl基準）の最小サイズ </summary>
    public static readonly Size MinimumSize = new(80, 80);

    public static readonly DependencyProperty MousePositionProperty
    = DependencyProperty.Register(nameof(MousePosition), typeof(Point), typeof(ClippingBoard), new PropertyMetadata(Zero));

    public static readonly DependencyProperty ClipRectProperty
    = DependencyProperty.Register(nameof(ClipRect), typeof(Rect), typeof(ClippingBoard), new PropertyMetadata(Rect.Empty, OnClipRectChanged));

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

    /// <summary>
    /// クリッピング範囲をロードしてUIに反映する
    /// </summary>
    public void Load()
    {
        Offset = Target.TransformToVisual(this).Transform(Zero);

        // クリッピング範囲情報が指定なしのとき
        if(ClipRect == Rect.Empty)
        {
            // クリッピング範囲は動画サイズと同じにする
            ClipRect = new Rect(Zero, new Size(OriginWidth, OriginHeight));
            UpdateArea(new Rect(Offset, MediaElementSize));
        }
        // クリッピング範囲情報が指定済みのとき
        else
        {
            // クリッピング範囲情報のスケールをUserControl基準のスケールに変換する
            var areaRect = new Rect
            {
                X = Offset.X + ClipRect.X * MediaElementSize.Width / OriginWidth,
                Y = Offset.Y + ClipRect.Y * MediaElementSize.Height / OriginHeight,
                Width = ClipRect.Width * MediaElementSize.Width / OriginWidth,
                Height = ClipRect.Height * MediaElementSize.Height / OriginHeight
            };
            UpdateArea(areaRect);
        }
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
        if(destinationX > EdgeCanvas.ActualWidth) destinationX = EdgeCanvas.ActualWidth;
        if(destinationY > EdgeCanvas.ActualHeight) destinationY = EdgeCanvas.ActualHeight;

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

    /// <summary>
    /// ClipRect変更時処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnClipRectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not ClippingBoard clippingBoard) return;

        // クリッピング範囲のUIを更新
        clippingBoard.Dispatcher.BeginInvoke(new Action(clippingBoard.Load));
    }
}

