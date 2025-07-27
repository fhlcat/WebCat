// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Text.Json;
using Serilog;
using Serilog.Core;
using WebCat;
using WebCat.Web;
using WebCat.Web.Browser;

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

    private static readonly Option<bool> NoHeadlessOption = new("--no-headless", "-n")
    {
        DefaultValueFactory = _ => false,
        Description = "Run the browser in non-headless mode"
    };
    
    private static readonly Option<Utils.BrowserType> BrowserTypeOption = new("--browser-type", "-b")
    {
        DefaultValueFactory = _ => Utils.BrowserType.Chrome,
        Description = "The type of browser to use"
    };

    private static readonly RootCommand RootCommand =
    [
        QuestionArgument,
        IntervalOption,
        EndpointOption,
        ApiKeyOption,
        ModelOption,
        NoHeadlessOption,
        BrowserTypeOption
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        IncludeFields = true
    };

    private static readonly Logger Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("log.txt")
        .Destructure.ByTransforming<WorkOptions>(options => new
        {
            options.Interval,
            options.Endpoint,
            options.ApiKey,
            options.Model
        })
        .Destructure.ByTransforming<WebCat.Progress<SearchResult>>(progress => new
        {
            progress.Current.Title,
            progress.CurrentCount,
            progress.Total
        }).Destructure.ByTransforming<WebCat.Progress<Webpage>>(progress => new
        {
            progress.Current.Title,
            progress.CurrentCount,
            progress.Total
        })
        .Destructure.ByTransforming<Work.WorkResult>(result => new
        {
            SearchResultTitle = result.FetchResult.SearchResult.Title,
            WebpageTitle = result.FetchResult.Webpage.Title,
            Result = result.ProcessResult.ToArray()
        })
        .CreateLogger();

    private static Task StoreWorkResultsAsync(IEnumerable<Work.WorkResult> workResults, string question)
    {
        var json = JsonSerializer.Serialize(new
        {
            Question = question,
            Results = workResults
        }, SerializerOptions);
        var fileName = $"./Results/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        Directory.CreateDirectory("./Results");
        return File.WriteAllTextAsync(fileName, json);
    }

    private static void MainAction(ParseResult parseResult)
    {
        var question = parseResult.GetValue(QuestionArgument);
        var options = new WorkOptions(
            parseResult.GetValue(IntervalOption),
            parseResult.GetValue(EndpointOption)!,
            parseResult.GetValue(ApiKeyOption)!,
            parseResult.GetValue(ModelOption)!,
            !parseResult.GetValue(NoHeadlessOption),
            parseResult.GetValue(BrowserTypeOption)
        );
        var events = new Work.WorkEvents(
            progress => Logger.Information("Fetching: {@Progress}", progress),
            progress => Logger.Information("Processing: {@Progress}", progress)
        );

        Logger.Information("Starting work with options: {@Options}", options);
        var workResults = Work.WorkAsync(question!, events, options).Result;
        Logger.Information("Completed: {@Result}", workResults);
        StoreWorkResultsAsync(workResults, question!).Wait();
    }

    static Program() => RootCommand.SetAction(MainAction);

    private static void Main(string[] args) => RootCommand.Parse(args).Invoke();
}