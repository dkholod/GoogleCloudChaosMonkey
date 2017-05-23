[<RequireQualifiedAccess>]
module Configuration

open FSharp.Configuration
open System.IO
open System.Reflection
open MonkeyCore

type private Config = YamlConfig<"config.yaml">

let private toGroup projectId (cfgGroup: Config.groups_Item_Type) =
    {
        ProjectId = projectId
        Name = cfgGroup.name
        Zone = cfgGroup.zone
        Instances = cfgGroup.instances |> List.ofSeq
        Failure = {
                    Interval = cfgGroup.failure.interval |> parseTimeInterval
                    Probability = cfgGroup.failure.probability
                    Strategy = parseStrategy cfgGroup.failure.strategy
                    }
    }

let private config =
    let cfg = Config()
    let cfgPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.yaml")
    cfg.Load(cfgPath)
    cfg

let ChaosGroups =
    config.groups 
        |> List.ofSeq
        |> List.map (fun g -> toGroup config.``project-id`` g)

let TestDuration = config.duration