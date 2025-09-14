// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Serilog;
using WebCat;

namespace WebCatCli;

public static class Program
{
    private static void MainAction(CliHelper.CliParameters parameters)
    {
        var aiClient = new AiClient(
            parameters.Model,
            parameters.Endpoint,
            parameters.ApiKey
        );
        var webcat = new WebCat.WebCat(aiClient);
        webcat.OnFetching += (title, current, total) =>
            Log.Information("Fetching ({Current}/{Total}): {Title}", current + 1, total, title);
        webcat.OnProcessing += (title, current, total) =>
            Log.Information("Processing ({Current}/{Total}): {Title}", current + 1, total, title);

        Log.Information("Starting work");
        var question = parameters.Question;
        var result = webcat.WorkAsync(question).Result;
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
        
        var cliHelper = new CliHelper(MainAction);
        cliHelper.Invoke(args);
    }
}