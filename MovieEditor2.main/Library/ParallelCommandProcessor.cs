using System.Diagnostics;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.main.ViewModels;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.Library;

internal class ParallelCommandProcessor : IDisposable
{
    /// <summary> プロセス進捗イベント </summary>
    public event Action? OnProgressed = null;

    private static readonly object ParallelLock = new();

    /// <summary> キャンセル機能 </summary>
    private readonly CancellationTokenSource _cancelable = new();

    /// <summary> 実行中プロセス管理用リスト </summary>
    private readonly List<Process> _processes = [];

    /// <summary> 処理済みファイル </summary>
    public List<ItemInfo> CompletedFiles { get; } = [];

    /// <summary>
    /// 並列実行
    /// </summary>
    /// <param name="commandInfos"> コマンド情報 </param>
    public void RunParallelly(IEnumerable<CommandInfo> commandInfos)
    {
        commandInfos
        .AsParallel()
        .ForAll(commandInfo =>
        {
            try
            {
                var info = new ProcessStartInfo("ffmpeg")
                {
                    // FFmpegコマンド
                    Arguments = commandInfo.Command,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // プロセス作成
                using var process = new Process { StartInfo = info };

                // リストはスレッドセーフではないため、ロックをかける
                lock (ParallelLock)
                {
                    // 実行中プロセスとして覚えておく
                    _processes.Add(process);
                }

                // プロセススタート
                process.Start();
                process.WaitForExit();

                // キャンセルされたらここで終了
                _cancelable.Token.ThrowIfCancellationRequested();

                // リストはスレッドセーフではないため、ロックをかける
                lock (ParallelLock)
                {
                    // 実行完了したため、リストから削除する
                    _processes.Remove(process);

                    // 処理済みファイルを記録
                    CompletedFiles.Add(commandInfo.Source);

                    // 進捗更新イベント発行
                    OnProgressed?.Invoke();
                }

                Debug.WriteLine($"arg:{info.Arguments}");
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("キャンセルされました");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}");
            }
        });

        Debug.WriteLine("処理終了");
    }

    /// <summary>
    /// 並列処理キャンセル
    /// </summary>
    public void Cancel()
    {
        // キャンセルを発行
        _cancelable.Cancel();

        // リストはスレッドセーフではないためロックをかける
        lock (ParallelLock)
        {
            foreach (var process in _processes)
            {
                // プロセス中断
                process.Kill();

                // プロセス破棄
                process.Dispose();
            }

            _processes.Clear();
        }
    }

    public void Dispose()
    {
        Cancel();
    }
}
