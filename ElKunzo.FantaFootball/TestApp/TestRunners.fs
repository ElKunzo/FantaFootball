namespace ElKunzo.FantaFootball.TestApp

open System
open System.Threading
open FSharp.Configuration

open ElKunzo.FantaFootball.Internal

module TestRunners = 

    type Settings = AppSettings<"App.config">



    let StaticDataCacheTest () = 
        let teams = TeamStaticData.Cache("usp_TeamData_Get", TeamStaticData.mapFromSqlType, TimeSpan.FromMinutes(0.2))
        let players = PlayerStaticData.Cache("usp_PlayerStaticData_Get", PlayerStaticData.mapFromSqlType, TimeSpan.FromMinutes(0.2))

        for i in 1 .. 5 do
            printf "Cache Iteration %i... " i
            let team = teams.TryGetItem 185
            let player = players.TryGetItem 4139
            match team with | None -> printf "No team found, " | Some x -> printf "Team %i: %s, " 185 x.FullName
            match player with | None -> printfn "No player found " | Some x -> printfn "Player %i: %s " 4139 x.FullName
            Thread.Sleep(2000)



    let FixtureDataTest () = 
        printf "Downloading Fixture Data...   "
        let teamCache = TeamStaticData.Cache("usp_TeamData_Get", TeamStaticData.mapFromSqlType, TimeSpan.FromMinutes(0.3))
        let url = String.Format(Settings.CompetitionUrlTemplate.OriginalString, Settings.LeagueId)
        let fixtureData = FixtureData.downloadDataAsync teamCache url |> Async.RunSynchronously

        printfn "Found %i Fixtures" (match fixtureData with | None -> 0 | Some x -> (x |> Seq.length))



    let GetMatchReportTest () = 
        printf "Retreiving WhoScored.com match report...   "

        let liveMatchReportUrlTemplate = Settings.LiveMatchReportUrlTemplate
        let matchReport = (MatchReport.downloadDataAsync (liveMatchReportUrlTemplate.ToString()) 1115173) |> Async.RunSynchronously
        match matchReport with 
        | None -> printfn "Could not download match report."
        | Some report -> printfn "%s - %s" report.Home.Name report.Away.Name 