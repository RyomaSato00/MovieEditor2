using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MovieEditor2.main.ViewModels;

internal partial class ProgressDialogViewModel : ObservableObject
{
    public ProgressDialogViewModel(int processCount)
    {
        ProcessCount = processCount;
    }

    /// <summary> キャンセルボタン押下イベント </summary>
    public event Action? OnCanceled = null;

    // プロセス総数
    [ObservableProperty] private int _processCount;

    // 進捗カウント
    [ObservableProperty] private int _progressCount = 0;

    /// <summary>
    /// 進捗更新処理
    /// </summary>
    public void UpdateProgress()
    {
        ProgressCount++;
    }

    /// <summary>
    /// キャンセルボタン押下処理
    /// </summary>
    [RelayCommand] private void Cancel()
    {
        OnCanceled?.Invoke();
    }

}
