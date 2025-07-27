// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using Serilog;
using WebCat;

namespace WebCatCli;

internal static class Program
{
    private static void MainAction(ParseResult parseResult)
    {
        var question = parseResult.GetValue<string>("question");
        var options = new Work.WorkOptions(parseResult.GetValue<int>("interval"), parseResult.GetValue<string>("endpoint")!,
            parseResult.GetValue<string>("api-key")!, parseResult.GetValue<string>("model")!);
        using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt")
            .CreateLogger();
        Log.Logger = log;
        var events = new Work.WorkingEvents(
            fetching => Log.Information("Fetching: {Progress}", fetching),
            processing => Log.Information("Processing: {Progress}", processing)
        );

        Work.WorkAsync(question!, events, options).Wait();
    }

    private static void Main(string[] args)
    {
        var questionArgument = new Argument<string>("question")
        {
            Description = "The question to gather information about"
        };
        var intervalOption = new Option<int>("--interval", "-i")
        {
            DefaultValueFactory = _ => 1000,
            Description = "The interval in milliseconds to wait between web spider requests",
        };
        var endpointOption = new Option<string>("--endpoint", "-e")
        {
            DefaultValueFactory = _ => "https://api.deepseek.com",
            Description = "The endpoint for the AI service"
        };
        var apiKeyOption = new Option<string>("--api-key", "-a")
        {
            Required = true,
            Description = "The API key for the AI service"
        };
        var modelOption = new Option<string>("--model", "-m")
        {
            Required = true,
            Description = "The model to use for the AI service"
        };
        var rootCommand = new RootCommand
        {
            questionArgument,
            intervalOption,
            endpointOption,
            apiKeyOption,
            modelOption
        };
        rootCommand.SetAction(MainAction);
        rootCommand.Parse(args).Invoke();
    }
}