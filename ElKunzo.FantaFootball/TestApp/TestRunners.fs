﻿namespace ElKunzo.FantaFootball.TestApp

open System.Threading

open ElKunzo.FantaFootball
open ElKunzo.FantaFootball.Internal

module TestRunners = 

    let UpdatePlayerScoreData id = async {
        let! matchReport = MatchReport.downloadDataAsync id
        match matchReport with 
        | Failure x -> printfn "Could not download match report: %s" x
        | Success x -> do! MatchReport.updateTeamIdsAsync CacheRepository.Team x |> Async.Ignore
                       CacheRepository.Team.Update()
                       do! MatchReport.updatePlayerIdsAsync CacheRepository.PlayerStatic CacheRepository.Team x |> Async.Ignore
                       CacheRepository.PlayerStatic.Update()
                       do! PlayerScoreData.getDataForMatchReportAsync CacheRepository.PlayerScore CacheRepository.PlayerStatic CacheRepository.Fixture x |> Async.Ignore
                       CacheRepository.PlayerScore.Update()
                       printfn "\tDone game update for FixtureId %i: %s - %s" id x.Home.Name x.Away.Name
    }



    let UpdateTeamAndPlayerWhoScoredIds () = 
        printfn "Retreiving WhoScored.com match report...   "
        let playedFixtureIds = CacheRepository.Fixture.PublicData |> Seq.filter (fun x -> x.Status = FixtureStatus.Finished) |> Seq.map (fun x -> x.WhoScoredId)

        playedFixtureIds 
        |> Seq.toList
        |> List.map UpdatePlayerScoreData
        |> Common.asyncThrottle 4
        |> Async.Parallel
        |> Async.RunSynchronously



    let UpdateFixtureWhoScoredIds () = 
        printfn "Updating Fixture Data with WhoScored Ids...   "
        let startingId = 1115149
        match (WhoScoredCalendarData.updateWhoScoredFixtureIdsAsync startingId CacheRepository.Fixture CacheRepository.Team |> Async.RunSynchronously) with
        | Failure x -> printfn "\tCould not update fixture data: %s" x
        | Success x -> CacheRepository.Fixture.Update()
                       printfn "\tDone updating fixture data."



    let UpdateTeams () = 
        printfn "Updating Teams...   "
        match (TeamStaticData.updateDataAsync CacheRepository.Team |> Async.RunSynchronously) with
        | Failure x -> printfn "\tCould not update team data: %s" x
        | Success x -> CacheRepository.Team.Update()
                       printfn "\tDone updating team data."



    let UpdatePlayers () =
        printfn "Updating Players...   "
        let result = CacheRepository.Team.PublicData 
                        |> Seq.map (fun t -> async { return! PlayerStaticData.updateDataForTeamAsync CacheRepository.PlayerStatic t } |> Async.RunSynchronously)
                        |> Seq.toArray
        match (result |> Array.forall (fun res -> isSuccess res)) with
        | false -> printfn "\tCould not update player data."
        | true -> CacheRepository.PlayerStatic.Update()
                  printfn "\tDone updating player data."
            


    let UpdateFixtures () = 
        printfn "Updating Fixtures...   "
        match (FixtureData.updateAsync CacheRepository.Team CacheRepository.Fixture |> Async.RunSynchronously) with
        | Failure x -> printfn "\tCould not update fixture data: %s" x
        | Success x -> CacheRepository.Fixture.Update()
                       printfn "\tDone updating fixture data."
        
        
        
