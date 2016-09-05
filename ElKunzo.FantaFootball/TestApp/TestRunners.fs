namespace ElKunzo.FantaFootball.TestApp

open System.Threading
open FSharp.Configuration
open ElKunzo.FantaFootball.Components
open ElKunzo.FantaFootball.DataTransferObjects
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.DataTransferObjects.External
open ElKunzo.FantaFootball.DataTransferObjects.Internal

module TestRunners = 
    type Settings = AppSettings<"App.config">

    let commandTimeout = Settings.CommandTimeout
    let databaseConnectionString = Settings.ConnectionStrings.FootballData

    
    let StaticDataCacheTest () = 
        let a = StaticDataCache.TeamDataCache
        //let b = StaticDataCache.PlayerDataCache

        while (true) do
            printfn "Iteration done"
            let team = a 185
            match team with | None -> printfn "No team found" | Some x -> printfn "Team with Id %i: %s" 185 x.FullName
            Thread.Sleep(10000)


    let GetMatchReportTest () = 
        printfn "\nRetreiving WhoScored.com match report\n"

        let liveMatchReportUrlTemplate = Settings.LiveMatchReportUrlTemplate
        let matchReport = (Downloader.downloadWhoScoredMatchReportAsync (liveMatchReportUrlTemplate.ToString()) 1115173) |> Async.RunSynchronously
        match matchReport with 
        | None -> printfn "Could not download match report."
        | Some report -> printfn "%s - %s" report.Home.Name report.Away.Name 
            

        0