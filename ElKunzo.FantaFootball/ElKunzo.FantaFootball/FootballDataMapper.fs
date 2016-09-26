namespace ElKunzo.FantaFootball.FootballDataOrg

open System

open ElKunzo.FantaFootball.DomainTypes
open ElKunzo.FantaFootball.Internal
open ElKunzo.FantaFootball.FootballDataOrg.FootballDataTypes

module Mapper = 

    let mapTeamStaticData (extTeam:Team) = 
        let footballDataId = extTeam._Links.Self.Href.Split('/') |> Seq.last |> int
        let known = TeamStaticData.Cache.GetData |> Seq.tryFind (fun t -> t.FootballDataId = footballDataId)
        let result : TeamStaticData.T = {
                Id = match known with | None -> -1 | Some x -> x.Id; 
                FootballDataId = footballDataId;
                WhoScoredId = match known with | None -> -1 | Some x -> x.WhoScoredId; 
                Name = extTeam.ShortName;
                FullName = extTeam.Name;
                Code = (mapNullString extTeam.Code);
                SquadMarketValue = (mapMarketValue extTeam.SquadMarketValue);
                CrestUrl = (mapNullString extTeam.CrestUrl);
            } 
        Success result



    let mapTeamStaticDataCollection (teams : OperationResult<seq<Team>, string>) = 
        match teams with
        | Failure x -> Seq.singleton (Failure x)
        | Success x -> x |> Seq.map (fun t -> mapTeamStaticData t)



    let mapPlayerStaticData (extPlayer:Player) = 
        let internalTeam = TeamStaticData.Cache.GetData |> Seq.find (fun t -> t.FootballDataId = extPlayer.FootballDataTeamId)
        let mapPosition positionAsString = 
            match positionAsString with
            | "Keeper" -> Position.Goalkeeper
            | "Right-Back" | "Left-Back" | "Centre Back" -> Position.Defender
            | "Defensive Midfield" | "Central Midfield" | "Attacking Midfield" -> Position.Midfielder
            | "Left Midfield" | "Right Midfield" | "Left Wing" | "Right Wing" -> Position.Midfielder
            | "Centre Forward" | "Secondary Striker" -> Position.Forward
            | _ -> Position.Unknown

        let getName (extName:string) = 
                extName.Split('\n').[0]

        let isPlayerTheSame (internalPlayer:PlayerStaticData.T) (externalPlayer:Player) =
            internalPlayer.DateOfBirth = externalPlayer.DateOfBirth &&
            internalPlayer.FullName = (externalPlayer.Name |> getName)

        let mapJerseyNumber name numberAsString =
            let opt = (mapNullString numberAsString)
            match opt with
            | None -> None
            | Some x -> if name = "Luca Ceppitelli" then Some (23) else Some (int x)

        let mapContractUntil contractUntilAsString = 
            let opt = (mapNullString contractUntilAsString)
            match opt with
            | None -> None
            | Some (x:string) -> let data = x.Split('-') |> Array.map (fun a -> int a)
                                 Some (DateTime(data.[0], data.[1], data.[2]))

        let internalId, whoScoredId = 
            let knownPlayersForTeam = PlayerStaticData.Cache.GetData |> Seq.filter (fun p -> p.TeamId = internalTeam.Id)
            let known = knownPlayersForTeam |> Seq.tryFind (fun p -> isPlayerTheSame p extPlayer)
            match known with
            | None -> (-1, -1)
            | Some x -> (x.Id, x.WhoScoredId)

        let result : PlayerStaticData.T = {
                Id = internalId
                WhoScoredId = whoScoredId;
                FootballDataTeamId = extPlayer.FootballDataTeamId;
                TeamId = internalTeam.Id;
                JerseyNumber = (mapJerseyNumber (extPlayer.Name |> getName) extPlayer.JerseyNumber);
                Position = (mapPosition extPlayer.Position);
                Name = extPlayer.Name |> getName;
                FullName = extPlayer.Name |> getName;
                DateOfBirth = extPlayer.DateOfBirth;
                Nationality = extPlayer.Nationality
                ContractUntil = mapContractUntil extPlayer.ContractUntil;
                MarketValue = (mapMarketValue extPlayer.MarketValue);
            }

        Success result



    let mapPlayerStaticDataCollection playerData = 
        match playerData with
        | Failure x -> Seq.singleton (Failure x) 
        | Success x -> x |> Seq.map (fun p -> mapPlayerStaticData p)



    let mapFixtureData (fixture:Fixture) = 
        let mapTeamId (stringUrl:string) =
            let footballDataId = stringUrl.Split('/') |> Seq.last |> int
            let teamOption = TeamStaticData.Cache.GetData |> Seq.tryFind(fun t -> t.FootballDataId = footballDataId)
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

        let known = FixtureData.Cache.GetData |> Seq.tryFind (fun t -> t.FootballDataId = footballDataFixtureId)

        let result : FixtureData.T = {
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
        Success result
