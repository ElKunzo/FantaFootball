namespace ElKunzo.FantaFootball.Internal

open System
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server

open ElKunzo.FantaFootball.DataAccess

module TeamStaticData = 

    type T = {
        Id : int;
        FootballDataId : int;
        WhoScoredId : int;
        Name : string;
        FullName : string;
        Code : string option;
        SquadMarketValue : int option;
        CrestUrl : string option;
    }



    let mapFromSqlType (dataReader:DbDataReader) = 
        let idOrdinal = dataReader.GetOrdinal("fId")
        let footbalDataIdOrdinal = dataReader.GetOrdinal("fFootballDataId")
        let whoScoredIdOrdinal = dataReader.GetOrdinal("fWhoScoredId")
        let nameOrdinal = dataReader.GetOrdinal("fName")
        let fullNameOrdinal = dataReader.GetOrdinal("fFullName")
        let codeOrdinal = dataReader.GetOrdinal("fCode")
        let squadMarketValueOrdinal = dataReader.GetOrdinal("fSquadMarketValue")
        let crestUrlOrdinal = dataReader.GetOrdinal("fCrestUrl")

        {
            Id = dataReader.GetInt32(idOrdinal);
            FootballDataId = dataReader.GetInt32(footbalDataIdOrdinal);
            WhoScoredId = dataReader.GetInt32(whoScoredIdOrdinal);
            Name = dataReader.GetString(nameOrdinal);
            FullName = dataReader.GetString(fullNameOrdinal);
            Code = if dataReader.IsDBNull(codeOrdinal) then None else Some (dataReader.GetString(codeOrdinal));
            SquadMarketValue = if dataReader.IsDBNull(squadMarketValueOrdinal) then None else Some (dataReader.GetInt32(squadMarketValueOrdinal));
            CrestUrl = if dataReader.IsDBNull(crestUrlOrdinal) then None else Some (dataReader.GetString(crestUrlOrdinal));
        }



    let mapToSqlType (teams:seq<T>) = 
        let metaData = [|
            new SqlMetaData("Id", SqlDbType.Int);
            new SqlMetaData("FootballDataId", SqlDbType.Int);
            new SqlMetaData("WhoScoredId", SqlDbType.Int);
            new SqlMetaData("Name", SqlDbType.NVarChar, 500L);
            new SqlMetaData("FullName", SqlDbType.NVarChar, 500L);
            new SqlMetaData("Code", SqlDbType.NVarChar, 5L);
            new SqlMetaData("SquadMarketValue", SqlDbType.Int);
            new SqlMetaData("CrestUrl", SqlDbType.NVarChar, 500L);
        |]

        let record = new SqlDataRecord(metaData)
        teams |> Seq.map (fun team ->
                record.SetInt32(0, team.Id)
                record.SetInt32(1, team.FootballDataId)
                record.SetInt32(2, team.WhoScoredId)
                record.SetString(3, team.Name)
                record.SetString(4, team.FullName)
                match team.Code with | Some x -> record.SetString(5,x) | None -> record.SetDBNull(5)
                match team.SquadMarketValue with | Some x -> record.SetInt32(6, x) | None -> record.SetDBNull(6)
                match team.CrestUrl with | Some x -> record.SetString(7, x) | None -> record.SetDBNull(7)
                record)



    let Cache = BaseCacheWithRefreshTimer<T>("usp_TeamData_Get", mapFromSqlType, TimeSpan.FromMinutes(60.0));



    let persistAsync data = async {
        let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@TeamData" mapToSqlType data
        return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_TeamData_Update" [| sqlParameter |]
    }