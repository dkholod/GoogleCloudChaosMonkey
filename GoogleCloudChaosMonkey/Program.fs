module Main

open System
open System.Threading
open MonkeyCore
open MonkeyManager
open Serilog

[<EntryPoint>]
let main argv =
    let logger = (new LoggerConfiguration())
                    .MinimumLevel.Debug()
                    .WriteTo.LiterateConsole()
                    .WriteTo.RollingFile("logs\\log-{Date}.txt")
                    .CreateLogger()
    Log.Logger <- logger

    Log.Logger.Information ("Google Cloud Chaos Monkey started. Chaos test duration: {duration}.", Configuration.TestDuration)

    Configuration.ChaosGroups |> createMonkeys

    Thread.Sleep(TimeSpan.FromSeconds(Configuration.TestDuration |> stringToSeconds |> float))
    Log.Logger.Information "Google Cloud Chaos Monkey finished."

    0