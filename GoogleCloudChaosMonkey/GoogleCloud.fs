[<RequireQualifiedAccess>]
module GoogleCloud

open System
open System.Threading
open System.Threading.Tasks
open MonkeyCore
open Google.Apis.Services
open Google.Apis.Auth.OAuth2
open Google.Apis.Compute.v1
open Serilog

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

let private resetInstance f =
    let request = getComputeService().Instances.Reset(f.ProjectId, f.Zone, f.Instance)
    let response = request.Execute()
    Log.Logger.Information ("Instance {instance} was {action}", f.Instance, "Reseted")
    // todo - check response for errors
    ()

let private stopAndStartInstance f =
    let stopRequest = getComputeService().Instances.Stop(f.ProjectId, f.Zone, f.Instance)
    let response = stopRequest.Execute()
    Log.Logger.Information ("Instance {instance} was {action}", f.Instance, "Stopped")
    
    Thread.Sleep(TimeSpan.FromSeconds(3.))
    let startRequest = getComputeService().Instances.Start(f.ProjectId, f.Zone, f.Instance)
    let response' = startRequest.Execute()
    Log.Logger.Information ("Instance {instance} was {action}", f.Instance, "Started")
    // todo - check response for errors
    ()

let induce (f: Failure) =
    let action = match f.Strategy with
                    | Reset -> resetInstance
                    | StopStart -> stopAndStartInstance
                    | NoAction -> fun _ -> ()
    try
        action f
    with
    | :? Google.GoogleApiException as ex -> Log.Logger.Error (ex, "Error during execution of {strategy} on {instance}", f.Strategy, f.Instance)