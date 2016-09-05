namespace ElKunzo.FantaFootball.Components

open System;
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.DataTransferObjects.External

module StaticDataCache = 
    type CacheWithRefreshTimer<'a> = {
            mutable Data : seq<'a>;
            mutable TimeStampUtc : DateTime;
            mutable RefreshIntervalInMinutes : int;
        } with
        member x.IsOutdated () = 
            let now = DateTime.UtcNow
            let difference = now.Subtract(x.TimeStampUtc)
            difference.TotalMinutes > (float x.RefreshIntervalInMinutes)

    let commandTimeout = 10
    let databaseConnectionString = "Server=tcp:localhost,1433;Integrated Security=SSPI;Database=ElKunzoFantaFootball;Timeout=15;Max Pool Size=500"

    let executeReadSpAsync spName mappingFunction = 
        printfn "DB interaction!"
        async {
            return! DatabaseDataAccess.executeReadOnlyStoredProcedureAsync databaseConnectionString spName (Some commandTimeout) mappingFunction Array.empty
        }

    let executeWriteSpAsync spName parameters = 
        async {
            return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync databaseConnectionString spName (Some commandTimeout) parameters
        }

    
    let TeamDataCache =
        let cache = { Data = executeReadSpAsync "usp_TeamData_Get" Mapper.mapTeamStaticDataFromSql |> Async.RunSynchronously;
                      TimeStampUtc = DateTime.UtcNow;
                      RefreshIntervalInMinutes = 1;
                    }
        fun teamId -> 
            if cache.IsOutdated() then
                cache.Data <- executeReadSpAsync "usp_TeamData_Get" Mapper.mapTeamStaticDataFromSql |> Async.RunSynchronously
                cache.TimeStampUtc <- DateTime.UtcNow
            cache.Data |> Seq.tryFind (fun t -> t.Id = teamId)


//    let PlayerDataCache =
//        let cache = { Data = executeReadSpAsync "usp_PlayerData_Get" Mapper.mapPlayerStaticDataFromSql |> Async.RunSynchronously;
//                      TimeStampUtc = DateTime.UtcNow;
//                      RefreshIntervalInMinutes = 1;
//                    }
//        fun playerId -> 
//            let currentCache = 
//                if cache.IsOutdated then
//                    { cache with Data = executeReadSpAsync "usp_PlayerData_Get" Mapper.mapPlayerStaticDataFromSql |> Async.RunSynchronously; TimeStampUtc = DateTime.UtcNow; }
//                else
//                    cache
//            currentCache.Data |> Seq.tryFind (fun t -> t.Id = playerId)
    


//    let DoDatabaseInteraction (competition:Competition) = 
//
//        
//
//        printfn "Updating team data in DB."
//        let teamSpParameter = competition.Teams
//                                |> Seq.map( fun team -> Mapper.mapExternalTeamStaticDataToInternal team)
//                                |> Seq.map( fun team -> 
//                                                let known = knownTeams |> Seq.tryFind(fun t -> t.FootballDataId = team.FootballDataId)
//                                                match known with
//                                                | None -> team
//                                                | Some x -> { team with Id = x.Id }
//                                          )
//                                |> DatabaseDataAccess.createTableValuedParameter "@TeamData" Mapper.mapTeamStaticDataToSql
//
//        do executeWriteSp "usp_TeamData_Update" [| teamSpParameter |]
//
//        //get the new Teams
//        let knownTeams = executeReadSp "usp_TeamData_Get" Mapper.mapTeamStaticDataFromSql
//
//        printfn "Reading Teams from Database."
//        let knownPlayers = executeReadSp "usp_PlayerStaticData_Get" Mapper.mapPlayerStaticDataFromSql
//
//        printfn "Updating Players."
//        let players = knownTeams |> Seq.map(fun team -> 
//                                let externalTeam = competition.Teams |> Seq.tryFind (fun t -> t.FootballDataId = team.FootballDataId)
//                                match externalTeam with
//                                | None -> Seq.empty
//                                | Some t -> t.Players |> Seq.map (fun p -> Mapper.mapExternalPlayerStaticDataToInternal team.Id p)) 
//                            |> Seq.concat
//        let playerSpParameter = 
//            players 
//            |> Seq.map (fun extPlayer ->
//                            let knownPlayer = knownPlayers |> Seq.tryFind (fun p -> p.Name = extPlayer.Name && p.DateOfBirth = extPlayer.DateOfBirth)
//                            match knownPlayer with
//                            | None -> extPlayer
//                            | Some x -> { extPlayer with Id = x.Id }
//                        )
//            |> DatabaseDataAccess.createTableValuedParameter "@PlayerData" Mapper.mapPlayerStaticDataToSql
//
//        do executeWriteSp "usp_PlayerStaticData_Update" [| playerSpParameter |]
//
//        0


