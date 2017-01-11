namespace ElKunzo.FantaFootball.Internal

open System
open System.Data
open System.Net.Http
open System.Collections.Generic
open Microsoft.SqlServer.Server

open ElKunzo.FantaFootball
open ElKunzo.FantaFootball.FootballDataOrg
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



    type BaseCacheWithRefreshTimer<'a>(_spName, _mappingFunction, _refreshInterval) =
        let mutable (Data:IReadOnlyList<'a>) = 
            let data = executeReadOnlyStoredProcedureAsync _spName _mappingFunction Array.empty |> Async.RunSynchronously
            match data with
            | Success x -> x
            | Failure x -> failwith (sprintf "Could not load cahce data from DB: %s" x)
        let mutable TimeStampUtc = DateTime.UtcNow

        member this.GetData = 
            if this.IsOutdated() then this.Update()
            Data
        
        member this.IsOutdated () = 
            let difference = DateTime.UtcNow.Subtract(TimeStampUtc)
            difference > _refreshInterval
        
        member this.Update () = 
            let data = 
                let dbResult = executeReadOnlyStoredProcedureAsync _spName _mappingFunction Array.empty |> Async.RunSynchronously
                match dbResult with
                | Success x -> x
                | Failure x -> failwith (sprintf "Could not load cahce data from DB: %s" x)
            Data <- data
            TimeStampUtc <- DateTime.UtcNow

    

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
            return Success output
        with
        | ex -> return Failure ex.Message
    }
    
    

    let persistWhoScoredIdAsync uspName data = async {
        let mapIdTupleToSqlType ids = 
            let metaData = [|
                new SqlMetaData("Id", SqlDbType.Int);
                new SqlMetaData("WhoScoredId", SqlDbType.Int);
            |]
            let record = new SqlDataRecord(metaData)
            ids |> Seq.map (fun id ->
                    record.SetInt32(0, fst id)
                    record.SetInt32(1, snd id)
                    record)

        let sqlParameter = createTableValuedParameter "@WhoScoredIdData" mapIdTupleToSqlType data
        return! executeWriteOnlyStoredProcedureAsync uspName [| sqlParameter |]
    }



    let asyncThrottle maxCountParallel asyncJobSeq =
        seq { 
                let n = new Threading.Semaphore(maxCountParallel, maxCountParallel)
                for f in asyncJobSeq ->
                    async { let! ok = Async.AwaitWaitHandle(n)
                            let! result = Async.Catch f
                            n.Release() |> ignore
                            return match result with
                                    | Choice1Of2 rslt -> rslt
                                    | Choice2Of2 exn  -> raise exn
                        }
            }



    let rnd = System.Random()
    let genRandomNumber () = rnd.Next (500, 5000)