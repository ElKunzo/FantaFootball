namespace ElKunzo.FantaFootball.TestApp

open System
open ElKunzo.FantaFootball.Components
open ElKunzo.FantaFootball.DataTransferObjects
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.DataTransferObjects.External
open ElKunzo.FantaFootball.DataTransferObjects.Internal

module TestRunners = 
    let GetCompetitionTest () = 
        printfn "Reading Teams form internet:\n"

        let competition = (Downloader.downloadTeamDataAsync 1) 
                        |> Async.RunSynchronously
        match competition with 
        | None -> printfn "Could not download team data."
        | Some comp -> 
            comp.Teams
            |> Seq.toList
            |> List.map (fun team -> printfn "%A" team)
            |> ignore

        0



    let DatabaseIOTest () = 
        printfn "\nWriting dummy data into DB\n"

        let dummyData = [|
                { Id = 1; ExternalId = 1; Name = "Dummy"; FullName = "Dummy 1"};
                { Id = 2; ExternalId = 2; Name = "Dummy"; FullName = "Dummy 2"};
                { Id = 3; ExternalId = 3; Name = "Dummy"; FullName = "Dummy 3"};
            |]
        let spParameters = [|
                (DatabaseDataAccess.createTableValuedParameter "@TeamData" dummyData Mapper.mapTeamStaticDataToSql)
            |]

        let result = (DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync 
                        Constants.connectionString 
                        "usp_TeamData_Update"
                        (Some Constants.commandTimeout)
                        spParameters
                     ) |> Async.RunSynchronously 

        printfn "\nReading Teams from Database:\n"

        let result = (DatabaseDataAccess.executeReadOnlyStoredProcedureAsync 
                        Constants.connectionString 
                        "usp_TeamData_Get"
                        (Some Constants.commandTimeout)
                        Mapper.mapTeamStaticDataFromSql
                        Array.empty) 
                    |> Async.RunSynchronously 

        result
        |> Seq.toList 
        |> List.map (fun team -> printfn "%A" team )
        |> ignore

        0

    let GetMatchReportTest () = 
        printfn "\nRetreiving WhoScored.com match report\n"

        let matchReport = (Downloader.downloadWhoScoredMatchReportAsync 1115173) |> Async.RunSynchronously
        match matchReport with 
        | None -> printfn "Could not download match report."
        | Some report -> printfn "%s - %s" report.Home.Name report.Away.Name 
            

        0