using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.ViewModels;

internal partial class CommandDialogViewModel : ObservableObject
{
    public CommandDialogViewModel(IEnumerable<CommandInfo> commandInfos)
    {
        CommandInfos = new ObservableCollection<CommandInfo>(commandInfos);
    }

    [ObservableProperty] private ObservableCollection<CommandInfo> _commandInfos = new();
}

internal partial class CommandInfo(ItemInfo source, string command) : ObservableObject
{
    public ItemInfo Source { get; } = source;

    [ObservableProperty] private string _command = command;
}
