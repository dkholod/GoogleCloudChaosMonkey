module MonkeyCore

open System

type TimeInterval = 
    | Seconds of s : UInt32
    | Minutes of m : UInt32
    | Hours of h : UInt32

type FaultStrategy = 
    | Reset
    | StopStart
    | NoAction

type Failure = { 
        Interval: TimeInterval
        WaitBeforeStart: TimeInterval
        Probability: float
        Strategy: FaultStrategy
        }

type ChaosGroup = {
    ProjectId: string
    Name: string
    Zone: string
    Instances: string list
    Failure: Failure
    }

type InstantFailure = {
    Instance: string
    ProjectId: string
    Zone: string
    Failure: FaultStrategy
    }

let parseStrategy = function
    | "StopStart" -> StopStart
    | "Reset" -> Reset
    | _ -> NoAction

// Converts 10s -> Seconds 10  
let parseTimeInterval (str: string) = 
    let lastChar = (str.[str.Length-1]).ToString().ToLowerInvariant()
    let number = str.Substring(0, str.Length-1);
    match lastChar with 
    | "s" -> Seconds (number |> uint32)
    | "m" -> Minutes (number |> uint32)
    | "h" -> Hours (number |> uint32)
    | _ -> Seconds (str |> uint32)

let toSeconds = function
        | Seconds s -> s
        | Minutes m -> 60u * m
        | Hours h -> 60u * 60u * h

let inline toMiliseconds seconds = seconds * 1000u

let stringToSeconds = parseTimeInterval >> toSeconds

let generateFailure (group: ChaosGroup) (probability: float) (instanceIdx:int) = 
    let instance = group.Instances |> List.item instanceIdx
    let chance = (probability <= group.Failure.Probability)
    let fail = { 
                Failure = group.Failure.Strategy 
                Instance = instance
                ProjectId = group.ProjectId
                Zone = group.Zone
                }

    match chance with
        | true -> Some fail
        | false -> None