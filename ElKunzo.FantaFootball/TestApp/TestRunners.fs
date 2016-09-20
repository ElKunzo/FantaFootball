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



    let UpdateTeamWhoScoredIds team = 
        match team with
        | Failure x -> Failure x
        | Success x -> CacheRepository.Team.Update()
                       let missingTeams = CacheRepository.Team.PublicData |> Seq.filter (fun x -> x.WhoScoredId = -1) |> Seq.length
                       Success (sprintf "Done with Team Update: Missing Teams %i" missingTeams)



    let UpdatePlayerWhoScoredIds players = 
        match players with
        | Failure x -> Failure x
        | Success x -> CacheRepository.PlayerStatic.Update()
                       let missingPlayers = CacheRepository.PlayerStatic.PublicData |> Seq.filter (fun x -> x.WhoScoredId = -1) |> Seq.length
                       Success (sprintf "Done with Player Update: Missing Players %i" missingPlayers)



    let UpdateTeamAndPlayerWhoScoredIds () = 
        printfn "Retreiving WhoScored.com match report...   "
        let playedFixtureIds = CacheRepository.Fixture.PublicData |> Seq.filter (fun x -> x.Status = FixtureStatus.Finished) |> Seq.map (fun x -> x.WhoScoredId)

        for i in playedFixtureIds do
            Thread.Sleep(5000)
            let matchReport = (MatchReport.downloadDataAsync (liveMatchReportUrlTemplate.ToString()) i) |> Async.RunSynchronously
            match matchReport with 
            | Failure x -> printfn "Could not download match report: %s" x
            | Success x -> 
                let team = UpdateTeamWhoScoredIds (MatchReport.updateTeamIdsAsync CacheRepository.Team x |> Async.RunSynchronously)
                let players = UpdatePlayerWhoScoredIds (MatchReport.updatePlayerIdsAsync CacheRepository.PlayerStatic CacheRepository.Team x |> Async.RunSynchronously)
                match team, players with
                | Success _, Success _ -> printfn "\tDone updating team and players"
                | Success _, Failure x -> printfn "\tTeam update Ok. Players not ok: %s" x
                | Failure x, Success _ -> printfn "\tPlayer update Ok. Team not ok: %s" x
                | Failure x, Failure y -> printfn "\tTeam update not Ok: %s\n\tPlayer update not Ok: %s" x y



    let UpdateFixtureWhoScoredIds () = 
        printfn "Updating Fixture Data with WhoScored Ids...   "
        let startingId = 1115149
        match (WhoScoredCalendarData.updateWhoScoredFixtureIdsAsync startingId CacheRepository.Fixture CacheRepository.Team |> Async.RunSynchronously) with
        | Failure x -> printfn "\tCould not update fixture data: %s" x
        | Success x -> CacheRepository.Fixture.Update()
                       printfn "\tDone updating fixture data."



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