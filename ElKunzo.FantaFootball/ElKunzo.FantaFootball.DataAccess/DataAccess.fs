namespace ElKunzo.FantaFootball.DataAccess

open System
open System.Collections.Generic
open System.Data
open System.Data.Common
open System.Data.SqlClient
open Microsoft.SqlServer.Server

module DatabaseDataAccess = 

    let defaultCommandTimeoutInSeconds = 10

    let createSqlCommand storedProcedureName connection commandTimeout parameters =
        let command = new SqlCommand(storedProcedureName, connection)
        command.CommandType <- System.Data.CommandType.StoredProcedure

        match parameters, commandTimeout with
        | null, None -> command.CommandTimeout <- defaultCommandTimeoutInSeconds
        | null, (Some x) -> command.CommandTimeout <- x
        | _, None -> command.Parameters.AddRange(parameters); 
                     command.CommandTimeout <- defaultCommandTimeoutInSeconds
        | _, (Some x) -> command.Parameters.AddRange(parameters); 
                         command.CommandTimeout <- x
        
        command

    let createTableValuedParameter parameterName item (sqlDataRecordsCreator:'a -> seq<SqlDataRecord>) =
        let result = 
            match item with
            | null -> new SqlParameter(parameterName, null)
            | _ ->new SqlParameter(parameterName, (sqlDataRecordsCreator item ))
        
        result.SqlDbType <- SqlDbType.Structured
        result

    
    let rec readRowsAsync (reader:DbDataReader) (results:ResizeArray<_>) mappingFunction = 
        async {
            let! readAsyncResult = Async.AwaitTask (reader.ReadAsync ())
            if readAsyncResult then
                results.Add (mappingFunction reader)
                return! readRowsAsync reader results mappingFunction
            else
                return results.AsReadOnly() :> IReadOnlyList<_>
        }
    
    let executeReadOnlyStoredProcedureAsync connectionString storedProcedureName commandTimeout readOperation parameters =
        try
            async {
                use connection = new SqlConnection(connectionString)
                use command = createSqlCommand storedProcedureName connection commandTimeout parameters
                do! connection.OpenAsync() |> Async.AwaitTask |> Async.Ignore
                use! dataReader = command.ExecuteReaderAsync() |> Async.AwaitTask
                let! result = (readRowsAsync dataReader (ResizeArray()) readOperation)
                // finally NextResult has to be called, to ensure that all pending exceptions are thrown
                do! dataReader.NextResultAsync() |> Async.AwaitTask |> Async.Ignore
                return result
            }
        with
        | ex -> printfn "Something went wrong reading from the DB (%s)" ex.Message; reraise ()

    let executeWriteOnlyStoredProcedureAsync connectionString storedProcedureName commandTimeout parameters =
        try        
            async {
                use connection = new SqlConnection(connectionString)
                use command = createSqlCommand storedProcedureName connection commandTimeout parameters
                do! connection.OpenAsync() |> Async.AwaitTask |> Async.Ignore
                use transaction = connection.BeginTransaction()
                try
                    command.Transaction <- transaction;
                    let! affectedRows = command.ExecuteNonQueryAsync() |> Async.AwaitTask
                    transaction.Commit()
                with
                | ex -> try
                            transaction.Rollback()
                        with
                        | ex -> printfn "Could not roll back the transaction. (%s)" ex.Message
            }
        with
        | ex -> printfn "Something went wrong writing to the DB. (%s)" ex.Message; reraise ()