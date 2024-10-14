using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.IndividualUI.ViewModels;

public partial class ClippingBoardViewModel : ObservableObject
{
    public static readonly Point Zero = new(0, 0);

    /// <summary> 動画情報 </summary>
    private ItemInfo? _item = null;

    /// <summary> UserControlを基準にしたMediaElementの座標 </summary>
    private Point _offset = Zero;

    /// <summary> MediaElementのサイズ </summary>
    private Size _mediaElementSize = Size.Empty;

    // クリッピング有効？
    [ObservableProperty] private bool _isClippingEnabled = false;

    // マウスの座標（動画実サイズスケール）
    [ObservableProperty] private Point _mousePoint = Zero;

    // クリッピング範囲表示更新要求
    [ObservableProperty] private UpdateEdgesRequest? _updateEdgesRequest = null;

    // Offset更新要求
    [ObservableProperty] private RefreshOffsetRequest? _refreshOffsetRequest = null;

    /// <summary>
    /// IsClippingEnabled変更時処理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    partial void OnIsClippingEnabledChanged(bool value)
    {
        // クリッピング有効時
        if(value)
        {
            // クリッピング範囲読み込みおよび表示
            LoadClipping();
        }
        // クリッピング無効時
        else
        {
            // クリッピング範囲が指定されているときは未指定にする
            if (_item is not null && _item.Clipping != Rect.Empty)
            {
                _item.Clipping = Rect.Empty;
            }
        }
    }

    /// <summary>
    /// 再生動画変更処理
    /// </summary>
    /// <param name="movie">新しい動画情報</param>
    public void UpdateItem(ItemInfo movie)
    {
        // 動画情報を更新
        _item = movie;

        // クリッピング範囲が指定されていないとき
        if (_item.Clipping == Rect.Empty)
        {
            // クリッピング範囲を表示しない
            IsClippingEnabled = false;
        }
        // クリッピング範囲が指定されているとき
        else
        {
            // クリッピング範囲を表示する
            IsClippingEnabled = true;
        }
    }

    /// <summary>
    /// クリッピング範囲読み込み処理
    /// </summary>
    private void LoadClipping()
    {
        if(_item is null) return;

        if (IsClippingEnabled == false) return;

        // offset読み込み要求
        RefreshOffsetRequest = new RefreshOffsetRequest();

        // クリッピング範囲が指定なしのとき
        if (_item.Clipping == Rect.Empty)
        {
            // クリッピング範囲は動画サイズと同じにする
            _item.Clipping = new Rect(Zero, new Size(_item.OriginalInfo.Width, _item.OriginalInfo.Height));

            // エッジの位置を更新
            UpdateEdges(new Rect(_offset, _mediaElementSize));
        }
        // クリッピング範囲が指定済みのとき
        else
        {
            // クリッピング範囲のスケールを動画サイズ基準からUserControl基準のスケールに変換する
            var rectBasedOnUserControl = ToUserControlScale(_item.Clipping);

            // エッジの位置を更新
            UpdateEdges(rectBasedOnUserControl);
        }
    }

    /// <summary>
    /// クリッピング範囲リセット処理
    /// </summary>
    [RelayCommand] private void ResetClipping()
    {
        if (_item is null) return;

        // offset読み込み
        RefreshOffsetRequest = new RefreshOffsetRequest();

        // クリッピング範囲は動画サイズと同じにする
        _item.Clipping = new Rect(Zero, new Size(_item.OriginalInfo.Width, _item.OriginalInfo.Height));

        // エッジの位置を更新
        UpdateEdges(new Rect(_offset, _mediaElementSize));
    }

    /// <summary>
    /// クリッピング範囲表示更新処理
    /// </summary>
    /// <param name="edgesRect">クリッピング範囲（UserControl基準）</param>
    private void UpdateEdges(Rect edgesRect)
    {
        // Viewにエッジ更新Requestを送る
        UpdateEdgesRequest = new UpdateEdgesRequest { AreaRect = edgesRect };
    }

    /// <summary>
    /// Offset更新時処理
    /// </summary>
    /// <param name="offset"></param>
    [RelayCommand] private void RefreshOffset(Point offset)
    {
        _offset = offset;
    }

    /// <summary>
    /// MediaElementサイズ変更時処理
    /// </summary>
    /// <param name="size"></param>
    [RelayCommand] private void RefreshMediaElementSize(Size size)
    {
        _mediaElementSize = size;

        // クリッピング範囲を再ロード
        LoadClipping();
    }

    /// <summary>
    /// マウス座標変更時処理
    /// </summary>
    /// <param name="point"></param>
    [RelayCommand] private void MouseMoved(Point point)
    {
        // マウス座標の表示を更新
        MousePoint = ToOriginalScale(point);
    }

    /// <summary>
    /// クリッピング範囲変更時処理
    /// </summary>
    /// <param name="rect"></param>
    [RelayCommand] private void ClipRectChanged(Rect rect)
    {
        if (_item is null) return;

        // クリッピング範囲を動画実サイズ寸法に変換
        _item.Clipping = ToOriginalScale(rect);
    }

    /// <summary>
    /// UserControlスケールへの寸法変換処理
    /// </summary>
    /// <param name="originalScale"></param>
    /// <returns></returns>
    private Rect ToUserControlScale(Rect originalScale)
    {
        if (_item is null || _item.OriginalInfo.Width == 0 || _item.OriginalInfo.Height == 0)
        {
            return Rect.Empty;
        }
        else
        {
            return new Rect
            {
                X = _offset.X + originalScale.X * _mediaElementSize.Width / _item.OriginalInfo.Width,
                Y = _offset.Y + originalScale.Y * _mediaElementSize.Height / _item.OriginalInfo.Height,
                Width = originalScale.Width * _mediaElementSize.Width / _item.OriginalInfo.Width,
                Height = originalScale.Height * _mediaElementSize.Height / _item.OriginalInfo.Height
            };
        }

    }

    /// <summary>
    /// 動画実サイズスケールへの寸法変換処理
    /// </summary>
    /// <param name="userControlScale"></param>
    /// <returns></returns>
    private Point ToOriginalScale(Point userControlScale)
    {
        if(_item is null || _mediaElementSize.Width == 0 || _mediaElementSize.Height == 0)
        {
            return Zero;
        }
        else
        {
            return new Point
            {
                X = (userControlScale.X - _offset.X) * _item.OriginalInfo.Width / _mediaElementSize.Width,
                Y = (userControlScale.Y - _offset.Y) * _item.OriginalInfo.Height / _mediaElementSize.Height
            };
        }
    }

    /// <summary>
    /// 動画実サイズスケールへの寸法変換処理
    /// </summary>
    /// <param name="userControlScale"></param>
    /// <returns></returns>
    private Rect ToOriginalScale(Rect userControlScale)
    {
        if(_item is null || _mediaElementSize.Width == 0 || _mediaElementSize.Height == 0)
        {
            return Rect.Empty;
        }
        else
        {
            return new Rect
            {
                X = (userControlScale.X - _offset.X) * _item.OriginalInfo.Width / _mediaElementSize.Width,
                Y = (userControlScale.Y - _offset.Y) * _item.OriginalInfo.Height / _mediaElementSize.Height,
                Width = userControlScale.Width * _item.OriginalInfo.Width / _mediaElementSize.Width,
                Height = userControlScale.Height * _item.OriginalInfo.Height / _mediaElementSize.Height
            };
        }
    }




}
