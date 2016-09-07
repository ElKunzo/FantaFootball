namespace ElKunzo.FantaFootball.TestApp

open System
open System.Threading
open FSharp.Configuration

open ElKunzo.FantaFootball
open ElKunzo.FantaFootball.Internal

module TestRunners = 

    type Settings = AppSettings<"App.config">



    let competitionUrl = String.Format(Settings.CompetitionUrlTemplate.OriginalString, Settings.LeagueId)
    let teamUrlTemplate = Settings.PlayerUrlTemplate.OriginalString
    let liveMatchReportUrlTemplate = Settings.LiveMatchReportUrlTemplate



    let GetMatchReportTest () = 
        printf "Retreiving WhoScored.com match report...   "

        for i in 1115140 .. 1115200 do
            Thread.Sleep(2000)
            let matchReport = (MatchReport.downloadDataAsync (liveMatchReportUrlTemplate.ToString()) i) |> Async.RunSynchronously
            match matchReport with 
            | None -> printfn "Could not download match report."
            | Some report -> 
                MatchReport.updatePlayerIdsAsync CacheRepository.PlayerStatic report |> Async.RunSynchronously
                CacheRepository.PlayerStatic.Update()
                let missingPlayers = CacheRepository.PlayerStatic.PublicData |> Seq.filter (fun x -> x.WhoScoredId = -1) |> Seq.length
                printfn "Done with %i: Missing Players %i" i missingPlayers



    let UpdateTeams () = 
        printf "Updating Teams...   "
        TeamStaticData.updateDataAsync CacheRepository.Team competitionUrl |> Async.RunSynchronously
        CacheRepository.Team.Update()
        printfn "Done."



    let UpdatePlayers () =
        printf "Updating Players...   "
        let updatePlayers = 
            CacheRepository.Team.PublicData 
            |> Seq.map (fun t -> async { do! PlayerStaticData.updateDataForTeamAsync teamUrlTemplate CacheRepository.PlayerStatic t })
            |> Async.Parallel
            |> Async.Ignore
        updatePlayers |> Async.RunSynchronously
        CacheRepository.PlayerStatic.Update()
        printfn "Done."



    let UpdateFixtures () = 
        printf "Updating Fixtures...   "
        FixtureData.updateAsync CacheRepository.Team CacheRepository.Fixture competitionUrl |> Async.RunSynchronously
        CacheRepository.Fixture.Update()
        printfn "Done."
