using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.MovieListUI;

public class FileDropBehavior : Behavior<UserControl>
{
    public static readonly object ParallelLock = new();

    public static readonly DependencyProperty DroppedFilesProperty =
    DependencyProperty.Register(nameof(DroppedFiles), typeof(ObservableCollection<ItemInfo>), typeof(FileDropBehavior), new PropertyMetadata(null));

    /// <summary>
    /// UserControlにDropされたファイルを受け取るコレクション
    /// </summary>
    /// <value></value>
    public ObservableCollection<ItemInfo> DroppedFiles
    {
        get => (ObservableCollection<ItemInfo>)GetValue(DroppedFilesProperty);
        set => SetValue(DroppedFilesProperty, value);
    }

    /// <summary>
    /// behaviorがUserControlにアタッチされたときに呼び出される<br/>
    /// AssociatedObjectはこのbehaviorがアタッチされているUserControlを指す
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AllowDrop = true;
        AssociatedObject.PreviewDragOver += OnDragOver;
        AssociatedObject.Drop += OnDrop;
    }

    /// <summary>
    /// behaviorがUserControlからデタッチされたときに呼び出される<br/>
    /// 設定したイベントハンドラを解除してメモリリークを防ぐ
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewDragOver -= OnDragOver;
        AssociatedObject.Drop -= OnDrop;
    }

    /// <summary>
    /// UserControlの上にドラッグされたときに呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDragOver(object sender, DragEventArgs e)
    {
        // ドラッグされているデータがファイルである場合、コピーエフェクトを表示する
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    /// <summary>
    /// ファイルがUserControlにドロップされたときに呼び出される<br/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnDrop(object sender, DragEventArgs e)
    {
        // ドロップされたデータがファイルではない場合は何もしない
        if (false == e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        // DroppedFilesがBindingされていないときは（nullのとき）は何もしない
        if (DroppedFiles is null) return;

        // ファイルパスの配列を受け取る
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        // 一時保存用リスト
        var items = new List<ItemInfo>();

        // ファイルパスからItemInfoオブジェクトを生成
        // サムネイル作成等でフリーズするため、非同期で実行する
        await Task.Run(() => files.AsParallel()
            // 時間短縮のため、並列に実行する
            .ForAll(file =>
            {
                try
                {
                    var item = new ItemInfo(file);
                    lock (ParallelLock)
                    {
                        items.Add(item);
                    }
                }
                // 拡張子非対応
                catch (ArgumentOutOfRangeException exception)
                {
                    System.Diagnostics.Debug.WriteLine($"{exception.Message}:{exception.ParamName}");
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"{exception}");
                }
            }));

        // 非同期スレッドでDroppedFilesにアクセスできないため、itemsを介して反映する
        foreach (var item in items)
        {
            DroppedFiles.Add(item);
        }
    }
}
