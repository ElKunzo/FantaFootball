namespace ElKunzo.FantaFootball.WhoScoredCom

open System

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.Internal

module Processor = 

    let processPreMatchFixturesAsync startId = async {
        let knownIds = FixtureData.Cache.GetData |> Seq.filter (fun f -> f.WhoScoredId <> -1) |> Seq.map (fun f -> f.WhoScoredId)
        let calendarData = DataHarvester.downloadPreMatchDataAsync startId knownIds
                           |> Async.RunSynchronously
                           |> Seq.filter (fun (id, op) -> isSuccess op)
                           |> Seq.map (fun (id, op) -> (id, (getSuccessValue op).Value))
                           |> Seq.map (fun (id, op) -> Mapper.parseCalendarData id op)

        let fixtureIdUpdate = calendarData
                                |> Seq.map (fun c -> Mapper.mapFixtureId c)
                                |> Seq.filter (fun op -> isSuccess op)
                                |> Seq.map (fun op -> (getSuccessValue op).Value)
                                |> Seq.concat
                                |> Common.persistWhoScoredIdAsync "usp_FixtureData_UpdateWhoScoredId"
                                |> Async.RunSynchronously
        
        FixtureData.Cache.Update ()
        TeamStaticData.Cache.Update ()

        let teamIdUpdate = calendarData
                                |> Seq.map (fun c -> Mapper.mapTeamIds c)
                                |> Seq.filter (fun op -> isSuccess op)
                                |> Seq.map (fun op -> (getSuccessValue op).Value)
                                |> Seq.concat
                                |> Common.persistWhoScoredIdAsync "usp_TeamData_UpdateWhoScoredId"
                                |> Async.RunSynchronously

        return (fixtureIdUpdate, teamIdUpdate)
    }



    let processMatchReportAsync id = async {
        let! data = DataHarvester.downloadMatchReportAsync id

        match data with
        | Failure x -> return Failure x
        | Success x -> 

            let! playerUpdate = async {
                match (Mapper.mapPlayerIds x) with
                | Failure y -> return Failure y
                | Success y -> do! y |> snd |> PlayerStaticData.persistWhoScoredAsync |> Async.Ignore
                               do! y |> fst |> Common.persistWhoScoredIdAsync "usp_PlayerStaticData_UpdateWhoScoredId" |> Async.Ignore
                               return Success "Done"
            }

            FixtureData.Cache.Update ()
            TeamStaticData.Cache.Update ()
            PlayerStaticData.Cache.Update ()
            PlayerScoreData.Cache.Update ()

            let! scoreUpdate = async {
                match (Mapper.mapMatchReport x) with
                | Failure x -> return Failure x
                | Success x -> do! PlayerScoreData.persistAsync x |> Async.Ignore
                               return Success "Done"
            }
            printfn "Done Fixture %s - %s" x.Home.Name x.Away.Name
            return Success (playerUpdate, scoreUpdate)
    }



    let processMatchReportCollectionAsync ids = async {
        return! ids 
                |> Seq.toList
                |> List.map processMatchReportAsync
                |> Common.asyncThrottle 1
                |> Async.Parallel
    }