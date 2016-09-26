namespace ElKunzo.FantaFootball.FootballDataOrg

open System

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.Internal
open ElKunzo.FantaFootball.FootballDataOrg.FootballDataTypes

module Processor = 

    let competitionUrl = "http://api.football-data.org/v1/competitions/438"
    let playerUrlTemplate = "http://api.football-data.org/v1/teams/{0}/players"



    let processTeamsAsync = async {
        return! DataHarvester.downloadTeamStaticDataAsync competitionUrl
                |> Async.RunSynchronously
                |> Mapper.mapTeamStaticDataCollection
                |> Seq.filter (fun op -> isSuccess op)
                |> Seq.map (fun op -> getSuccessValue op) 
                |> Seq.map (fun team -> team.Value)
                |> TeamStaticData.persistAsync
    }



    let processPlayersAsync = async {
        return! TeamStaticData.Cache.GetData
                |> Seq.map (fun t -> t.FootballDataId)
                |> Seq.map (fun id -> String.Format(playerUrlTemplate, id))
                |> Seq.map DataHarvester.downloadPlayerDataForTeamAsync
                |> Seq.toList
                |> Common.asyncThrottle 4
                |> Async.Parallel
                |> Async.RunSynchronously
                |> Seq.map (fun p -> Mapper.mapPlayerStaticDataCollection p)
                |> Seq.concat
                |> Seq.filter (fun op -> isSuccess op)
                |> Seq.map (fun op -> getSuccessValue op) 
                |> Seq.map (fun p -> p.Value)
                |> PlayerStaticData.persistAsync
    }



    let processFixturesAsync = async {
        let! data = DataHarvester.downloadFixtureDataAsync competitionUrl

        match data with 
        | Failure x -> return Failure x
        | Success x -> return! x.Fixtures 
                               |> Seq.map Mapper.mapFixtureData
                               |> Seq.filter (fun op -> isSuccess op)
                               |> Seq.map (fun op -> getSuccessValue op) 
                               |> Seq.map (fun p -> p.Value)
                               |> FixtureData.persistAsync
    }