namespace ElKunzo.FantaFootball.Components

open System
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server
open ElKunzo.FantaFootball.DataTransferObjects.Internal
//open ElKunzo.FantaFootball.DataTransferObjects.External

module Mapper = 

    let mapTeamStaticDataFromSql (dataReader:DbDataReader) = 
        let idOrdinal = dataReader.GetOrdinal("fId")
        let externalIdOrdinal = dataReader.GetOrdinal("fExternalId")
        let nameOrdinal = dataReader.GetOrdinal("fName")
        let fullNameOrdinal = dataReader.GetOrdinal("fFullName")

        {
            Id = dataReader.GetInt32(idOrdinal);
            ExternalId = dataReader.GetInt32(externalIdOrdinal);
            Name = dataReader.GetString(nameOrdinal);
            FullName = dataReader.GetString(fullNameOrdinal);
        }

    let mapTeamStaticDataToSql (teams:seq<TeamStaticData>) = 
        let metaData = [|
            new SqlMetaData("fExternalId", SqlDbType.Int);
            new SqlMetaData("fName", SqlDbType.NVarChar, 500L);
            new SqlMetaData("fFullName", SqlDbType.NVarChar, 500L);
        |]

        let record = new SqlDataRecord(metaData)
        teams |> Seq.map (fun team ->
                record.SetInt32(0, team.ExternalId)
                record.SetString(1, team.Name)
                if (String.IsNullOrWhiteSpace(team.FullName)) then record.SetDBNull(2) else record.SetString(2, team.FullName)
                record)