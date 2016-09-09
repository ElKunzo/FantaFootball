namespace ElKunzo.FantaFootball.Internal

open System
open Newtonsoft.Json

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.External.WhoScoredTypes

module MatchReport = 

    let parseMatchReport (jsonString:string) = 
        try
            let a = jsonString.Split([| "var matchCentreData = " |], StringSplitOptions.RemoveEmptyEntries).[1]
            let b = a.Split([| "\"events\":" |], StringSplitOptions.None).[0]
            let matchAndTeamData = b.Remove(b.Length - 1, 1) + "}"
            let result = JsonConvert.DeserializeObject<MatchReport>(matchAndTeamData)
            Success result
        with
        | ex -> Failure ex.Message


    let updateTeamIdsAsync (teamCache:TeamStaticData.Cache) (report:MatchReport) = async {
        let getUpdateableTeam (updateableTeams:seq<TeamStaticData.T>) (teamdata:(int*string)) = 
            let id, name = teamdata
            let known = updateableTeams |> Seq.filter (fun t -> t.Name = name || t.FullName = name)
            match (known |> Seq.length) with
            | 1 -> let t = known |> Seq.head; 
                   if (t.WhoScoredId <> id) then Some (t.Id, id) else None
            | 0 -> None
            | _ -> None


        let missingIdTeams = teamCache.PublicData |> Seq.filter (fun t -> t.WhoScoredId = -1)
        if (missingIdTeams |> Seq.length = 0) then return ()
        let teamIds = [| (report.Home.TeamId, report.Home.Name); (report.Away.TeamId, report.Away.Name) |]
                      |> Array.map (fun x -> getUpdateableTeam missingIdTeams x)
                      |> Array.filter (fun x -> x.IsSome)
                      |> Array.map (fun x -> x.Value)
        let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@WhoScoredIdData" mapIdTupleToSqlType teamIds
        return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_TeamData_UpdateWhoScoredId" [| sqlParameter |]

    }



    let updatePlayerIdsAsync (playerCache:PlayerStaticData.Cache) (report:MatchReport) = async {
        let getUpdateablePlayer (updateablePlayers:seq<PlayerStaticData.T>) id name = 
            let known = updateablePlayers |> Seq.filter (fun p -> p.Name = name)
            match (known |> Seq.length) with
            | 1 -> let p = known |> Seq.head
                   if (p.WhoScoredId <> id) then Some (p.Id, id) else None
            | 0 -> printfn "Player %s not found in FootballData data" name; None
            | _ -> None


        let missingIdPlayers = playerCache.PublicData //|> Seq.filter (fun p -> p.WhoScoredId = -1)
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
            | Failure x -> return Failure x
            | Success x -> return parseMatchReport x
        }