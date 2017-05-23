module gcloud

open MonkeyCore
open Google.Apis.Services
open Google.Apis.Auth.OAuth2
open System.Threading.Tasks
open Google.Apis.Compute.v1

let private getCredential() = 
    let credential = Task.Run<GoogleCredential>(fun _ -> GoogleCredential.GetApplicationDefaultAsync()).Result;
    if credential.IsCreateScopedRequired then
        credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform")
    else 
        credential

let private getComputeService = 
    let initializer = new BaseClientService.Initializer()
    initializer.HttpClientInitializer <- getCredential()
    initializer.ApplicationName <- "Google-ComputeSample/0.1"
    let computeSrv = new ComputeService(initializer)
    fun () -> computeSrv

let execute (f: InstantFailure) =
    try
        let stopRequest = getComputeService().Instances.Stop(f.ProjectId, f.Zone, f.Instance)
        let r1 = stopRequest.Execute()
        System.Threading.Thread.Sleep(f.Delay |> toSeconds |> toMiliseconds |> int32)
        //let startRequest = getComputeService().Instances.Start(f.ProjectId, f.Zone, f.Instance)
        //let r2 = startRequest.Execute()
        ()
    with
    | :? Google.GoogleApiException as ex ->  printfn "%s" ex.Message