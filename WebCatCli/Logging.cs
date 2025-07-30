using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Serilog;
using Serilog.Core;
using WebCat;
using WebCat.Fetch.Struct;
using WebCat.Struct;
using static WebCat.Process.Utils;

namespace WebCatCli;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, IncludeFields = true)]
[JsonSerializable(typeof(string[][]))]
[JsonSerializable(typeof(Work.WorkOptions))]
[JsonSerializable(typeof(ProcessOptions))]
internal partial class LoggingJsonSourceGenerationContext : JsonSerializerContext;

public static class Logging
{
    public static readonly Logger Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("log.txt")
        .Destructure.ByTransforming<Work.WorkOptions>(options =>
            JsonSerializer.Serialize(options, LoggingJsonSourceGenerationContext.Default.WorkOptions)
        )
        .Destructure.ByTransforming<ProcessOptions>(options =>
            JsonSerializer.Serialize(options, LoggingJsonSourceGenerationContext.Default.ProcessOptions)
        )
        .Destructure.ByTransforming<WebCat.Struct.Progress<SearchEngineResult>>(progress => new
        {
            progress.Current.Title,
            progress.CurrentCount,
            progress.Total
        }).Destructure.ByTransforming<WebCat.Struct.Progress<FetchResult>>(progress => new
        {
            progress.Current.Webpage.Title,
            progress.CurrentCount,
            progress.Total
        }).Destructure.ByTransforming<Work.WorkRecord>(record =>
        {
            var results = record.Results.Select(result => result.ProcessResult.ToArray()).ToArray();
            var resultsJson = JsonSerializer.Serialize(results, LoggingJsonSourceGenerationContext.Default.StringArrayArray);
            return new
            {
                record.Query,
                Results = Regex.Unescape(resultsJson),
            };
        }).CreateLogger();
}