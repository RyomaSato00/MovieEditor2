using System.Diagnostics;

using MovieEditor2.CommonSettingUI.ViewModels;
using MovieEditor2.MovieListUI.ViewModels;

namespace MovieEditor2.main.Library;

internal class ParallelCommandProcessor : IDisposable
{
    /// <summary> プロセス完了イベント </summary>
    public event Action? OnProgressed = null;

    private static readonly object ParallelLock = new();

    /// <summary> キャンセル機能 </summary>
    private readonly CancellationTokenSource _cancelable = new();

    /// <summary> 実行中プロセス管理用リスト </summary>
    private readonly List<Process> _processes = [];

    /// <summary> 処理済みファイル </summary>
    public List<ItemInfo> CompletedFiles { get; } = [];

    public void RunParallelly(ItemInfo[] sources, CommonSettingBoardViewModel setting, string outputDirectory)
    {
        try
        {
            sources
            .AsParallel()
            .ForAll(item =>
            {
                // キャンセルされたらここで終了
                _cancelable.Token.ThrowIfCancellationRequested();

                var info = new ProcessStartInfo("ffmpeg")
                {
                    // FFmpegのコマンドを取得する
                    Arguments = FFmpegCommandConverter.ToCompressCommand(item, setting, outputDirectory),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // プロセス作成
                using var process = new Process { StartInfo = info };

                // リストはスレッドセーフではないためロックをかける
                lock (ParallelLock)
                {
                    // 実行中プロセスとして覚えておく
                    _processes.Add(process);
                }

                // プロセススタート
                process.Start();
                process.WaitForExit();

                // リストはスレッドセーフではないためロックをかける
                lock (ParallelLock)
                {
                    // 実行完了したため、リストから削除する
                    _processes.Remove(process);

                    // 処理済みファイルを記録
                    CompletedFiles.Add(item);

                    // 進捗更新イベント発行
                    OnProgressed?.Invoke();
                }

                System.Diagnostics.Debug.WriteLine($"arg:{info.Arguments}");
            });
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"キャンセルされました");
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{e}");
        }

        System.Diagnostics.Debug.WriteLine("処理終了");

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
