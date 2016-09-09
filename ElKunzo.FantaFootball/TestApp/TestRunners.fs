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

        for i in 1115155 .. 1115156 do
            Thread.Sleep(5000)
            let matchReport = (MatchReport.downloadDataAsync (liveMatchReportUrlTemplate.ToString()) i) |> Async.RunSynchronously
            match matchReport with 
            | Failure x -> printfn "Could not download match report: %s" x
            | Success x -> 
                let team = (MatchReport.updateTeamIdsAsync CacheRepository.Team x |> Async.RunSynchronously)
                let players = (MatchReport.updatePlayerIdsAsync CacheRepository.PlayerStatic x |> Async.RunSynchronously)
                
                match team with
                | Failure x -> printfn "Could not download match report for %i: %s" i x
//                | Success x -> CacheRepository.PlayerStatic.Update()
//                               let missingPlayers = CacheRepository.PlayerStatic.PublicData |> Seq.filter (fun x -> x.WhoScoredId = -1) |> Seq.length
//                               printfn "Done with %i: Missing Players %i" i missingPlayers
                | Success x -> CacheRepository.Team.Update()
                               let missingTeams = CacheRepository.Team.PublicData |> Seq.filter (fun x -> x.WhoScoredId = -1) |> Seq.length
                               printfn "Done with %i: Missing Teams %i" i missingTeams



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



    let TestArrayThings temp =
        let totalTimer = System.Diagnostics.Stopwatch.StartNew()
        let timer = System.Diagnostics.Stopwatch()
        let maxPoints = 160
        let stepsInMatch = 160
        let probs = [|0.80; 0.14; 0.02; 0.02; 0.02|]

        timer.Start()
        let mutable result = [| for s in 0..stepsInMatch do yield [| for i in 0..maxPoints do yield [| for j in 0..maxPoints do yield [| for k in 1..2 do yield 0.0 |] |] |] |]
        timer.Stop()
        let creationTime = timer.ElapsedMilliseconds
        timer.Reset()
        timer.Start()
        result.[0].[0].[0].[0] <- 1.0
        for s in 1..stepsInMatch do
            for i in 0..(maxPoints-3) do
                for j in 0..(maxPoints-3) do
                    for k in 0..1 do
                        let stateProb = result.[s-1].[i].[j].[k]
                        if not (stateProb = 0.0) then
                            if k = 0 then
                                result.[s].[i].[j].[0] <- result.[s].[i].[j].[0] + stateProb*probs.[0] * temp
                                result.[s].[i].[j].[1] <- result.[s].[i].[j].[1] + stateProb*probs.[1]
                                result.[s].[i+1].[j].[1] <- result.[s].[i+1].[j].[1] + stateProb*probs.[2]
                                result.[s].[i+2].[j].[1] <- result.[s].[i+2].[j].[1] + stateProb*probs.[3]
                                result.[s].[i+3].[j].[1] <- result.[s].[i+3].[j].[1] + stateProb*probs.[4]
                            if k = 1 then
                                result.[s].[i].[j].[1] <- result.[s].[i].[j].[1] + stateProb*probs.[0]
                                result.[s].[i].[j].[0] <- result.[s].[i].[j].[0] + stateProb*probs.[1]
                                result.[s].[i].[j+1].[0] <- result.[s].[i].[j+1].[0] + stateProb*probs.[2]
                                result.[s].[i].[j+2].[0] <- result.[s].[i].[j+2].[0] + stateProb*probs.[3]
                                result.[s].[i].[j+3].[0] <- result.[s].[i].[j+3].[0] + stateProb*probs.[4]

        timer.Stop()
        let calculationTime = timer.ElapsedMilliseconds
        totalTimer.Stop()

        printfn "Creation Time: %i" creationTime
        printfn "Calculation Time: %i" calculationTime
        printfn "Total Time: %i" totalTimer.ElapsedMilliseconds

        result
