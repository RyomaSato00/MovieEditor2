using System.Diagnostics;

namespace MovieEditor2.Models;

public class ParallelCommandProcessor : IDisposable
{
    private static readonly object ParallelLock = new();

    private readonly CancellationTokenSource _cancelable = new();

    private readonly List<Process> _processes = [];

    public void RunParallelly(string[] source)
    {
        try
        {
            source
                .AsParallel()
                .ForAll(command =>
                {
                    // キャンセルされたらここで終了
                    _cancelable.Token.ThrowIfCancellationRequested();

                    var info = new ProcessStartInfo("ffmpeg")
                    {
                        Arguments = command,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process{ StartInfo = info };

                    // リストはスレッドセーフではないためロックをかける
                    lock(ParallelLock)
                    {
                        _processes.Add(process);
                    }

                    process.Start();
                    process.WaitForExit();

                    // リストはスレッドセーフではないためロックをかける
                    lock(ParallelLock)
                    {
                        _processes.Remove(process);
                    }

                    System.Diagnostics.Debug.WriteLine($"arg:{command}");
                });
        }
        catch(OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"キャンセルされました");
        }
        catch(Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{e}");
        }

        System.Diagnostics.Debug.WriteLine("処理終了");
    }

    public void Cancel()
    {
        // キャンセルを発行
        _cancelable.Cancel();

        // リストはスレッドセーフではないためロックをかける
        lock(ParallelLock)
        {
            foreach(var process in _processes)
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
