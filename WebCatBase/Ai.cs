using System.ClientModel;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

namespace WebCatBase;

public record struct AiOptions(string Model, string Endpoint, string ApiKey, float Temperature);

public static class Ai
{
    //todo rewrite prompt to be more concise and clear
    private const string SystemPrompt =
        """
        你是一个AI助手，专门用于阅读给定的文章并回答问题。你的任务是基于文章内容，准确、简洁地回答问题。

        输入格式：一个JSON对象，包含两个键：'article'（文章文本）和'question'（问题）。

        输出格式：一个JSON对象，包含一个键'response'，其值是一个数组，数组中包含问题的答案。如果有多个答案，请列出所有；如果只有一个，请作为数组的单一元素；如果找不到合理答案，请保留空数组。答案应直接从文章中提取或合理推断，不要添加额外解释或无关信息。

        示例输入：
        {
            "article": "小明和小红昨天一起去了公园。他们在那里玩了两个小时，之后去了附近的咖啡馆。小明点了一个拿铁，小红则点了一个焦糖玛奇朵。",
            "question": "小明点了什么饮料？"
        }

        示例输出：
        {
            "response": ["拿铁"]
        }

        严格遵守输入和输出格式，确保输出是有效的JSON。
        """;

    private static IEnumerable<string> ParseResponse(string responseText)
    {
        return JsonDocument
            .Parse(responseText).RootElement
            .GetProperty("response")
            .EnumerateArray()
            .Select(element => element.GetString()!);
    }

    public static Func<string, string, Task<IEnumerable<string>>> PrepareParseAsync(AiOptions options)
    {
        var chatClient = new ChatClient(
            options.Model,
            new ApiKeyCredential(options.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri(options.Endpoint) }
        );
        return async (article, question) =>
        {
            var result = await chatClient.CompleteChatAsync(
                (ChatMessage[])
                [
                    new SystemChatMessage(SystemPrompt),
                    new UserChatMessage(JsonSerializer.Serialize(new { article, question }))
                ],
                new ChatCompletionOptions
                    { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(), Temperature = options.Temperature }
            );
            return ParseResponse(result.Value.Content[0].Text!);
        };
    }
}