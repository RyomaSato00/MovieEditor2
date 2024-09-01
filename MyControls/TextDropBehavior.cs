using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Microsoft.Xaml.Behaviors;

namespace MyControls;

public class TextDropBehavior : Behavior<TextBox>
{
    /// <summary>
    /// behaviorがTextBoxにアタッチされたときに呼び出される<br/>
    /// AssociatedObjectはこのbehaviorがアタッチされているTextBoxを指す
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.AllowDrop = true;
        AssociatedObject.PreviewDragOver += OnDragOver;
        AssociatedObject.Drop += OnDrop;
    }

    /// <summary>
    /// behaviorがTextBoxからデタッチされたときに呼び出される<br/>
    /// 設定したイベントハンドラを解除してメモリリークを防ぐ
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewDragOver -= OnDragOver;
        AssociatedObject.Drop -= OnDrop;
    }

    /// <summary>
    /// TextBox上をドラッグされたときに呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnDragOver(object sender, DragEventArgs e)
    {
        // ドラッグされているデータがファイルである場合、コピーエフェクトを表示する
        if(e.Data.GetDataPresent(DataFormats.FileDrop))
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
    /// TextBoxにDropしたときの処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDrop(object sender, DragEventArgs e)
    {
        // ドロップされたデータがファイルではない場合は何もしない
        if(false == e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        // ファイルパスの配列を受け取る
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        // 複数ファイルが選択されている場合は最初の要素をターゲットとする
        var target = files[0];

        // ターゲットがディレクトリパスのとき
        if(Directory.Exists(target))
        {
            // テキストに反映する
            AssociatedObject.Text = target;

            // TextBoxのBindingを更新する
            BindingOperations.GetBindingExpression(AssociatedObject, TextBox.TextProperty).UpdateSource();
        }
    }
}
