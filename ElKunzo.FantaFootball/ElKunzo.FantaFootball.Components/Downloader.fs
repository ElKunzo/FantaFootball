namespace ElKunzo.FantaFootball.Components

open System
open System.Net.Http
open ElKunzo.FantaFootball.DataTransferObjects.External

module Downloader = 

    let downloadAsync url (clientBuilder:unit -> HttpClient) =
        async {            
            try
                let client = clientBuilder ()
                let uri = new Uri(url)
                let! response = client.GetAsync(uri) |> Async.AwaitTask
                do response.EnsureSuccessStatusCode() |> ignore
                let! output = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return (Some output)
            with
            | ex -> printfn "Something went wrong while downloading: %s" ex.Message; return None
        }

    let buildDefaultHttpClient () = 
        let client = new HttpClient()
        client

    let buildFootballDataApiHttpClient () = 
        let client = new HttpClient()
        client.DefaultRequestHeaders.Add("X-Auth-Token", FootballDataApiKey.key);
        client
        
    let downloadPlayersAsync (t:Team) = 
        let teamId = 
            (t._Links.Self.Href).Split('/') 
            |> Seq.last
            |> int

        async {
            let url = t._Links.Players.Href
            let! result = downloadAsync url buildFootballDataApiHttpClient
            match result with
            | None -> return { t with FootballDataId = teamId }
            | Some data -> 
                let playerCollection = JsonParser.parsePlayerDataJson data 
                let players = playerCollection.Players |> Seq.map (fun p -> { p with FootballDataTeamId = teamId })
                return { t with Players = players; FootballDataId = teamId  }
        }

    let downloadTeamDataAsync baseUrl = 
        async {
            let url = baseUrl + "/teams"
            let! result = downloadAsync url buildFootballDataApiHttpClient
            
            match result with
            | None -> return None 
            | Some x ->
                let comp = JsonParser.parseCompetitionDataJson x
                let teams = Async.Parallel [ for team in comp.Teams -> downloadPlayersAsync team ] 
                            |> Async.RunSynchronously
                let output = { comp with Teams = teams }
                return (Some output)
        }

    let downloadFixtureDataAsync baseUrl = 
        async {
            let url = baseUrl + "/fixtures"
            let! result = downloadAsync url buildFootballDataApiHttpClient

            match result with 
            | None -> return None
            | Some x -> return (Some (JsonParser.parseSeasonFixtures x))
        }

    let downloadWhoScoredMatchReportAsync (baseUrl:string) (id:int) = 
        async {
            let url = String.Format(baseUrl, id)
            let! result = downloadAsync url buildDefaultHttpClient

            match result with 
            | None -> return None
            | Some x -> return (Some (JsonParser.parseMatchReport x))
        }
    

