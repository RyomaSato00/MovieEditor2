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

using MovieEditor2.main.ViewModels;
using MovieEditor2.Models;

namespace MovieEditor2.main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public MainWindow()
    {
        // キャッシュ削除
        MovieFileProcessor.DeleteThumbnailCaches();

        _mainWindowViewModel = new MainWindowViewModel();

        DataContext = _mainWindowViewModel;

        InitializeComponent();
    }
}
