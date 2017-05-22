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

let induceChaos (group: ChaosGroup) (state:Object) = 
    let probability = generateProbability()
    let randomIndex = getRandomNumber group.Instances.Length
    let failure = generateFailure group probability randomIndex

    match failure with 
        | Some f -> gcloud.execute f
        | None -> printfn "Due to selected probability, no failure induced to group %s" group.Name 
    ()

let startTimer (group: ChaosGroup) =
    printfn "%s" group.Name
    let period = group.Failure.Interval |> toSeconds |> toMiliseconds
    new Threading.Timer(induceChaos group, null, period, period)
    |> ignore

let createMonkeys (groups: ChaosGroup list) = 
    groups |> List.iter startTimer