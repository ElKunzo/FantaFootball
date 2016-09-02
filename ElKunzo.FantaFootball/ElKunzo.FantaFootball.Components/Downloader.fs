namespace ElKunzo.FantaFootball.Components

open System
open System.Net.Http
open ElKunzo.FantaFootball.DataTransferObjects.External


module Downloader = 

    let downloadAsync url =
        async {            
            try
                let client = new HttpClient()
                let uri = new Uri(url)
                let! response = client.GetAsync(uri) |> Async.AwaitTask
                do response.EnsureSuccessStatusCode() |> ignore
                let! output = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return (Some output)
            with
            | ex -> printfn "Something went wrong while downloading: %s" ex.Message; return None
        }
        
    let downloadPlayersAsync (t:Team) = 
        async {
            let url = t._Links.Players.Href
            let! result = downloadAsync(url)
            match result with
            | None -> return t
            | Some data -> 
                let playerCollection = JsonParser.parsePlayerDataJson data
                return { t with Players = playerCollection.Players }
        }

    let downloadTeamDataAsync (seasonId:int) = 
        async {
            let url = Constants.competitionUrlTemplate + "/teams"
            let! result = downloadAsync url
            
            match result with
            | None -> return None 
            | Some x ->
                let comp = JsonParser.parseCompetitionDataJson x
                let teams = Async.Parallel [ for team in comp.Teams -> downloadPlayersAsync team ] 
                            |> Async.RunSynchronously
                let output = { comp with Teams = teams }
                return (Some output)
        }

    let downloadFixtureDataAsync (seasonId:int) = 
        async {
            let url = Constants.competitionUrlTemplate + "/fixtures"
            let! result = downloadAsync url 

            match result with 
            | None -> return None
            | Some x -> return (Some (JsonParser.parseSeasonFixtures x))
        }

    let downloadWhoScoredMatchReportAsync (id:int) = 
        async {
            let url = String.Format(Constants.matchReportUrlTemplate, id)
            let! result = downloadAsync url

            match result with 
            | None -> return None
            | Some x -> return (Some (JsonParser.parseMatchReport x))
        }
    

