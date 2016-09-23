namespace ElKunzo.FantaFootball.Internal

open System
open System.Collections.Generic
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server

open ElKunzo.FantaFootball
open ElKunzo.FantaFootball.DataAccess
open ElKunzo.FantaFootball.External.WhoScoredTypes

module PlayerScoreData = 

    type T = {
        Id : int;
        FixtureId : int;
        PlayerId : int;
        TotalPoints : int;
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
        let totalPointsOrdinal = dataReader.GetOrdinal("fTotalPoints")
        let minutesPlayedOrdinal = dataReader.GetOrdinal("fMinutesPlayed")
        let goalsScoredOrdinal = dataReader.GetOrdinal("fGoalsScored")
        let assistsOrdinal = dataReader.GetOrdinal("fAssists")
        let cleanSheetOrdinal = dataReader.GetOrdinal("fCleanSheet")
        let shotsSavedOrdinal = dataReader.GetOrdinal("fShotsSaved")
        let penaltiesSavedOrdinal = dataReader.GetOrdinal("fPenaltiesSaved")
        let penaltiesMissedOrdinal = dataReader.GetOrdinal("fPenaltiesMissed")
        let goalsConcededOrdinal = dataReader.GetOrdinal("fGoalsConceded")
        let yellowCardsOrdinal = dataReader.GetOrdinal("fYellowCards")
        let redCardsOrdinal = dataReader.GetOrdinal("fRedCard")
        let ownGoalsOrdinal = dataReader.GetOrdinal("fOwnGoals")
        
        {
            Id = dataReader.GetInt32(idOrdinal);
            FixtureId = dataReader.GetInt32(fixtureIdOrdinal);
            PlayerId = dataReader.GetInt32(playerIdOrdinal);
            TotalPoints = dataReader.GetInt32(totalPointsOrdinal);
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
            new SqlMetaData("TotalPoints", SqlDbType.Int);
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
                record.SetInt32(3, player.TotalPoints)
                record.SetInt32(4, player.MinutesPlayed)
                record.SetInt32(5, player.GoalsScored)
                record.SetInt32(6, player.Assists)
                record.SetBoolean(7, player.CleanSheet)
                record.SetInt32(8, player.ShotsSaved)
                record.SetInt32(9, player.PenaltiesSaved)
                record.SetInt32(10, player.PenaltiesMissed)
                record.SetInt32(11, player.GoalsConceded)
                record.SetInt32(12, player.YellowCards)
                record.SetInt32(13, player.RedCard)
                record.SetInt32(14, player.OwnGoals)
                record)



    let mapPlayerData fixtureId (ownIncidentEvents:IDictionary<string,seq<IncidentEvent>>) (opponentIncidentEvents:IDictionary<string,seq<IncidentEvent>>) 
                      playerId (playerData:PlayerData) =
        let getPlayerScore item = 
            ownIncidentEvents.Item(item) |> Seq.filter(fun x -> x.PlayerId = playerData.PlayerId) |> Seq.length

        let minutesOnPitch = 
            let substitutedOn = ownIncidentEvents.Item("SubOn") |> Seq.tryFind(fun x -> x.PlayerId = playerData.PlayerId)
            let substitutedOff = ownIncidentEvents.Item("SubOff") |> Seq.tryFind(fun x -> x.PlayerId = playerData.PlayerId)
            match playerData.IsFirstEleven with
            | true when substitutedOff.IsSome -> [| for i in 0 .. substitutedOff.Value.Minute do yield i |]
            | true -> [| for i in 0 .. 90 do yield i |]
            | false when substitutedOn.IsSome -> [| for i in (90 - substitutedOn.Value.Minute) .. 90 do yield i |]
            | false -> [| 0 |]
        let minutesPlayed = max 0 ((minutesOnPitch |> Array.last) - (minutesOnPitch |> Array.head))
        let goalsConceded = opponentIncidentEvents.Item("Goal") |> Seq.filter (fun x -> Array.exists (fun m -> m = x.Minute) minutesOnPitch) |> Seq.length
        let cleanSheet = (goalsConceded = 0) && (minutesPlayed > 60)
        let shotsSaved = if (isNull (box playerData.Stats.TotalSaves)) then 0 else playerData.Stats.TotalSaves |> Map.toSeq |> Seq.sumBy (fun (_,v) -> v) |> int
                          
        {
            Id = -1;
            FixtureId = fixtureId;
            PlayerId = playerId;
            TotalPoints = -1;
            MinutesPlayed = minutesPlayed;
            GoalsScored = getPlayerScore "Goal";
            Assists = getPlayerScore "Assist";
            CleanSheet = cleanSheet;
            ShotsSaved = shotsSaved;
            PenaltiesSaved = getPlayerScore "PenSaved";
            PenaltiesMissed = getPlayerScore "PenMissed";
            GoalsConceded = goalsConceded;
            YellowCards = getPlayerScore "YellowCard";
            RedCard = getPlayerScore "RedCard";
            OwnGoals = getPlayerScore "OwnGoal";
        }



    let calculateTotalPoints (player:T) (playerPosition:Position) = 
        let wasPlayerOnPitch = 
            if player.MinutesPlayed = 0 then 0 else 1

        let minutePoints = 
            match player.MinutesPlayed with
            | 0 -> 0
            | _ when player.MinutesPlayed < 60 -> 1
            | _ -> 2

        let goalPoints = 
            match playerPosition with
            | Position.Goalkeeper | Position.Defender -> 6 * player.GoalsScored
            | Position.Midfielder -> 5 * player.GoalsScored
            | Position.Forward -> 4 * player.GoalsScored
            | _ -> 0

        let assistPoints = 
            3 * player.Assists

        let cleanSheetPoints = 
            let cleanSheet = if player.CleanSheet then 1 else 0
            match playerPosition with
            | Position.Goalkeeper | Position.Defender -> 4 * cleanSheet
            | Position.Midfielder -> 1 * cleanSheet
            | Position.Forward -> 0
            | _ -> 0

        let savesPoints = 
            match playerPosition with
            | Position.Goalkeeper -> player.ShotsSaved / 3 
            | _ -> 0

        let penaltySavePoints = 
            player.PenaltiesSaved * 5

        let penaltyMissedPoints = 
            player.PenaltiesMissed * (-2)

        let goalsConcededPoints = 
            match playerPosition with
            | Position.Goalkeeper | Position.Defender -> (-1) * (player.GoalsConceded / 2)
            | _ -> 0

        let yellowCardPoints = 
            player.YellowCards * -1

        let redCardPoints = 
            player.RedCard * -3

        let ownGoalPoints = 
            player.OwnGoals * -2

        let result = wasPlayerOnPitch * (minutePoints + goalPoints + assistPoints + cleanSheetPoints + savesPoints + 
                        penaltySavePoints + penaltyMissedPoints + goalsConcededPoints + yellowCardPoints + 
                        redCardPoints + ownGoalPoints)

        result



    let getDataForMatchReportAsync (scoreCache:Cache) (playerCache:PlayerStaticData.Cache) (fixtureCache:FixtureData.Cache) (report:MatchReport) = async {
        let findEvent (team:int option) (queryEvent:string) (qualifiers:string[] option) (eventList:seq<IncidentEvent>) = 
            let containsAll (qualifiers:string[]) (theList:seq<IncidentEventQualifier>) = 
                let result = qualifiers |> Seq.map (fun s -> theList |> Seq.tryFind (fun x -> x.Type.DisplayName = s))
                result |> Seq.forall (fun o -> o.IsSome)
            let foundEvents = eventList |> Seq.filter (fun e -> e.Type.DisplayName = queryEvent)
            let foundEventsWithQualifiers = 
                match qualifiers with
                | Some qual -> foundEvents |> Seq.filter (fun e -> (e.Qualifiers |> containsAll qual))
                | None -> foundEvents
            let foundEventsWithQualifiersAndTeam = 
                match team with
                | Some id -> foundEventsWithQualifiers |> Seq.filter (fun e -> e.TeamId = id)
                | None -> foundEventsWithQualifiers
            foundEventsWithQualifiersAndTeam

        let getGoals (ownIncidentEvents:seq<IncidentEvent>) (opponentIncidentEvents:seq<IncidentEvent>) = 
            let goals = ownIncidentEvents 
                        |> findEvent None "Goal" None
                        |> Seq.filter (fun e -> (e.Qualifiers |> Seq.tryFind (fun x -> x.Type.DisplayName = "OwnGoal")).IsNone)
            let opponentOwnGoals = opponentIncidentEvents |> findEvent None "Goal" (Some [| "OwnGoal" |])
            Seq.concat [| goals; opponentOwnGoals |]

        let getMissedPenalties opponentTeamId =
            report.Events 
            |> findEvent (Some opponentTeamId) "PenaltyFaced" None
            |> Seq.map (fun x -> (x.Qualifiers |> Seq.find (fun x -> x.Type.Value = 233)).Value |> int)
            |> Seq.map (fun i -> report.Events |> Seq.find (fun x -> x.IsShot = true && x.EventId = i))
            |> Seq.filter (fun x -> x.IsGoal = false)

        let getEvents isHome = 
            let ownTeamId, opponentTeamId, ownIncidentEvents, opponentIncidentEvents = 
                match isHome with
                | true -> report.Home.TeamId, report.Away.TeamId, report.Home.IncidentEvents, report.Away.IncidentEvents
                | false -> report.Away.TeamId, report.Home.TeamId, report.Away.IncidentEvents, report.Home.IncidentEvents
            let goal = ("Goal", getGoals ownIncidentEvents opponentIncidentEvents)
            let ownGoal = ("OwnGoal", ownIncidentEvents |> findEvent None "Goal" (Some [| "OwnGoal" |]))
            let pass = ("Assist", ownIncidentEvents |> findEvent None "Pass" None)
            let subOn = ("SubOn", ownIncidentEvents |> findEvent None "SubstitutionOn" None)
            let subOff = ("SubOff", ownIncidentEvents |> findEvent None "SubstitutionOff" None)
            let yellowCard = ("YellowCard", ownIncidentEvents |> findEvent None "Card" (Some [| "Yellow" |]))
            let redCard = ("RedCard", ownIncidentEvents |> findEvent None "Card" (Some [| "Red" |]))
            let penConc = ("PenConc", report.Events |> findEvent (Some ownTeamId) "Foul" (Some [| "Defensive"; "Penalty" |]))
            let penSaved = ("PenSaved", report.Events |> findEvent (Some ownTeamId) "PenaltyFaced" (Some [| "KeeperSaved" |]))
            let penMissed = ("PenMissed", getMissedPenalties opponentTeamId)
            [ goal ; ownGoal ; pass ; subOn ; subOff ; yellowCard ; redCard ; penConc ; penSaved ; penMissed ]

        let fixture = fixtureCache.PublicData |> Seq.find (fun x -> x.WhoScoredId = report.WhoScoredId)
        let homeIncidentEvents = getEvents true |> dict
        let awayIncidentEvents = getEvents false |> dict

        let homePlayerMapper (player:PlayerData) = 
            let internalPlayer = playerCache.PublicData |> Seq.tryFind (fun x -> x.WhoScoredId = player.PlayerId)
            match internalPlayer with
            | Some x -> Some (mapPlayerData fixture.Id homeIncidentEvents awayIncidentEvents x.Id player)
            | None -> None

        let awayPlayerMapper (player:PlayerData) = 
            let internalPlayer = playerCache.PublicData |> Seq.tryFind (fun x -> x.WhoScoredId = player.PlayerId)
            match internalPlayer with
            | Some x -> Some (mapPlayerData fixture.Id awayIncidentEvents homeIncidentEvents x.Id player)
            | None -> None

        let fantasyPoints player = 
            let internalPlayer = playerCache.PublicData |> Seq.find (fun x -> x.Id = player.PlayerId)
            calculateTotalPoints player internalPlayer.Position

        let getInternalId (player:T) =
            let internalPlayer = scoreCache.PublicData |> Seq.tryFind (fun x -> x.PlayerId = player.PlayerId && x.FixtureId = player.FixtureId)
            match internalPlayer with
            | Some x -> x.Id
            | None -> -1

        let map (mapperFunction:PlayerData -> T option) playerSequence = 
            playerSequence
            |> Seq.map (fun x -> mapperFunction x) 
            |> Seq.filter (fun x -> x.IsSome) 
            |> Seq.map (fun x -> x.Value)
            |> Seq.map (fun x -> { x with TotalPoints = fantasyPoints x })
            |> Seq.map (fun x -> { x with Id = getInternalId x })
            |> Seq.toArray
        
        let awayPlayers = report.Away.Players |> map awayPlayerMapper
        let homePlayers = report.Home.Players |> map homePlayerMapper
        let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@PlayerData" mapToSqlType (Array.concat [ homePlayers; awayPlayers ])
        return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_PlayerScoreData_Update" [| sqlParameter |]
    }


