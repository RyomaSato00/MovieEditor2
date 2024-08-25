using System.IO;
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

using Microsoft.Win32;

namespace MyControls;

/// <summary>
/// Interaction logic for PathBox.xaml
/// </summary>
public partial class PathBox : UserControl
{
    // デフォルトでTwoWayBinding
    public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register(nameof(Text), typeof(string), typeof(PathBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public PathBox()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private void Reference_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Multiselect = false,
            Title = "出力先フォルダを選択",
        };

        // ダイアログボックスの初期表示をTextにする
        if(Directory.Exists(Text))
        {
            dialog.InitialDirectory = Text;
        }

        // ダイアログボックスオープン
        if(dialog.ShowDialog() == true)
        {
            Text = dialog.FolderName;
        }
    }
}

