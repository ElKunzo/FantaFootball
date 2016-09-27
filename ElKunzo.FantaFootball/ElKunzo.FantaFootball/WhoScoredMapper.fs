namespace ElKunzo.FantaFootball.WhoScoredCom

open System
open System.Collections.Generic

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.Internal
open ElKunzo.FantaFootball.External.WhoScoredTypes

module Mapper = 

    let mapPlayerIds (report:MatchReport) = 
        let compareJerseyNumber internalNo externalNo =
                match internalNo with
                | None -> false
                | Some x -> x = externalNo

        let wasPlayerOnPitch (externalPlayer:PlayerData) = 
            if (externalPlayer.IsFirstEleven) then true
            else 
               match box externalPlayer.SubbedInPeriod with
               | null -> false
               | _ -> true

        let getInternalPlayer teamId (externalPlayer:PlayerData) = 
            let foundById = PlayerStaticData.Cache.GetData |> Seq.tryFind (fun p -> p.WhoScoredId = externalPlayer.PlayerId)
            let foundByName = PlayerStaticData.Cache.GetData |> Seq.tryFind (fun p -> p.Name = externalPlayer.Name || p.FullName = externalPlayer.Name)
            let foundByTeamAndShirt = PlayerStaticData.Cache.GetData |> Seq.tryFind (fun p -> p.TeamId = teamId && compareJerseyNumber p.JerseyNumber externalPlayer.ShirtNo)
            match foundById with
            | Some _ -> None //PlayerId already existing
            | None -> match foundByName with
                      | Some x -> Some (x.Id, externalPlayer.PlayerId)
                      | None -> match foundByTeamAndShirt with
                                | None -> Some (-1, externalPlayer.PlayerId)
                                | Some x -> Some (x.Id, externalPlayer.PlayerId)

        let mapPlayer (team:TeamStaticData.T) (externalPlayer:PlayerData) : PlayerStaticData.T = 
            let mapPosition (positionString:string) =
                match positionString with
                | "GK" -> Position.Goalkeeper
                | "DL" | "DC" | "DR" -> Position.Defender
                | "ML" | "MC" | "MR" -> Position.Midfielder
                | "F" | "Forward" -> Position.Forward
                | _ -> Position.Unknown

            {
                Id = -1;
                WhoScoredId = externalPlayer.PlayerId;
                FootballDataTeamId = team.FootballDataId;
                TeamId = team.Id;
                JerseyNumber = Some externalPlayer.ShirtNo;
                Position = (mapPosition externalPlayer.Position);
                Name = externalPlayer.Name;
                FullName = externalPlayer.Name;
                DateOfBirth = DateTime.Now.AddYears(-1 * externalPlayer.Age);
                Nationality = null;
                ContractUntil = None;
                MarketValue = None;
            }

        let getUpdateablePlayerIds teamId (externalPlayers:seq<PlayerData>) = 
            let team = TeamStaticData.Cache.GetData |> Seq.find (fun x -> x.WhoScoredId = teamId)
            let result = 
                externalPlayers 
                |> Seq.filter (fun externalPlayer -> wasPlayerOnPitch externalPlayer)
                |> Seq.map (fun externalPlayer -> externalPlayer |> getInternalPlayer team.Id)
            let intermediateResult = result |> Seq.filter (fun x -> x.IsSome) |> Seq.map (fun x -> x.Value) |> Seq.toArray
            let knownUpdateablePlayerIds = intermediateResult |> Seq.filter (fun (x, _) -> x <> -1)
            let unknownPlayers = intermediateResult |> Seq.filter (fun (x, _) -> x = -1) |> Seq.map (fun (_, id) -> externalPlayers |> Seq.find (fun x -> x.PlayerId = id) |> mapPlayer team)
            (knownUpdateablePlayerIds, unknownPlayers)

        let homePlayers =  report.Home.Players |> getUpdateablePlayerIds report.Home.TeamId
        let awayPlayers =  report.Away.Players |> getUpdateablePlayerIds report.Away.TeamId
        let known = (Seq.concat [| (fst homePlayers); (fst awayPlayers) |])
        let unknown = (Seq.concat [| (snd homePlayers); (snd awayPlayers) |])
        Success (known, unknown)



    let mapPlayerScoreData fixtureId 
                           (ownIncidentEvents:IDictionary<string,seq<IncidentEvent>>) 
                           (opponentIncidentEvents:IDictionary<string,seq<IncidentEvent>>) 
                           playerId 
                           (playerData:PlayerData) 
                           maxMinute =
        let maxMinute = if maxMinute > 130 then 90 else maxMinute
        let getPlayerScore item = 
            ownIncidentEvents.Item(item) |> Seq.filter(fun x -> x.PlayerId = playerData.PlayerId) |> Seq.length

        let minutesOnPitch = 
            let substitutedOn = ownIncidentEvents.Item("SubOn") |> Seq.tryFind(fun x -> x.PlayerId = playerData.PlayerId)
            let substitutedOff = ownIncidentEvents.Item("SubOff") |> Seq.tryFind(fun x -> x.PlayerId = playerData.PlayerId)
            match playerData.IsFirstEleven with
            | true when substitutedOff.IsSome -> [| for i in 0 .. substitutedOff.Value.Minute do yield i |]
            | true -> [| for i in 0 .. 90 do yield i |]
            | false when substitutedOn.IsSome -> [| for i in substitutedOn.Value.Minute .. maxMinute do yield i |]
            | false -> [| 0 |]
        let minutesPlayed = max 0 ((minutesOnPitch |> Array.last) - (minutesOnPitch |> Array.head))
        let goalsConceded = opponentIncidentEvents.Item("Goal") |> Seq.filter (fun x -> Array.exists (fun m -> m = x.Minute) minutesOnPitch) |> Seq.length
        let cleanSheet = (goalsConceded = 0) && (minutesPlayed > 60)
        let shotsSaved = if (isNull (box playerData.Stats.TotalSaves)) then 0 else playerData.Stats.TotalSaves |> Map.toSeq |> Seq.sumBy (fun (_,v) -> v) |> int
                          
        let result : PlayerScoreData.T = {
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
        result



    let calculateTotalPoints (player:PlayerScoreData.T) (playerPosition:Position) = 
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



    let mapMatchReport (report:MatchReport) = 
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

        let fixture = FixtureData.Cache.GetData |> Seq.find (fun x -> x.WhoScoredId = report.WhoScoredId)
        let homeIncidentEvents = getEvents true |> dict
        let awayIncidentEvents = getEvents false |> dict

        let homePlayerMapper (player:PlayerData) = 
            let internalPlayer = PlayerStaticData.Cache.GetData |> Seq.tryFind (fun x -> x.WhoScoredId = player.PlayerId)
            match internalPlayer with
            | Some x -> Some (mapPlayerScoreData fixture.Id homeIncidentEvents awayIncidentEvents x.Id player (report.PeriodEndMinutes.Item(2)))
            | None -> None

        let awayPlayerMapper (player:PlayerData) = 
            let internalPlayer = PlayerStaticData.Cache.GetData |> Seq.tryFind (fun x -> x.WhoScoredId = player.PlayerId)
            match internalPlayer with
            | Some x -> Some (mapPlayerScoreData fixture.Id awayIncidentEvents homeIncidentEvents x.Id player (report.PeriodEndMinutes.Item(2)))
            | None -> None

        let fantasyPoints (player:PlayerScoreData.T) = 
            let internalPlayer = PlayerStaticData.Cache.GetData |> Seq.find (fun x -> x.Id = player.PlayerId)
            calculateTotalPoints player internalPlayer.Position

        let getInternalId (player:PlayerScoreData.T) =
            let internalPlayer = PlayerScoreData.Cache.GetData |> Seq.tryFind (fun x -> x.PlayerId = player.PlayerId && x.FixtureId = player.FixtureId)
            match internalPlayer with
            | Some x -> x.Id
            | None -> -1

        let map (mapperFunction:PlayerData -> PlayerScoreData.T option) playerSequence = 
            playerSequence
            |> Seq.map (fun x -> mapperFunction x) 
            |> Seq.filter (fun x -> x.IsSome) 
            |> Seq.map (fun x -> x.Value)
            |> Seq.map (fun x -> { x with Id = getInternalId x })
            |> Seq.map (fun x -> { x with TotalPoints = fantasyPoints x })
            |> Seq.toArray
        
        let awayPlayers = report.Away.Players |> map awayPlayerMapper
        let homePlayers = report.Home.Players |> map homePlayerMapper
        Success (Array.concat [ homePlayers; awayPlayers ])



    let mapTeamIds data = 
        let getUpdateableTeam (internalId, whoScoredId) =
            TeamStaticData.Cache.Update () |> ignore
            let known = TeamStaticData.Cache.GetData 
                        |> Seq.filter (fun t -> t.WhoScoredId = -1)
                        |> Seq.filter (fun t -> t.Id = internalId)
            match (known |> Seq.length) with
            | 1 -> let t = known |> Seq.head; 
                   if (t.WhoScoredId <> whoScoredId) then Some (t.Id, whoScoredId) else None
            | _ -> None

        match data with
        | Failure x -> Failure x
        | Success x -> let teamIds = [| (x.InternalHomeId, x.WhoScoredHomeId); (x.InternalAwayId, x.WhoScoredAwayId) |]
                                     |> Array.map (fun y -> getUpdateableTeam y)
                                     |> Array.filter (fun y -> y.IsSome)
                                     |> Array.map (fun y -> y.Value)
                       Success teamIds



    let mapFixtureId whoScoredCalendarData = 
        match whoScoredCalendarData with
        | Failure x -> Failure x
        | Success x -> let missingIdFixtures = FixtureData.Cache.GetData |> Seq.filter (fun t -> t.WhoScoredId = -1)
                       let internalId = missingIdFixtures |> Seq.tryFind (fun d -> d.HomeTeamId = x.InternalHomeId && d.AwayTeamId = x.InternalAwayId)
                       match internalId with
                       | None -> Success Seq.empty
                       | Some y -> Success (Seq.singleton (y.Id, x.WhoScoredId))



    let parseCalendarData whoScoredId (htmlString:string) =
        let data = htmlString
                    .Split([| "matchHeader.load([" |], StringSplitOptions.RemoveEmptyEntries).[1]
                    .Split(']').[0]
                    .Replace("'", "")
                    .Split([| "," |], StringSplitOptions.RemoveEmptyEntries)

        let homeId, homeName = Convert.ToInt32(data.[0]), data.[2]
        let awayId, awayName = Convert.ToInt32(data.[1]), data.[3]
        let dateValues = data.[4].Split([| '/'; ':'; ' ' |], StringSplitOptions.RemoveEmptyEntries) |> Array.map (fun x -> int x)
        let dateObject = new DateTime(dateValues.[2], dateValues.[1], dateValues.[0], dateValues.[3], dateValues.[4], dateValues.[5])
        let gmtTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")
        let dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateObject, gmtTimeZone)
        let homeTeam = TeamStaticData.Cache.GetData |> Seq.tryFind (fun t -> t.Name = homeName || t.FullName = homeName)
        let awayTeam = TeamStaticData.Cache.GetData |> Seq.tryFind (fun t -> t.Name = awayName || t.FullName = awayName)
        
        match homeTeam, awayTeam with
        | Some home, Some away -> Success 
                                    { 
                                        WhoScoredId = whoScoredId; 
                                        MatchDateUtc = dateUtc; 
                                        WhoScoredHomeId = homeId; 
                                        WhoScoredAwayId = awayId; 
                                        InternalHomeId = home.Id; 
                                        InternalAwayId = away.Id; 
                                    }
        | _, _ -> Failure "Home or Away team not recognized"
