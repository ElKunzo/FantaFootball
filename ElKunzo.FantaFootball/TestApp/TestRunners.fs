namespace ElKunzo.FantaFootball.TestApp

open System
open System.Threading
open FSharp.Configuration
open ElKunzo.FantaFootball.Components
open ElKunzo.FantaFootball.DataTransferObjects.Internal

module TestRunners = 
    type Settings = AppSettings<"App.config">

    let StaticDataCacheTest () = 
        let teams = StaticDataCache.TeamDataCache("usp_TeamData_Get", Mapper.mapTeamStaticDataFromSql, TimeSpan.FromMinutes(0.3))
        let players = StaticDataCache.PlayerStaticDataCache("usp_PlayerStaticData_Get", Mapper.mapPlayerStaticDataFromSql, TimeSpan.FromMinutes(0.3))

        for i in 1 .. 40 do
            printfn "Iteration %i done" i
            let team = teams.TryGetItem 185
            let player = players.TryGetItem 4138
            match team with | None -> printfn "No team found" | Some x -> printfn "Team with Id %i: %s" 185 x.FullName
            match player with | None -> printfn "No player found" | Some x -> printfn "Player with Id %i: %s" 4138 x.FullName
            Thread.Sleep(2000)

    let GetMatchReportTest () = 
        printfn "\nRetreiving WhoScored.com match report\n"

        let liveMatchReportUrlTemplate = Settings.LiveMatchReportUrlTemplate
        let matchReport = (Downloader.downloadWhoScoredMatchReportAsync (liveMatchReportUrlTemplate.ToString()) 1115173) |> Async.RunSynchronously
        match matchReport with 
        | None -> printfn "Could not download match report."
        | Some report -> printfn "%s - %s" report.Home.Name report.Away.Name 