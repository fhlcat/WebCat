module WebCat.Process

open System
open System.ClientModel
open System.Text.Json
open System.Text.Json.Nodes
open OpenAI
open OpenAI.Chat

let private systemPrompt =
    """你是一个AI助手，专门用于阅读给定的文章并回答问题。你的任务是基于文章内容，准确、简洁地回答问题。

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

严格遵守输入和输出格式，确保输出是有效的JSON。 """

let private parseResponse (responseText: string) : string seq =
    JsonDocument
        .Parse(responseText)
        .RootElement.GetProperty("response")
        .EnumerateArray()
    |> Seq.map _.GetString()

[<Struct>]
type ProcessRequest = { Article: string; Question: string }

let private formatProcessRequest (request: ProcessRequest) : string =
    let object = JsonObject()
    object["article"] <- request.Article
    object["question"] <- request.Question
    object.ToJsonString()

let private performRequest (client: ChatClient) (temperature: float32) (request: ProcessRequest) =
    async {
        let messages: ChatMessage array =
            [| SystemChatMessage(systemPrompt)
               UserChatMessage(formatProcessRequest request) |]

        let options =
            ChatCompletionOptions(
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                Temperature = temperature
            )

        let! clientResult = client.CompleteChatAsync(messages, options) |> Async.AwaitTask
        return clientResult.Value.Content[0].Text |> parseResponse
    }

[<Struct>]
type ProcessOptions =
    { ApiKey: string
      Temperature: float32
      Model: string
      Endpoint: string }

let processAsync (options: ProcessOptions) =
    let client =
        ChatClient(
            options.Model,
            ApiKeyCredential(options.ApiKey),
            OpenAIClientOptions(Endpoint = Uri(options.Endpoint))
        )

    performRequest client options.Temperature
