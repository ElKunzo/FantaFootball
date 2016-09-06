namespace ElKunzo.FantaFootball.Internal

open System
open System.Net.Http
open System.Collections.Generic

open ElKunzo.FantaFootball
open ElKunzo.FantaFootball.DataAccess.DatabaseDataAccess

[<AutoOpen>]
module Common = 

    type Position = 
        | Goalkeeper = 1 
        | Defender = 2 
        | Midfielder = 3 
        | Forward = 4 
        | Unknown = 5



    type FixtureStatus = 
        | Scheduled = 1 
        | Timed = 2 
        | InPlay = 3 
        | Finished = 4 
        | Postponed = 5 
        | Canceled = 6 
        | Unknown = 7



    [<AbstractClass>]
    type BaseCacheWithRefreshTimer<'a>(_spName, _mappingFunction, _refreshInterval) =
        let RefreshInterval = _refreshInterval
        let SpName = _spName
        let MappingFunction = _mappingFunction
        let mutable (Data:IReadOnlyList<'a>) = executeReadOnlyStoredProcedureAsync SpName MappingFunction Array.empty |> Async.RunSynchronously
        let mutable TimeStampUtc = DateTime.UtcNow

        member this.PublicData = 
            Data
        
        member this.IsOutdated () = 
            let difference = DateTime.UtcNow.Subtract(TimeStampUtc)
            difference > RefreshInterval
        
        member this.Update () = 
            Data <- executeReadOnlyStoredProcedureAsync SpName MappingFunction Array.empty |> Async.RunSynchronously
            TimeStampUtc <- DateTime.UtcNow
        
        abstract member TryGetItem : int -> 'a option



    let mapMarketValue (marketValueAsString:string) = 
        match marketValueAsString with
        | null -> None
        | _ -> let result = marketValueAsString.Split(' ').[0].Replace(",", "")
               Some (int result)



    let mapNullString input = 
        match input with
        | null -> None
        | _ -> Some input



    let buildDefaultHttpClient () = 
        let client = new HttpClient()
        client



    let buildFootballDataApiHttpClient () = 
        let client = new HttpClient()
        client.DefaultRequestHeaders.Add("X-Auth-Token", FootballDataApiKey.KEY)
        client



    let downloadAsync url (clientBuilder:unit -> HttpClient) = async {            
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