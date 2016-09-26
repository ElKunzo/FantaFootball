namespace ElKunzo.FantaFootball.FootballDataOrg

open System
open Newtonsoft.Json

open ElKunzo.FantaFootball.Internal
open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.FootballDataOrg.FootballDataTypes

module DataHarvester = 

    let competitionUrl = "http://api.football-data.org/v1/competitions/438"
    let playerUrlTemplate = "http://api.football-data.org/v1/teams/{0}/players"

    let downloadTeamStaticDataAsync baseUrl = async {
        let url = baseUrl + "/teams"
        let! result = downloadAsync url buildFootballDataApiHttpClient
            
        match result with 
            | Failure x -> return Failure x
            | Success x -> 
                try
                    let comp = JsonConvert.DeserializeObject<Competition>(x)
                    return Success comp.Teams
                with
                | ex -> return Failure ex.Message
    }



    let downloadPlayerDataForTeamAsync url = async {
        let! result = downloadAsync url buildFootballDataApiHttpClient
        match result with 
        | Failure x -> return Failure x
        | Success x -> let playerCollection = JsonConvert.DeserializeObject<PlayerCollection>(x)
                       let footballDataTeamId = playerCollection._Links.Team.Href.Split('/') |> Seq.last |> int
                       let playerCollectionWithTeamId = playerCollection.Players |> Seq.map (fun p -> { p with FootballDataTeamId = footballDataTeamId })
                       return Success playerCollectionWithTeamId
    }



    let downloadFixtureDataAsync baseUrl = async {
        let url = baseUrl + "/fixtures"
        let! result = downloadAsync url buildFootballDataApiHttpClient

        match result with 
            | Failure x -> return Failure x
            | Success x -> return Success (JsonConvert.DeserializeObject<SeasonFixtures>(x))
    }

