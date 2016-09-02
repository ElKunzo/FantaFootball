namespace ElKunzo.FantaFootball.TestApp

open System
open FSharp.Configuration
open ElKunzo.FantaFootball.Components
open ElKunzo.FantaFootball.DataTransferObjects
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.DataTransferObjects.External
open ElKunzo.FantaFootball.DataTransferObjects.Internal

module TestRunners = 
    type Settings = AppSettings<"App.config">

    let GetCompetitionTest () = 
        printfn "Reading Teams form internet:\n"

        let leagueId = Settings.LeagueId
        let competitionUrlTemplate = Settings.CompetitionUrlTemplate
        let baseUrl = String.Format(competitionUrlTemplate.ToString(), leagueId)

        let competition = (Downloader.downloadTeamDataAsync baseUrl) |> Async.RunSynchronously
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

        let commandTimeout = Settings.CommandTimeout
        let databaseConnectionString = Settings.ConnectionStrings.FootballData

        let dummyData = [|
                { Id = 1; ExternalId = 1; Name = "Dummy"; FullName = "Dummy 1"};
                { Id = 2; ExternalId = 2; Name = "Dummy"; FullName = "Dummy 2"};
                { Id = 3; ExternalId = 3; Name = "Dummy"; FullName = "Dummy 3"};
            |]
        let spParameters = [|
                (DatabaseDataAccess.createTableValuedParameter "@TeamData" dummyData Mapper.mapTeamStaticDataToSql)
            |]

        let result = (DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync 
                        databaseConnectionString 
                        "usp_TeamData_Update"
                        (Some commandTimeout)
                        spParameters
                     ) |> Async.RunSynchronously 

        printfn "\nReading Teams from Database:\n"

        let result = (DatabaseDataAccess.executeReadOnlyStoredProcedureAsync 
                        databaseConnectionString 
                        "usp_TeamData_Get"
                        (Some commandTimeout)
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

        let liveMatchReportUrlTemplate = Settings.LiveMatchReportUrlTemplate
        let matchReport = (Downloader.downloadWhoScoredMatchReportAsync (liveMatchReportUrlTemplate.ToString()) 1115173) |> Async.RunSynchronously
        match matchReport with 
        | None -> printfn "Could not download match report."
        | Some report -> printfn "%s - %s" report.Home.Name report.Away.Name 
            

        0