// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Text.Json;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Serilog;
using WebCat;
using static WebCat.BrowserUtils;
using static WebCat.Main;
using FetchProgress = WebCat.Main.Progress<WebCat.Bing.SearchEngineResult>;
using ProcessProgress = WebCat.Main.Progress<WebCat.BrowserUtils.Webpage>;

namespace WebCatCli;

internal static class Program
{
    private static readonly Argument<string> QuestionArgument = new("question")
    {
        Description = "The question to gather information about"
    };

    private static readonly Option<int> IntervalOption = new("--interval", "-i")
    {
        Required = true,
        DefaultValueFactory = _ => 1000,
        Description = "The interval in milliseconds to wait between web spider requests"
    };

    private static readonly Option<string> EndpointOption = new("--endpoint", "-e")
    {
        DefaultValueFactory = _ => "https://api.deepseek.com",
        Description = "The endpoint for the AI service"
    };

    private static readonly Option<string> ApiKeyOption = new("--api-key", "-a")
    {
        Required = true,
        Description = "The API key for the AI service"
    };

    private static readonly Option<string> ModelOption = new("--model", "-m")
    {
        Required = true,
        Description = "The model to use for the AI service"
    };

    private static readonly Option<bool> HeadlessOption = new("--headless", "-n")
    {
        DefaultValueFactory = _ => false,
        Description = "Run the browser in non-headless mode"
    };

    private static readonly Option<Browser> BrowserTypeOption = new("--browser-type", "-b")
    {
        DefaultValueFactory = _ => Browser.Chrome,
        Description = "The type of browser to use"
    };

    private static readonly Option<float> TemperatureOption = new("--temperature", "-t")
    {
        DefaultValueFactory = _ => 0,
        Description =
            "The temperature for the AI model, controlling randomness in responses, must be between 0.0 and 1.0"
    };

    private static readonly RootCommand RootCommand =
    [
        QuestionArgument,
        IntervalOption,
        EndpointOption,
        ApiKeyOption,
        ModelOption,
        HeadlessOption,
        BrowserTypeOption,
        TemperatureOption
    ];

    private static Task StoreWorkRecordAsync(MainResult result)
    {
        var json = JsonSerializer.Serialize(result, LoggingJsonSourceGenerationContext.Default.MainResult);
        var fileName = $"./Results/{result.Query}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        Directory.CreateDirectory("./Results");
        return File.WriteAllTextAsync(fileName, json);
    }

    private static void MainAction(ParseResult parseResult)
    {
        var query = parseResult.GetValue(QuestionArgument);
        var processOptions = new Process.ProcessOptions(
            parseResult.GetValue(ApiKeyOption)!,
            parseResult.GetValue(TemperatureOption),
            parseResult.GetValue(ModelOption)!,
            parseResult.GetValue(EndpointOption)!
        );
        var options = new MainOptions(
            parseResult.GetValue(IntervalOption),
            parseResult.GetValue(BrowserTypeOption),
            parseResult.GetValue(HeadlessOption),
            processOptions,
            FSharpOption<FSharpFunc<FetchProgress, Unit>>.Some(
                FuncConvert.FromAction((FetchProgress progress) => Log.Information("Fetching: {@Progress}", progress))
            ),
            FSharpOption<FSharpFunc<ProcessProgress, Unit>>.Some(
                FuncConvert.FromAction((ProcessProgress progress) => Log.Information("Processing: {@Progress}", progress))
            )
        );

        Log.Information("Starting work with options: {@Options}", options);
        var record = FSharpAsync.StartAsTask(runMainAsync(query, options), null, null).Result;
        Log.Information("Completed: {@Record}", record);
        StoreWorkRecordAsync(record).Wait();
    }

    static Program()
    {
        TemperatureOption.Validators.Add(result =>
        {
            var value = result.GetValueOrDefault<float>();
            if (value < 0.0 || value > 1.0)
            {
                result.AddError("--temperature must be between 0.0 and 1.0");
            }
        });
        RootCommand.SetAction(MainAction);
        Log.Logger = Logging.Logger;
    }

    private static void Main(string[] args) => RootCommand.Parse(args).Invoke();
}