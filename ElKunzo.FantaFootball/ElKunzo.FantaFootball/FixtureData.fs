namespace ElKunzo.FantaFootball.Internal

open System
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server
open Newtonsoft.Json

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.External.FootballDataTypes

module FixtureData = 

    type T = {
        Id : int;
        WhoScoredId : int;
        FootballDataId : int;
        Status: FixtureStatus;
        KickOff : DateTime;
        MatchDay : int;
        HomeTeamId : int;
        AwayTeamId : int;
        HomeScore : int option;
        AwayScore : int option;
    }



    type Cache (spName, mappingFunction, refreshInterval) =
        inherit BaseCacheWithRefreshTimer<T>(spName, mappingFunction, refreshInterval)

        override this.TryGetItem (id) = 
            if this.IsOutdated() then this.Update()
            this.PublicData |> Seq.tryFind (fun t -> t.Id = id)



    let mapFromSqlType (dataReader:DbDataReader) = 
        let idOrdinal = dataReader.GetOrdinal("fId")
        let whoScoredIdOrdinal = dataReader.GetOrdinal("fWhoScoredId")
        let footbalDataIdOrdinal = dataReader.GetOrdinal("fFootballDataId")
        let statusOrdinal = dataReader.GetOrdinal("frStatusId")
        let kickOffTimeOrdinal = dataReader.GetOrdinal("fKickOffUtc")
        let matchDayOrdinal = dataReader.GetOrdinal("fMatchDay")
        let homeTeamIdOrdinal = dataReader.GetOrdinal("frHomeTeamId")
        let awayTeamIdOrdinal = dataReader.GetOrdinal("frAwayTeamId")
        let homeTeamScoreOrdinal = dataReader.GetOrdinal("fHomeScore")
        let awayTeamScoreOrdinal = dataReader.GetOrdinal("fAwayScore")

        {
            Id = dataReader.GetInt32(idOrdinal);
            WhoScoredId = dataReader.GetInt32(whoScoredIdOrdinal);
            FootballDataId = dataReader.GetInt32(footbalDataIdOrdinal);
            Status =  enum<FixtureStatus> (dataReader.GetInt32(statusOrdinal));
            KickOff = dataReader.GetDateTime(kickOffTimeOrdinal);
            MatchDay = dataReader.GetInt32(matchDayOrdinal);
            HomeTeamId = dataReader.GetInt32(homeTeamIdOrdinal);
            AwayTeamId = dataReader.GetInt32(awayTeamIdOrdinal);
            HomeScore = if dataReader.IsDBNull(homeTeamScoreOrdinal) then None else Some (dataReader.GetInt32(homeTeamScoreOrdinal));
            AwayScore = if dataReader.IsDBNull(awayTeamScoreOrdinal) then None else Some (dataReader.GetInt32(awayTeamScoreOrdinal));
        }



    let mapToSqlType (fixtures:seq<T>) = 
        let metaData = [|
            new SqlMetaData("Id", SqlDbType.Int);
            new SqlMetaData("WhoScoredId", SqlDbType.Int);
            new SqlMetaData("FootballDataId", SqlDbType.Int);
            new SqlMetaData("StatusId", SqlDbType.Int);
            new SqlMetaData("KickOffUtc", SqlDbType.DateTime);
            new SqlMetaData("MatchDay", SqlDbType.Int);
            new SqlMetaData("HomeTeamId", SqlDbType.Int);
            new SqlMetaData("AwayTeamId", SqlDbType.Int);
            new SqlMetaData("HomeScore", SqlDbType.Int);
            new SqlMetaData("AwayScore", SqlDbType.Int);
        |]

        let record = new SqlDataRecord(metaData)
        fixtures |> Seq.map (fun fixture ->
                record.SetInt32(0, fixture.Id)
                record.SetInt32(1, fixture.WhoScoredId)
                record.SetInt32(2, fixture.FootballDataId)
                record.SetInt32(3, int fixture.Status)
                record.SetDateTime(4, fixture.KickOff)
                record.SetInt32(5, fixture.MatchDay)
                record.SetInt32(6, fixture.HomeTeamId)
                record.SetInt32(7, fixture.AwayTeamId)
                match fixture.HomeScore with | Some x -> record.SetInt32(8, x) | None -> record.SetDBNull(8)
                match fixture.AwayScore with | Some x -> record.SetInt32(9, x) | None -> record.SetDBNull(9)
                record)



    let mapFromExternal (teamDataCache:TeamStaticData.Cache) (fixtureDataCache:Cache) (fixture:Fixture) = 
        let mapTeamId (stringUrl:string) =
            let footballDataId = stringUrl.Split('/') |> Seq.last |> int
            let teamOption = teamDataCache.PublicData |> Seq.tryFind(fun t -> t.FootballDataId = footballDataId)
            match teamOption with
            | None -> failwith "Could not find participant team(s) in cache."
            | Some x -> x.Id

        let mapStatus s = 
            match s with
            | "SCHEDULED" -> FixtureStatus.Scheduled
            | "TIMED" -> FixtureStatus.Timed
            | "IN_PLAY" -> FixtureStatus.InPlay
            | "FINISHED" -> FixtureStatus.Finished
            | "POSTPONED" -> FixtureStatus.Postponed
            | "CANCELED" -> FixtureStatus.Canceled
            | _ -> failwith "Could not identify fixture stauts."

        let footballDataFixtureId = 
            fixture._Links.Self.Href.Split('/') |> Seq.last |> int

        let mapScoreFromNullString scoreString = 
            let s = mapNullString scoreString
            match s with
            | None -> None
            | Some x -> Some (int x)

        let known = fixtureDataCache.PublicData |> Seq.tryFind (fun t -> t.FootballDataId = footballDataFixtureId)

        {
            Id = match known with | None -> -1 | Some x -> x.Id;
            WhoScoredId = -1;
            FootballDataId = footballDataFixtureId ;
            Status = (mapStatus fixture.Status);
            KickOff = fixture.Date;
            MatchDay = fixture.MatchDay;
            HomeTeamId = mapTeamId fixture._Links.HomeTeam.Href;
            AwayTeamId = mapTeamId fixture._Links.AwayTeam.Href;
            HomeScore = mapScoreFromNullString fixture.Result.GoalsHomeTeam;
            AwayScore = mapScoreFromNullString fixture.Result.GoalsAwayTeam;
        }



    let downloadDataAsync baseUrl = async {
        let url = baseUrl + "/fixtures"
        let! result = downloadAsync url buildFootballDataApiHttpClient

        match result with 
            | Failure x -> return Failure x
            | Success x -> return Success (JsonConvert.DeserializeObject<SeasonFixtures>(x))
    }



    let updateAsync teamCache fixtureCache competitionUrl = async {
        let! externalFixtures = downloadDataAsync competitionUrl

        match externalFixtures with
        | Failure x -> return Failure x
        | Success x -> let internalFixtures = x.Fixtures |> Seq.map (fun f -> mapFromExternal teamCache fixtureCache f)
                       let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@FixtureData" mapToSqlType internalFixtures
                       return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_FixtureData_Update" [| sqlParameter |]
    }

