module MonkeyManager

open System
open MonkeyCore
open Serilog

let getRandomizer =
    let r = new Random()
    fun () -> r

let generateProbability() =
    getRandomizer().NextDouble()

let getRandomNumber maxVal =
    getRandomizer().Next(maxVal)

let induceChaos group (t: System.Timers.Timer) =
    let probability = generateProbability()
    let randomIndex = getRandomNumber group.Instances.Length
    let failure = generateFailure group probability randomIndex

    match failure with
        | Some f -> GoogleCloud.induce f
        | None -> Log.Logger.Information("No failure induced to group {group} (probability {probability}%)", group.Name, (group.Failure.Probability * 100.)|> int)

    t.Start() // restart timer

let startTimer group =
    Log.Logger.Information ("Created scheduler for group: {group}", group.Name)
    let period = group.Failure.Interval |> toSeconds |> toMiliseconds
    let timer = new System.Timers.Timer(period)
    timer.Elapsed.AddHandler(fun s e -> induceChaos group timer)
    timer.AutoReset <- false
    timer.Start()

let createMonkeys (groups: ChaosGroupConfig list) =
    groups |> List.iter startTimer