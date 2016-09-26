namespace ElKunzo.FantaFootball.WhoScoredCom

open System
open Newtonsoft.Json

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.Internal
open ElKunzo.FantaFootball.External.WhoScoredTypes

module DataHarvester = 

    let urlTemplate = "https://www.whoscored.com/Matches/{0}/Live";
    let preMatchMatchDataUrlTemplate = "https://www.whoscored.com/Matches/{0}";


    let parseMatchReport (jsonString:string) = 
        try
            let a = jsonString.Split([| "var matchCentreData = " |], StringSplitOptions.RemoveEmptyEntries).[1]
            let b = (a.Split([| "var matchCentreEventTypeJson =" |], StringSplitOptions.None).[0]).Trim()
            let matchAndTeamData = b.Remove(b.Length - 1, 1)
            let result = JsonConvert.DeserializeObject<MatchReport>(matchAndTeamData)
            Success result
        with
        | ex -> Failure ex.Message



    let downloadMatchReportAsync (id:int) = async {
        let url = String.Format(urlTemplate, id)
        let! result = downloadAsync url buildDefaultHttpClient

        match result with 
        | Failure x -> return Failure x
        | Success x -> let matchReport = parseMatchReport x
                       match matchReport with
                       | Failure y -> return Failure y
                       | Success y -> return Success ({ y with WhoScoredId = id })
    }



    let downloadMatchReportCollectionAsync (ids:seq<int>) = async {
        return! ids 
                |> Seq.toList
                |> List.map downloadMatchReportAsync
                |> Common.asyncThrottle 4
                |> Async.Parallel
    }
        



    let downloadPreMatchDataAsync (startingMatchId:int) (knwonIds:seq<int>) = async {
        let downloader (id:int) = async {
                let url = String.Format(preMatchMatchDataUrlTemplate, id)
                let! result = downloadAsync url buildDefaultHttpClient
                return (id, result)
            }
        
        let possibleIds = new ResizeArray<int>(seq { for i in startingMatchId .. (startingMatchId + 379) do yield i })
        for i in knwonIds do possibleIds.Remove i |> ignore


        return! possibleIds 
                |> Seq.toList
                |> List.map downloader
                |> Common.asyncThrottle 4
                |> Async.Parallel
    }