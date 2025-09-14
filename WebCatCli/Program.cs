// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Text.Json;
using Serilog;
using WebCat;

namespace WebCatCli;

internal static class Program
{
    private static readonly Argument<string> QuestionArgument = new("question")
    {
        Description = "The question to gather information about"
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

    private static readonly Option<float> TemperatureOption = new("--temperature", "-t")
    {
        DefaultValueFactory = _ => 0,
        Description =
            "The temperature for the AI model, controlling randomness in responses, must be between 0.0 and 1.0"
    };

    private static readonly RootCommand RootCommand =
    [
        QuestionArgument,
        EndpointOption,
        ApiKeyOption,
        ModelOption,
        TemperatureOption
    ];

    static Program()
    {
        TemperatureOption.Validators.Add(result =>
        {
            var value = result.GetValueOrDefault<float>();
            if (value < 0.0 || value > 1.0) result.AddError("--temperature must be between 0.0 and 1.0");
        });
        RootCommand.SetAction(MainAction);
        
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }

    private static void MainAction(ParseResult parseResult)
    {
        var aiClient = new AiClient(
            parseResult.GetRequiredValue(ModelOption),
            parseResult.GetRequiredValue(EndpointOption),
            parseResult.GetRequiredValue(ApiKeyOption)
        );
        var webcat = new WebCat.WebCat(aiClient);
        webcat.OnFetching += (title, current, total) =>
            Log.Information("Fetching ({Current}/{Total}): {Title}", current + 1, total, title);
        webcat.OnProcessing += (title, current, total) =>
            Log.Information("Processing ({Current}/{Total}): {Title}", current + 1, total, title);

        Log.Information("Starting work");
        var question = parseResult.GetRequiredValue(QuestionArgument);
        var result = webcat.WorkAsync(question).Result;
        Log.Information("Completed work");

        var json = JsonSerializer.Serialize(result);
        var fileName = $"./Results/{question}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        Directory.CreateDirectory("./Results");
        File.WriteAllTextAsync(fileName, json);
    }

    private static void Main(string[] args)
    {
        RootCommand.Parse(args).Invoke();
    }
}