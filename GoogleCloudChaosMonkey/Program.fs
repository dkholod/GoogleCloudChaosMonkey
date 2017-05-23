module Main

open System
open System.Threading
open MonkeyCore
open MonkeyManager

[<EntryPoint>]
let main argv =
    printfn "Google Cloud Chaos Monkey started. Duration: %A" (Configuration.TestDuration |> parseTimeInterval)

    Configuration.ChaosGroups |> createMonkeys

    Thread.Sleep(TimeSpan.FromSeconds(Configuration.TestDuration |> stringToSeconds |> float))
    printfn "Google Cloud Chaos Monkey finished."

    0