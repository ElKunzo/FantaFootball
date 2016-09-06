namespace ElKunzo.FantaFootball.Internal

open System
open System.Net.Http
open System.Data
open System.Data.Common
open Microsoft.SqlServer.Server
open Newtonsoft.Json

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



    let mapFromExternal homeTeamId awayTeamId (fixture:Fixture) = 
        let mapStatus s = 
            match s with
            | "SCHEDULED" -> FixtureStatus.Scheduled
            | "TIMED" -> FixtureStatus.Timed
            | "IN_PLAY" -> FixtureStatus.InPlay
            | "FINISHED" -> FixtureStatus.Finished
            | "POSTPONED" -> FixtureStatus.Postponed
            | "CANCELED" -> FixtureStatus.Canceled
            | _ -> failwith "Could not identify fixture stauts."

        let mapFixtureIdFromUrl (urlString:string) = 
            int (urlString.Split('/') |> Seq.last)

        let mapScoreFromNullString scoreString = 
            let s = mapNullString scoreString
            match s with
            | None -> None
            | Some x -> Some (int x)

        {
            Id = -1;
            WhoScoredId = -1;
            FootballDataId = mapFixtureIdFromUrl fixture._Links.Self.Href;
            Status = (mapStatus fixture.Status);
            KickOff = fixture.Date;
            MatchDay = fixture.MatchDay;
            HomeTeamId = homeTeamId;
            AwayTeamId = awayTeamId;
            HomeScore = mapScoreFromNullString fixture.Result.GoalsHomeTeam;
            AwayScore = mapScoreFromNullString fixture.Result.GoalsAwayTeam;
        }



    let mapFromExternalSeasonFixtures (teamDataCache:TeamStaticData.Cache) seasonFixtures = 
        let map (fixture:Fixture) = 
            let homeTeamOption = 
                let footballDataHomeId = fixture._Links.HomeTeam.Href.Split('/') |> Seq.last |> int
                teamDataCache.PublicData |> Seq.tryFind(fun t -> t.FootballDataId = footballDataHomeId)
            let awayTeamOption = 
                let footballDataAwayId = fixture._Links.AwayTeam.Href.Split('/') |> Seq.last |> int
                teamDataCache.PublicData |> Seq.tryFind(fun t -> t.FootballDataId = footballDataAwayId)

            if (homeTeamOption.IsNone || awayTeamOption.IsNone) then
                failwith "Could not find participan teams in cache."
            else
                let home = homeTeamOption.Value
                let away = awayTeamOption.Value
                mapFromExternal home.Id away.Id (fixture:Fixture)

        seasonFixtures.Fixtures |> Seq.map (fun f -> map f)



    let downloadDataAsync cache baseUrl = async {
        let url = baseUrl + "/fixtures"
        let! result = downloadAsync url buildFootballDataApiHttpClient

        match result with 
        | None -> return None
        | Some x -> 
            let seasonFixtures = Some (JsonConvert.DeserializeObject<SeasonFixtures>(x))
            return Some (mapFromExternalSeasonFixtures cache seasonFixtures.Value)
    }

