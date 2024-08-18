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
/// Interaction logic for TrimmingArea.xaml
/// </summary>
public partial class TrimmingArea : UserControl
{
    public static readonly DependencyProperty CurrentTimeProperty =
    DependencyProperty.Register(nameof(CurrentTime), typeof(TimeSpan), typeof(TrimmingArea), new PropertyMetadata(TimeSpan.Zero));

    /// <summary> 動画再生現在時刻 </summary>
    public TimeSpan CurrentTime
    {
        get => (TimeSpan)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    public TrimmingArea()
    {
        InitializeComponent();
    }
}

