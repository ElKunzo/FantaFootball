namespace ElKunzo.FantaFootball.Internal

open System
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server

open ElKunzo.FantaFootball
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.External.FootballDataTypes
open ElKunzo.FantaFootball.External.WhoScoredTypes

module PlayerScoreData = 

    type T = {
        Id : int;
        FixtureId : int;
        PlayerId : int;
        MinutesPlayed : int;
        GoalsScored : int;
        Assists : int;
        CleanSheet : bool;
        ShotsSaved : int;
        PenaltiesSaved : int;
        PenaltiesMissed : int;
        GoalsConceded : int;
        YellowCards : int;
        RedCard : int;
        OwnGoals : int;
    }

    type Cache (spName, mappingFunction, refreshInterval) =
        inherit BaseCacheWithRefreshTimer<T>(spName, mappingFunction, refreshInterval)

        override this.TryGetItem (id) = 
            if this.IsOutdated() then this.Update()
            this.PublicData |> Seq.tryFind (fun p -> p.Id = id)



    let mapFromSqlType (dataReader:DbDataReader) = 
        let idOrdinal = dataReader.GetOrdinal("fId")
        let fixtureIdOrdinal = dataReader.GetOrdinal("frFixtureId")
        let playerIdOrdinal = dataReader.GetOrdinal("frPlayerId")
        let minutesPlayedOrdinal = dataReader.GetOrdinal("fMinutesPlayed")
        let goalsScoredOrdinal = dataReader.GetOrdinal("fGoalsScored")
        let assistsOrdinal = dataReader.GetOrdinal("fAssists")
        let cleanSheetOrdinal = dataReader.GetOrdinal("fCleanSheet")
        let shotsSavedOrdinal = dataReader.GetOrdinal("fShotsSaved")
        let penaltiesSavedOrdinal = dataReader.GetOrdinal("fPenaltiesSaved")
        let penaltiesMissedOrdinal = dataReader.GetOrdinal("fPenaltiesMissed")
        let goalsConcededOrdinal = dataReader.GetOrdinal("fGoalsConceded")
        let yellowCardsOrdinal = dataReader.GetOrdinal("fYellowCards")
        let redCardsOrdinal = dataReader.GetOrdinal("fRedCards")
        let ownGoalsOrdinal = dataReader.GetOrdinal("fOwnGoals")
        
        {
            Id = dataReader.GetInt32(idOrdinal);
            FixtureId = dataReader.GetInt32(fixtureIdOrdinal);
            PlayerId = dataReader.GetInt32(playerIdOrdinal);
            MinutesPlayed = dataReader.GetInt32(minutesPlayedOrdinal);
            GoalsScored = dataReader.GetInt32(goalsScoredOrdinal);
            Assists = dataReader.GetInt32(assistsOrdinal);
            CleanSheet = dataReader.GetBoolean(cleanSheetOrdinal);
            ShotsSaved = dataReader.GetInt32(shotsSavedOrdinal);
            PenaltiesSaved = dataReader.GetInt32(penaltiesSavedOrdinal);
            PenaltiesMissed = dataReader.GetInt32(penaltiesMissedOrdinal);
            GoalsConceded = dataReader.GetInt32(goalsConcededOrdinal);
            YellowCards = dataReader.GetInt32(yellowCardsOrdinal);
            RedCard = dataReader.GetInt32(redCardsOrdinal);
            OwnGoals = dataReader.GetInt32(ownGoalsOrdinal);
        }



    let mapToSqlType (players:seq<T>) = 
        let metaData = [|
            new SqlMetaData("fId", SqlDbType.Int);
            new SqlMetaData("FixtureId", SqlDbType.Int);
            new SqlMetaData("PlayerId", SqlDbType.Int);
            new SqlMetaData("MinutesPlayed", SqlDbType.Int);
            new SqlMetaData("GoalsScored", SqlDbType.Int);
            new SqlMetaData("Assists", SqlDbType.Int);
            new SqlMetaData("CleanSheet", SqlDbType.Bit);
            new SqlMetaData("ShotsSaved", SqlDbType.Int);
            new SqlMetaData("PenaltiesSaved", SqlDbType.Int);
            new SqlMetaData("PenaltiesMissed", SqlDbType.Int);
            new SqlMetaData("GoalsConceded", SqlDbType.Int);
            new SqlMetaData("YellowCards", SqlDbType.Int);
            new SqlMetaData("RedCards", SqlDbType.Int);
            new SqlMetaData("OwnGoals", SqlDbType.Int);
        |]

        let record = new SqlDataRecord(metaData)
        players |> Seq.map (fun player ->
                record.SetInt32(0, player.Id)
                record.SetInt32(1, player.FixtureId)
                record.SetInt32(2, player.PlayerId)
                record.SetInt32(3, player.MinutesPlayed)
                record.SetInt32(4, player.GoalsScored)
                record.SetInt32(5, player.Assists)
                record.SetBoolean(6, player.CleanSheet)
                record.SetInt32(7, player.ShotsSaved)
                record.SetInt32(8, player.PenaltiesSaved)
                record.SetInt32(9, player.PenaltiesMissed)
                record.SetInt32(10, player.GoalsConceded)
                record.SetInt32(11, player.YellowCards)
                record.SetInt32(12, player.RedCard)
                record.SetInt32(13, player.OwnGoals)
                record)


