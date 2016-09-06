namespace ElKunzo.FantaFootball.Internal

open System
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server

open ElKunzo.FantaFootball.External.FootballDataTypes

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
        DateOfBirth : DateTime option;
        Nationality : string;
        ContractUntil : DateTime option;
        MarketValue : int option;
    }



    type Cache (spName, mappingFunction, refreshInterval) =
        inherit BaseCacheWithRefreshTimer<T>(spName, mappingFunction, refreshInterval)

        override this.TryGetItem (id) = 
            if this.IsOutdated() then this.Update()
            this.PublicData |> Seq.tryFind (fun p -> p.Id = id)



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
            DateOfBirth = if dataReader.IsDBNull(dateOfBirthOrdinal) then None else Some (dataReader.GetDateTime(dateOfBirthOrdinal));
            Nationality = dataReader.GetString(nationalityOrdinal);
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
                match player.DateOfBirth with | Some x -> record.SetDateTime(8, x) | None -> record.SetDBNull(8)
                record.SetString(9, player.Nationality)
                match player.ContractUntil with | Some x -> record.SetDateTime(10, x) | None -> record.SetDBNull(10)
                match player.MarketValue with | Some x -> record.SetInt32(11, x) | None -> record.SetDBNull(11)
                record)



    let mapFromExternal teamId (extPlayer:Player) = 
        let mapPosition positionAsString = 
            printfn "%s" positionAsString
            match positionAsString with
            | "Keeper" -> Position.Goalkeeper
            | "Right-Back" | "Left-Back" | "Centre Back" -> Position.Defender
            | "Defensive Midfield" | "Central Midfield" | "Attacking Midfield" -> Position.Midfielder
            | "Left Midfield" | "Right Midfield" | "Left Wing" | "Right Wing" -> Position.Midfielder
            | "Centre Forward" | "Secondary Striker" -> Position.Forward
            | _ -> Position.Unknown

        let mapJerseyNumber numberAsString =
            let opt = (mapNullString numberAsString)
            match opt with
            | None -> None
            | Some x -> Some (int x)

        let mapContractUntil contractUntilAsString = 
            let opt = (mapNullString contractUntilAsString)
            match opt with
            | None -> None
            | Some (x:string) -> let data = x.Split('-') |> Array.map (fun a -> int a)
                                 Some (DateTime(data.[0], data.[1], data.[2]))

        {
            Id = -1;
            WhoScoredId = -1;
            FootballDataTeamId = extPlayer.FootballDataTeamId;
            TeamId = teamId;
            JerseyNumber = (mapJerseyNumber extPlayer.JerseyNumber);
            Position = (mapPosition extPlayer.Position);
            Name = extPlayer.Name;
            FullName = extPlayer.Name;
            DateOfBirth = if extPlayer.DateOfBirth = DateTime.MinValue then None else Some extPlayer.DateOfBirth;
            Nationality = extPlayer.Nationality
            ContractUntil = mapContractUntil extPlayer.ContractUntil;
            MarketValue = (mapMarketValue extPlayer.MarketValue);
        }

