open System
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open WebCat.Main
open WebCat.Process

//todo use db
let tasks = Dictionary<string, Task<MainResult>>()

[<Struct>]
type StartTaskParameters =
    { Query: string
      FetchOptions: FetchOptions
      ProcessOptions: ProcessOptions }

[<Struct>]
type StartTaskResponseLinks = { Result: string }

[<Struct>]
type StartTaskResponse =
    { TaskId: string
      Links: StartTaskResponseLinks }

let startTask (startTaskParameters: StartTaskParameters) =
    let id = Guid.NewGuid().ToString()

    let options =
        { FetchOptions = startTaskParameters.FetchOptions
          ProcessOptions = startTaskParameters.ProcessOptions
          OnFetchStart = None
          OnProcessStart = None }

    let task = runMainAsync startTaskParameters.Query options |> Async.StartAsTask
    tasks.Add(id, task)

    { TaskId = id
      Links = { Result = $"/tasks/{id}/result" } }

let getTaskResult taskId =
    match tasks.TryGetValue(taskId) with
    | true, task -> Results.Ok task.Result
    | _ -> Results.NotFound "Not completed or not exist"

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    app.MapPost("/tasks", Func<StartTaskParameters, StartTaskResponse>(startTask))
    |> ignore

    app.MapGet("/tasks/{taskId}/result", Func<string, IResult>(getTaskResult))
    |> ignore

    app.Run()

    0