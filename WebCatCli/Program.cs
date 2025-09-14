// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Serilog;
using WebCatBase;

namespace WebCatCli;

public static class Program
{
    private static void OnProcessing(string title, int current, int total) =>
        Log.Information("Processing ({Current}/{Total}): {Title}", current + 1, total, title);

    private static void OnFetching(string title, int current, int total) =>
        Log.Information("Fetching ({Current}/{Total}): {Title}", current + 1, total, title);

    private static void MainAction(Cli.CliParameters parameters)
    {
        var aiOptions = new AiOptions
        {
            ApiKey = parameters.ApiKey, Endpoint = parameters.Endpoint, Model = parameters.Model,
            Temperature = parameters.Temperature
        };

        Log.Information("Starting work");
        var question = parameters.Question;
        var result = WebCat.WorkAsync(question, aiOptions, OnFetching, OnProcessing);
        Log.Information("Completed work");

        var json = JsonSerializer.Serialize(result);
        var fileName = $"./Results/{question}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        Directory.CreateDirectory("./Results");
        File.WriteAllTextAsync(fileName, json);
    }

    private static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt")
            .CreateLogger();

        Cli.Invoke(MainAction, args);
    }
}