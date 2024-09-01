using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using MaterialDesignThemes.Wpf;

using Microsoft.Xaml.Behaviors;

namespace MyControls;

public class OpenExplorerBehavior : Behavior<TextBox>
{
    /// <summary>
    /// behaviorがTextBoxにアタッチされたときに呼び出される<br/>
    /// AssociatedObjectはこのbehaviorがアタッチされているTextBoxを指す
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();

        // デフォルトアイテム定義
        FrameworkElement[] defaultMenuItems =
        [
            new MenuItem{ Header="切り取り", Command = ApplicationCommands.Cut, Icon = new PackIcon{ Kind = PackIconKind.ContentCut} },
            new MenuItem{ Header="コピー", Command= ApplicationCommands.Copy, Icon = new PackIcon{Kind=PackIconKind.ContentCopy} },
            new MenuItem{ Header="貼り付け", Command= ApplicationCommands.Paste, Icon = new PackIcon{Kind = PackIconKind.ContentPaste} },
            new Separator(),
            new MenuItem{ Header="すべて選択", Command = ApplicationCommands.SelectAll, Icon = new PackIcon{Kind = PackIconKind.SelectAll} }
        ];

        // デフォルトコンテキストメニュー作成
        var menu = new ContextMenu();

        foreach (var item in defaultMenuItems)
        {
            menu.Items.Add(item);
        }

        // 「エクスプローラーで開く」メニューアイテムを定義
        var openExplorerItem = new MenuItem { Header = "エクスプローラで開く", Icon = new PackIcon { Kind = PackIconKind.FolderOpen } };
        openExplorerItem.Click += (_, _) => OpenExplorer(AssociatedObject.Text);

        menu.Items.Add(new Separator());
        menu.Items.Add(openExplorerItem);

        // 新しいコンテキストメニューに置き換え
        AssociatedObject.ContextMenu = menu;
    }

    /// <summary>
    /// エクスプローラでpathのフォルダを開く
    /// </summary>
    /// <param name="path"></param>
    private static void OpenExplorer(string path)
    {
        var info = new ProcessStartInfo("EXPLORER.EXE") { Arguments = path, UseShellExecute = false };

        using var process = new Process { StartInfo = info };

        process.Start();
        _ = process.WaitForExitAsync();
    }
}
