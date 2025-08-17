using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using WebCat;
using static WebCat.Main;

namespace WebCatWinui;

public partial class MainViewmodel : ObservableObject
{
    [ObservableProperty] public partial string? Query { get; set; }

    [ObservableProperty] public partial string Endpoint { get; set; } = "https://api.deepseek.com";

    [ObservableProperty] public partial string? ApiKey { get; set; }

    [ObservableProperty] public partial string? Model { get; set; }

    [ObservableProperty] public partial double Temperature { get; set; } = 0.7;

    [ObservableProperty] public partial double Interval { get; set; } = 1000;

    [ObservableProperty] public partial bool IsHeadless { get; set; } = true;

    [ObservableProperty] public partial double FetchingProgress { get; private set; } = 0;

    [ObservableProperty] public partial double ProcessingProgress { get; private set; } = 0;

    private Process.ProcessOptions ProcessOptions => new(
        ApiKey!,
        (float)Temperature,
        Model,
        Endpoint
    );

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExecuteQuery))]
    public partial bool IsRunning { get; private set; }

    private bool CanExecuteQuery => !IsRunning;
    
    public event Func<MainResult, Task>? WorkFinishedAsync;

    [RelayCommand(CanExecute = nameof(IsRunning))]
    public async Task WorkAsync()
    {
        IsRunning = true;

        var onFetchStart = FSharpOption<FSharpFunc<Main.Progress<Bing.SearchEngineResult>, Unit>>.Some(
            FuncConvert.FromAction((Main.Progress<Bing.SearchEngineResult> progress) =>
                {
                    if (progress.Current == 0)
                    {
                        return;
                    }

                    FetchingProgress = (double)progress.Current / progress.Total * 100;
                }
            )
        );
        var onProcessStart = FSharpOption<FSharpFunc<Main.Progress<BrowserUtils.Webpage>, Unit>>.Some(
            FuncConvert.FromAction((Main.Progress<BrowserUtils.Webpage> progress) =>
                {
                    if (progress.Current == 0)
                    {
                        return;
                    }

                    FetchingProgress = (double)progress.Current / progress.Total * 100;
                }
            )
        );
        var options = new MainOptions(
            (int)Interval,
            BrowserUtils.Browser.Chrome,
            IsHeadless,
            ProcessOptions,
            onFetchStart,
            onProcessStart
        );
        var result = await FSharpAsync.StartAsTask(runMainAsync(Query, options), null, null);
        var finishingTask = WorkFinishedAsync?.Invoke(result);
        if (finishingTask is not null)
        {
            await finishingTask;
        }
        
        IsRunning = false;
    }
}