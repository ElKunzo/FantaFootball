namespace ElKunzo.FantaFootball.Internal

open System
open Newtonsoft.Json

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.External.WhoScoredTypes

module MatchReport = 

    let urlTemplate = "https://www.whoscored.com/Matches/{0}/Live";



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



    let mapPlayerToInternal (team:TeamStaticData.T) (externalPlayer:PlayerData) : PlayerStaticData.T = 
        let mapPosition (positionString:string) =
            match positionString with
            | "GK" -> Position.Goalkeeper
            | "DL" | "DC" | "DR" -> Position.Defender
            | "ML" | "MC" | "MR" -> Position.Midfielder
            | "F" | "Forward" -> Position.Forward
            | _ -> Position.Unknown

        {
            Id = -1;
            WhoScoredId = externalPlayer.PlayerId;
            FootballDataTeamId = team.FootballDataId;
            TeamId = team.Id;
            JerseyNumber = Some externalPlayer.ShirtNo;
            Position = (mapPosition externalPlayer.Position);
            Name = externalPlayer.Name;
            FullName = externalPlayer.Name;
            DateOfBirth = DateTime.Now.AddYears(-1 * externalPlayer.Age);
            Nationality = null;
            ContractUntil = None;
            MarketValue = None;
        }



    let updatePlayerIdsAsync (playerCache:PlayerStaticData.Cache) (teamDataCache:TeamStaticData.Cache) (report:MatchReport) = async {
        let compareJerseyNumber internalNo externalNo =
                match internalNo with
                | None -> false
                | Some x -> x = externalNo

        let wasPlayerOnPitch (externalPlayer:PlayerData) = 
            if (externalPlayer.IsFirstEleven) then true
            else 
               match box externalPlayer.SubbedInPeriod with
               | null -> false
               | _ -> true

        let getInternalPlayer teamId (availablePlayers:seq<PlayerStaticData.T>) (externalPlayer:PlayerData) = 
            let processed = availablePlayers |> Seq.tryFind (fun p -> p.WhoScoredId = externalPlayer.PlayerId)
            match processed with
            | Some x -> None
            | None -> match (availablePlayers |> Seq.tryFind (fun p -> p.Name = externalPlayer.Name || p.FullName = externalPlayer.Name)) with
                      | Some x -> Some (x.Id, externalPlayer.PlayerId)
                      | None -> match (availablePlayers |> Seq.tryFind (fun p -> p.TeamId = teamId && compareJerseyNumber p.JerseyNumber externalPlayer.ShirtNo)) with
                                | None -> Some (-1, externalPlayer.PlayerId)
                                | Some x -> Some (x.Id, externalPlayer.PlayerId)

        let getUpdateablePlayerIds teamId (externalPlayers:seq<PlayerData>) = async {
            let team = teamDataCache.PublicData |> Seq.find (fun x -> x.WhoScoredId = teamId)
            let updateablePlayers = playerCache.PublicData //|> Seq.filter (fun p -> p.TeamId = team.Id)
            let result = 
                externalPlayers 
                |> Seq.filter (fun externalPlayer -> wasPlayerOnPitch externalPlayer)
                |> Seq.map (fun externalPlayer -> externalPlayer |> getInternalPlayer team.Id updateablePlayers)
            let intermediateResult = result |> Seq.filter (fun x -> x.IsSome) |> Seq.map (fun x -> x.Value) |> Seq.toArray
            let knownUpdateablePlayers = intermediateResult |> Seq.filter (fun (x, _) -> x <> -1)
            let remainingUnknownPlayers = intermediateResult 
                                          |> Seq.filter (fun (x, _) -> x = -1)
                                          |> Seq.map (fun (_, id) -> externalPlayers |> Seq.find (fun x -> x.PlayerId = id) |> mapPlayerToInternal team)
            do! PlayerStaticData.persistAsync "usp_PlayerStaticData_InsertWhoScored" (Success remainingUnknownPlayers) |> Async.Ignore
            return knownUpdateablePlayers |> Seq.toArray
        }

        let! homeIds =  report.Home.Players |> getUpdateablePlayerIds report.Home.TeamId
        let! awayIds =  report.Away.Players |> getUpdateablePlayerIds report.Away.TeamId
        let ids = (Array.concat [| homeIds; awayIds |])
        match (Array.length ids) with
        | 0 -> return Success ()
        | _ -> let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@WhoScoredIdData" mapIdTupleToSqlType ids
               return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_PlayerStaticData_UpdateWhoScoredId" [| sqlParameter |]
    }
    
    

    let downloadDataAsync (id:int) = 
        async {
            let url = String.Format(urlTemplate, id)
            let! result = downloadAsync url buildDefaultHttpClient

            match result with 
            | Failure x -> return Failure x
            | Success x -> let matchReport = parseMatchReport x
                           match matchReport with
                           | Failure y -> return Failure y
                           | Success y -> return Success ({ y with WhoScoredId = id })
        }