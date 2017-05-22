module Main

open System
open System.Threading
open System.Reflection
open System.IO
open FSharp.Configuration
open MonkeyCore
open MonkeyManager

type Config = YamlConfig<"config.yaml">
let config = Config()
let cfgPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.yaml")
config.Load(cfgPath);

let toGroup (projectId: string) (cfgGroup: Config.groups_Item_Type) = 
    { 
        ProjectId = projectId
        Name = cfgGroup.name
        Zone = cfgGroup.zone
        Instances = cfgGroup.instances |> List.ofSeq
        Failure = {
                    Interval = cfgGroup.failure.interval |> parseTimeInterval
                    WaitBeforeStart = cfgGroup.failure.``wait-before-start`` |> parseTimeInterval
                    Probability = cfgGroup.failure.probability
                    Strategy = parseStrategy cfgGroup.failure.strategy
                    }
    }

[<EntryPoint>]
let main argv = 
    printfn "Google Cloud Chaos Monkey started. Duration: %A" (config.duration |> parseTimeInterval)
    
    let groups = 
        config.groups 
        |> List.ofSeq
        |> List.map (fun g -> toGroup config.``project-id`` g)

    createMonkeys groups |> ignore
    
    Thread.Sleep(TimeSpan.FromSeconds(config.duration |> stringToSeconds |> float))
    printfn "Google Cloud Chaos Monkey finished."
    
    0