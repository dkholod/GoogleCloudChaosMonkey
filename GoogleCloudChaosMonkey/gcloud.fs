module gcloud

open MonkeyCore

let execute (f: InstantFailure) =
    printfn "%A" f