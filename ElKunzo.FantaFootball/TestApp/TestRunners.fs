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
        printfn "Retreiving WhoScored.com match report...   "

        for i in 1006150 .. 1006200 do
            Thread.Sleep(10000)
            let matchReport = (MatchReport.downloadDataAsync (liveMatchReportUrlTemplate.ToString()) i) |> Async.RunSynchronously
            match matchReport with 
            | Failure x -> printfn "Could not download match report: %s" x
            | Success x -> 
                match (MatchReport.updatePlayerIdsAsync CacheRepository.PlayerStatic x |> Async.RunSynchronously) with
                | Failure x -> printfn "Could not download match report for %i: %s" i x
                | Success x -> CacheRepository.PlayerStatic.Update()
                               let missingPlayers = CacheRepository.PlayerStatic.PublicData |> Seq.filter (fun x -> x.WhoScoredId = -1) |> Seq.length
                               printfn "Done with %i: Missing Players %i" i missingPlayers



    let UpdateTeams () = 
        printfn "Updating Teams...   "
        match (TeamStaticData.updateDataAsync CacheRepository.Team competitionUrl |> Async.RunSynchronously) with
        | Failure x -> printfn "\tCould not update team data: %s" x
        | Success x -> CacheRepository.Team.Update()
                       printfn "\tDone updating team data."



    let UpdatePlayers () =
        printfn "Updating Players...   "
        let result = CacheRepository.Team.PublicData 
                        |> Seq.map (fun t -> async { return! PlayerStaticData.updateDataForTeamAsync teamUrlTemplate CacheRepository.PlayerStatic t } |> Async.RunSynchronously)
                        |> Seq.toArray
        match (result |> Array.forall (fun res -> isSuccess res)) with
        | false -> printfn "\tCould not update player data."
        | true -> CacheRepository.PlayerStatic.Update()
                  printfn "\tDone updating player data."
            



    let UpdateFixtures () = 
        printfn "Updating Fixtures...   "
        match (FixtureData.updateAsync CacheRepository.Team CacheRepository.Fixture competitionUrl |> Async.RunSynchronously) with
        | Failure x -> printfn "\tCould not update fixture data: %s" x
        | Success x -> CacheRepository.Fixture.Update()
                       printfn "\tDone updating fixture data."