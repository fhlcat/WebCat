using System.CommandLine;

namespace WebCatCli;

public static class Cli
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

    public record struct CliParameters
    {
        public required string Question { get; init; }
        public required string Endpoint { get; init; }
        public required string ApiKey { get; init; }
        public required string Model { get; init; }
        public required float Temperature { get; init; }
    }

    public static int Invoke(Action<CliParameters> action, IReadOnlyList<string> args)
    {
        TemperatureOption.Validators.Add(result =>
        {
            var value = result.GetValueOrDefault<float>();
            if (value < 0.0 || value > 1.0) result.AddError("--temperature must be between 0.0 and 1.0");
        });

        RootCommand rootCommand =
        [
            QuestionArgument,
            EndpointOption,
            ApiKeyOption,
            ModelOption,
            TemperatureOption
        ];

        rootCommand.SetAction(parseResult =>
        {
            action(new CliParameters
            {
                Question = parseResult.GetRequiredValue(QuestionArgument),
                Endpoint = parseResult.GetRequiredValue(EndpointOption),
                ApiKey = parseResult.GetRequiredValue(ApiKeyOption),
                Model = parseResult.GetRequiredValue(ModelOption),
                Temperature = parseResult.GetRequiredValue(TemperatureOption)
            });
        });

        return rootCommand.Parse(args).Invoke();
    }
}