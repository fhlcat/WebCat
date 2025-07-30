module WebCat.Main

open System.Collections.Generic
open System.Threading.Channels
open FSharp.Control
open WebCat.Bing
open WebCat.BrowserUtils
open WebCat.Process

[<Struct>]
type Progress<'a> =
    { CurrentWorking: 'a
      Current: int
      Total: int }

[<Struct>]
type MainOptions =
    { RefreshInterval: int
      Browser: Browser
      Headless: bool
      ProcessOptions: ProcessOptions
      OnFetchStart: Option<Progress<SearchEngineResult> -> unit>
      OnProcessStart: Option<Progress<Webpage> -> unit> }

[<Struct>]
type MainResult =
    { Query: string
      Value: KeyValuePair<Webpage, string array> array }

let toArrayParallelSequenceAsync (mapping: int64 -> 'a -> Async<'b>) (source: AsyncSeq<'a>) : Async<'b array> =
    let channel = Channel.CreateUnbounded<'a>()

    async {
        let productorIterator item =
            async {
                let task = channel.Writer.WriteAsync item
                do! task.AsTask() |> Async.AwaitTask
            }

        do! AsyncSeq.iterAsync productorIterator source
        channel.Writer.Complete()
    }
    |> Async.Start

    channel.Reader.ReadAllAsync()
    |> AsyncSeq.ofAsyncEnum
    |> AsyncSeq.mapiAsync mapping
    |> AsyncSeq.toArrayAsync

let callIfHave (wrapper: Option<'a -> unit>) (arg: 'a) =
    match wrapper with
    | None -> ()
    | Some f -> f arg

let runMainAsync (query: string) (options: MainOptions) : Async<MainResult> =
    let driver = initWebDriver (options.Browser, options.Headless)

    async {
        let! searchResults = fetchBingResultsAsync driver query
        let total = searchResults.Length
        let processAsync = processAsync options.ProcessOptions

        let fetchiAsync (i: int64) (result: SearchEngineResult) =
            callIfHave
                options.OnFetchStart
                { Current = int i
                  Total = total
                  CurrentWorking = result }

            async {
                let! webpage = fetchWebpageAsync driver result.Url
                do! Async.Sleep options.RefreshInterval
                return webpage
            }

        let processiAsync (i: int64) (webpage: Webpage) =
            callIfHave
                options.OnProcessStart
                { Current = int i
                  Total = total
                  CurrentWorking = webpage }

            async {
                let! result =
                    processAsync
                        { Article = webpage.Content
                          Question = query }

                return KeyValuePair(webpage, result |> Array.ofSeq)
            }

        let! results =
            searchResults
            |> AsyncSeq.ofSeq
            |> AsyncSeq.mapiAsync fetchiAsync
            |> toArrayParallelSequenceAsync processiAsync

        driver.Close()
        driver.Dispose()

        return { Query = query; Value = results }
    }
