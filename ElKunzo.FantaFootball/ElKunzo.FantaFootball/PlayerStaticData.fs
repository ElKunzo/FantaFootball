namespace ElKunzo.FantaFootball.Internal

open System
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server

open ElKunzo.FantaFootball.DataAccess

module PlayerStaticData = 

    type T = {
        Id : int;
        WhoScoredId : int;
        FootballDataTeamId : int;
        TeamId : int;
        JerseyNumber : int option;
        Position : Position;
        Name : string;
        FullName : string;
        DateOfBirth : DateTime;
        Nationality : string;
        ContractUntil : DateTime option;
        MarketValue : int option;
    }



    let mapFromSqlType (dataReader:DbDataReader) = 
        let idOrdinal = dataReader.GetOrdinal("fId")
        let whoScoredIdOrdinal = dataReader.GetOrdinal("fWhoScoredId")
        let footballDataTeamIdOrdinal = dataReader.GetOrdinal("fFootballDataTeamId")
        let teamIdOrdinal = dataReader.GetOrdinal("frTeamId")
        let jerseyNumberOrdinal = dataReader.GetOrdinal("fJerseyNumber")
        let positionOrdinal = dataReader.GetOrdinal("frPosition")
        let nameOrdinal = dataReader.GetOrdinal("fName")
        let fullNameOrdinal = dataReader.GetOrdinal("fFullName")
        let dateOfBirthOrdinal = dataReader.GetOrdinal("fDateOfBirth")
        let nationalityOrdinal = dataReader.GetOrdinal("fNationality")
        let contractUntilOrdinal = dataReader.GetOrdinal("fContractUntil")
        let marketValueOrdinal = dataReader.GetOrdinal("fMarketValue")
        
        {
            Id = dataReader.GetInt32(idOrdinal);
            WhoScoredId = dataReader.GetInt32(whoScoredIdOrdinal);
            FootballDataTeamId = dataReader.GetInt32(footballDataTeamIdOrdinal);
            TeamId = dataReader.GetInt32(teamIdOrdinal);
            JerseyNumber = if dataReader.IsDBNull(jerseyNumberOrdinal) then None else Some (dataReader.GetInt32(jerseyNumberOrdinal));
            Position = enum<Position> (dataReader.GetInt32(positionOrdinal));
            Name = dataReader.GetString(nameOrdinal);
            FullName = dataReader.GetString(fullNameOrdinal);
            DateOfBirth = dataReader.GetDateTime(dateOfBirthOrdinal);
            Nationality = if dataReader.IsDBNull(nationalityOrdinal) then "" else dataReader.GetString(nationalityOrdinal);
            ContractUntil = if dataReader.IsDBNull(contractUntilOrdinal) then None else Some (dataReader.GetDateTime(contractUntilOrdinal));
            MarketValue = if dataReader.IsDBNull(marketValueOrdinal) then None else Some (dataReader.GetInt32(marketValueOrdinal));
        }



    let mapToSqlType (players:seq<T>) = 
        let metaData = [|
            new SqlMetaData("Id", SqlDbType.Int);
            new SqlMetaData("WhoScoredId", SqlDbType.Int);
            new SqlMetaData("FootbalDataTeamId", SqlDbType.Int);
            new SqlMetaData("TeamId", SqlDbType.Int);
            new SqlMetaData("JerseyNumber", SqlDbType.Int);
            new SqlMetaData("Position", SqlDbType.Int);
            new SqlMetaData("Name", SqlDbType.NVarChar, 500L);
            new SqlMetaData("FullName", SqlDbType.NVarChar, 500L);
            new SqlMetaData("DateOfBirth", SqlDbType.DateTime);
            new SqlMetaData("Nationality", SqlDbType.NVarChar, 50L);
            new SqlMetaData("ContractUntil", SqlDbType.DateTime);
            new SqlMetaData("MarketValue", SqlDbType.Int);
        |]

        let record = new SqlDataRecord(metaData)
        players |> Seq.map (fun player ->
                record.SetInt32(0, player.Id)
                record.SetInt32(1, player.WhoScoredId)
                record.SetInt32(2, player.FootballDataTeamId)
                record.SetInt32(3, player.TeamId)
                match player.JerseyNumber with | Some x -> record.SetInt32(4, x) | None -> record.SetDBNull(4)
                record.SetInt32(5, int player.Position)
                record.SetString(6, player.Name)
                record.SetString(7, player.FullName)
                record.SetDateTime(8, player.DateOfBirth)
                match player.Nationality with | null -> record.SetDBNull(9) | _ -> record.SetString(9, player.Nationality)
                match player.ContractUntil with | Some x -> record.SetDateTime(10, x) | None -> record.SetDBNull(10)
                match player.MarketValue with | Some x -> record.SetInt32(11, x) | None -> record.SetDBNull(11)
                record)



    let Cache = BaseCacheWithRefreshTimer<T>("usp_PlayerStaticData_Get", mapFromSqlType, TimeSpan.FromMinutes(60.0))


    
    let persistAsync data = async {
        let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@PlayerData" mapToSqlType data
        return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_PlayerStaticData_Update" [| sqlParameter |]
    }



    let persistWhoScoredAsync data = async {
        let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@PlayerData" mapToSqlType data
        return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_PlayerStaticData_InsertWhoScored" [| sqlParameter |]
    }



    
