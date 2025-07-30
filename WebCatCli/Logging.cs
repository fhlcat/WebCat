using System.Text.Json;
using System.Text.RegularExpressions;
using Serilog;
using Serilog.Core;
using WebCat;
using static WebCat.Main;
using static WebCat.Process;

namespace WebCatCli;

public static class Logging
{
    public static readonly Logger Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("log.txt")
        .Destructure.ByTransforming<MainOptions>(options =>
            JsonSerializer.Serialize(options, LoggingJsonSourceGenerationContext.Default.MainOptions)
        )
        .Destructure.ByTransforming<ProcessOptions>(options =>
            JsonSerializer.Serialize(options, LoggingJsonSourceGenerationContext.Default.ProcessOptions)
        )
        .Destructure.ByTransforming<Main.Progress<Bing.SearchEngineResult>>(progress => new
        {
            progress.CurrentWorking.Title,
            Current = progress.Current + 1,
            progress.Total
        })
        .Destructure.ByTransforming<Main.Progress<BrowserUtils.Webpage>>(progress => new
        {
            progress.CurrentWorking.Title,
            Current = progress.Current + 1,
            progress.Total
        })
        .Destructure.ByTransforming<MainResult>(result => new
        {
            result.Query,
            Results = SerializeResult(result)
        })
        .CreateLogger();

    private static string SerializeResult(MainResult result)
    {
        var resultsArray = result.Value.Select(pair => pair.Value).ToArray();
        var json = JsonSerializer.Serialize(
            resultsArray,
            LoggingJsonSourceGenerationContext.Default.StringArrayArray
        );
        return Regex.Unescape(json);
    }
}