namespace ElKunzo.FantaFootball.DataAccess

open System.Collections.Generic
open System.Data
open System.Data.Common
open System.Data.SqlClient
open Microsoft.SqlServer.Server

open ElKunzo.FantaFootball.DomainTypes

module DatabaseDataAccess = 

    let defaultCommandTimeoutInSeconds = 10



    let databaseConnectionString = "Server=tcp:localhost,1433;Integrated Security=SSPI;Database=ElKunzoFantasyFootball;Timeout=15;Max Pool Size=500"



    let createSqlCommand storedProcedureName connection parameters =
        let command = new SqlCommand(storedProcedureName, connection)
        command.CommandType <- System.Data.CommandType.StoredProcedure
        command.CommandTimeout <- defaultCommandTimeoutInSeconds
        match parameters with
        | null -> ()
        | _ -> command.Parameters.AddRange(parameters)
        command



    let createTableValuedParameter parameterName (sqlDataRecordsCreator:seq<'a> -> seq<SqlDataRecord>) (item:seq<'a>) =
        let result = 
            match item with
            | null -> new SqlParameter(parameterName, null)
            | _ when Seq.isEmpty item -> new SqlParameter(parameterName, null)
            | _ ->new SqlParameter(parameterName, (sqlDataRecordsCreator item ))
        result.SqlDbType <- SqlDbType.Structured
        result

    

    let rec readRowsAsync (reader:DbDataReader) (results:ResizeArray<_>) mappingFunction = async {
        let! readAsyncResult = Async.AwaitTask (reader.ReadAsync ())
        if readAsyncResult then
            results.Add (mappingFunction reader)
            return! readRowsAsync reader results mappingFunction
        else
            return results.AsReadOnly() :> IReadOnlyList<_>
    }
    


    let executeReadOnlyStoredProcedureAsync storedProcedureName readOperation parameters =
        async {
            try
                use connection = new SqlConnection(databaseConnectionString)
                use command = createSqlCommand storedProcedureName connection parameters
                do! connection.OpenAsync() |> Async.AwaitTask |> Async.Ignore
                use! dataReader = command.ExecuteReaderAsync() |> Async.AwaitTask
                let! result = (readRowsAsync dataReader (ResizeArray()) readOperation)
                // finally NextResult has to be called, to ensure that all pending exceptions are thrown
                do! dataReader.NextResultAsync() |> Async.AwaitTask |> Async.Ignore
                return Success result
            with
            | ex -> return Failure ex.Message
        }



    let executeWriteOnlyStoredProcedureAsync storedProcedureName parameters =      
        async {
            try
                use connection = new SqlConnection(databaseConnectionString)
                use command = createSqlCommand storedProcedureName connection parameters
                do! connection.OpenAsync() |> Async.AwaitTask
                use transaction = connection.BeginTransaction()
                try
                    command.Transaction <- transaction;
                    let! affectedRows = command.ExecuteNonQueryAsync() |> Async.AwaitTask
                    transaction.Commit()
                    return Success ()
                with
                | ex -> transaction.Rollback()
                        return Failure ex.Message
            with
            | ex -> return Failure ex.Message
        }