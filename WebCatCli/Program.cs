// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Text.Json;
using Serilog;
using WebCat;
using WebCat.Fetch.Browser;
using static WebCat.Work;

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

    private static readonly Option<Utils.BrowserType> BrowserTypeOption = new("--browser-type", "-b")
    {
        DefaultValueFactory = _ => Utils.BrowserType.Chrome,
        Description = "The type of browser to use"
    };

    private static readonly Option<float> TemperatureOption = new("--temperature", "-t")
    {
        DefaultValueFactory = _ => 0,
        Description = "The temperature for the AI model, controlling randomness in responses, must be between 0.0 and 1.0"
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

    private static Task StoreWorkRecordAsync(WorkRecord record)
    {
        var json = JsonSerializer.Serialize(record, JsonSourceGenerationContext.Default.WorkRecord);
        var fileName = $"./Results/{record.Query}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        Directory.CreateDirectory("./Results");
        return File.WriteAllTextAsync(fileName, json);
    }

    private static void MainAction(ParseResult parseResult)
    {
        var question = parseResult.GetValue(QuestionArgument);
        var processOptions = new WebCat.Process.Utils.ProcessOptions(
            parseResult.GetValue(ModelOption)!,
            parseResult.GetValue(EndpointOption)!,
            parseResult.GetValue(ApiKeyOption)!,
            parseResult.GetValue(TemperatureOption)
        );
        var options = new WorkOptions(
            parseResult.GetValue(IntervalOption),
            processOptions,
            parseResult.GetValue(HeadlessOption),
            parseResult.GetValue(BrowserTypeOption)
        );
        var events = new WorkEvents(
            progress => Log.Information("Fetching: {@Progress}", progress),
            progress => Log.Information("Processing: {@Progress}", progress)
        );

        Log.Information("Starting work with options: {@Options}", options);
        var record = WorkAsync(question!, events, options).Result;
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