namespace ElKunzo.FantaFootball.Internal

open System
open Newtonsoft.Json

open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.External.WhoScoredTypes

module MatchReport = 

    let parseMatchReport (jsonString:string) = 
        let a = jsonString.Split([| "var matchCentreData = " |], StringSplitOptions.RemoveEmptyEntries).[1]
        let b = a.Split([| "\"events\":" |], StringSplitOptions.None).[0]
        let matchAndTeamData = b.Remove(b.Length - 1, 1) + "}"
        let result = JsonConvert.DeserializeObject<MatchReport>(matchAndTeamData)
        result



    let getUpdateablePlayer (updateablePlayers:seq<PlayerStaticData.T>) id name = 
        let known = updateablePlayers |> Seq.filter (fun p -> p.Name = name)
        match (known |> Seq.length) with
        | 1 -> let p = known |> Seq.head
               if (p.WhoScoredId <> id) then Some (p.Id, id) else None
        | 0 -> None
        | _ -> None



    let updatePlayerIdsAsync (playerCache:PlayerStaticData.Cache) (report:MatchReport) = async {
        let missingIdPlayers = playerCache.PublicData |> Seq.filter (fun p -> p.WhoScoredId = -1)
        let ids =  report.PlayerIdNameDictionary 
                    |> Map.toArray
                    |> Array.map (fun (id, name) -> getUpdateablePlayer missingIdPlayers id name)
                    |> Array.filter (fun x -> x.IsSome)
                    |> Array.map (fun x -> x.Value)
        let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@WhoScoredIdData" mapIdTupleToSqlType ids
        return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_PlayerStaticData_UpdateWhoScoredId" [| sqlParameter |]
    }

    
    
    let downloadDataAsync (baseUrl:string) (id:int) = 
        async {
            let url = String.Format(baseUrl, id)
            let! result = downloadAsync url buildDefaultHttpClient

            match result with 
            | None -> return None
            | Some x -> return Some (parseMatchReport x)
        }