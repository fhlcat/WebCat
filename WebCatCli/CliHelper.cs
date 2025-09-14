using System.CommandLine;

namespace WebCatCli;

public class CliHelper
{
    private readonly Argument<string> _questionArgument = new("question")
    {
        Description = "The question to gather information about"
    };

    private readonly Option<string> _endpointOption = new("--endpoint", "-e")
    {
        DefaultValueFactory = _ => "https://api.deepseek.com",
        Description = "The endpoint for the AI service"
    };

    private readonly Option<string> _apiKeyOption = new("--api-key", "-a")
    {
        Required = true,
        Description = "The API key for the AI service"
    };

    private readonly Option<string> _modelOption = new("--model", "-m")
    {
        Required = true,
        Description = "The model to use for the AI service"
    };

    private readonly Option<float> _temperatureOption = new("--temperature", "-t")
    {
        DefaultValueFactory = _ => 0,
        Description =
            "The temperature for the AI model, controlling randomness in responses, must be between 0.0 and 1.0"
    };

    private readonly RootCommand _rootCommand;

    public record struct CliParameters
    {
        public required string Question { get; init; }
        public required string Endpoint { get; init; }
        public required string ApiKey { get; init; }
        public required string Model { get; init; }
        public required float Temperature { get; init; }
    }

    public CliHelper(Action<CliParameters> action)
    {
        _temperatureOption.Validators.Add(result =>
        {
            var value = result.GetValueOrDefault<float>();
            if (value < 0.0 || value > 1.0) result.AddError("--temperature must be between 0.0 and 1.0");
        });
        
        _rootCommand = [
            _questionArgument,
            _endpointOption,
            _apiKeyOption,
            _modelOption,
            _temperatureOption
        ];
        
        _rootCommand.SetAction(parseResult =>
        {
            action(new CliParameters
            {
                Question = parseResult.GetRequiredValue(_questionArgument),
                Endpoint = parseResult.GetRequiredValue(_endpointOption),
                ApiKey = parseResult.GetRequiredValue(_apiKeyOption),
                Model = parseResult.GetRequiredValue(_modelOption),
                Temperature = parseResult.GetRequiredValue(_temperatureOption)
            });
        });
    }

    public void Invoke(string[] args) => _rootCommand.Parse(args).Invoke();
}