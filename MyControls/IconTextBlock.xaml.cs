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

using MaterialDesignThemes.Wpf;

namespace MyControls;

/// <summary>
/// Interaction logic for IconTextBlock.xaml
/// </summary>
public partial class IconTextBlock : UserControl
{
    public static readonly DependencyProperty SpaceProperty =
    DependencyProperty.Register(nameof(Space), typeof(double), typeof(IconTextBlock), new PropertyMetadata(4.0));

    public static readonly DependencyProperty IconProperty =
    DependencyProperty.Register(nameof(Icon), typeof(string), typeof(IconTextBlock), new PropertyMetadata(""));

    public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register(nameof(Text), typeof(string), typeof(IconTextBlock), new PropertyMetadata(""));

    public static readonly DependencyProperty IconSizeProperty =
    DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(IconTextBlock), new PropertyMetadata(18.0));

    public static readonly DependencyProperty TextSizeProperty =
    DependencyProperty.Register(nameof(TextSize), typeof(double), typeof(IconTextBlock), new PropertyMetadata(14.0));

    public IconTextBlock()
    {
        InitializeComponent();
    }

    /// <summary> アイコンとテキストの間の間隔 </summary>
    public double Space
    {
        get { return (double)GetValue(SpaceProperty); }
        set { SetValue(SpaceProperty, value); }
    }

    public string Icon
    {
        get { return (string)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public double IconSize
    {
        get { return (double)GetValue(IconSizeProperty); }
        set { SetValue(IconSizeProperty, value); }
    }

    public double TextSize
    {
        get { return (double)GetValue(TextSizeProperty); }
        set { SetValue(TextSizeProperty, value); }
    }
}

