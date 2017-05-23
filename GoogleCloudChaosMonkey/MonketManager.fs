module MonkeyManager

open System
open MonkeyCore

let getRandomizer = 
    let r = new Random()
    fun () -> r

let generateProbability() = 
    getRandomizer().NextDouble()

let getRandomNumber maxVal = 
    getRandomizer().Next(maxVal)

let induceChaos (group: ChaosGroup) (t: System.Timers.Timer) = 
    let probability = generateProbability()
    let randomIndex = getRandomNumber group.Instances.Length
    let failure = generateFailure group probability randomIndex

    match failure with 
        | Some f -> gcloud.execute f
        | None -> printfn "Due to selected probability, no failure induced to group %s" group.Name 
    
    t.Start() // restart timer

let startTimer (group: ChaosGroup) =
    printfn "Registered timer for group: %s" group.Name
    let period = group.Failure.Interval |> toSeconds |> toMiliseconds |> float   
    let timer = new System.Timers.Timer(period);
    timer.Elapsed.AddHandler(fun s e -> induceChaos group timer)
    timer.AutoReset <- false
    timer.Start()
    
let createMonkeys (groups: ChaosGroup list) = 
    groups |> List.iter startTimer