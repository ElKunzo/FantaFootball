namespace ElKunzo.FantaFootball.DataTransferObjects

open System;

module External = 
    type Link = {
            Href : string
        }

    type CompetitionLinks = {
            Self : Link;
            Competition : Link;
        }

    type TeamLinks = {
            Self : Link;
            Fixtures : Link;
            Players : Link;
        }

    type PlayerLinks = {
        Self : Link;
        Team : Link;
        }

    type Player = {
            Name : string;
            Position : string;
            JerseyNumber : string;
            DateOfBirth : DateTime;
            Nationality : string;
            ContractUntil : string;
            MarketValue : string;
            FootballDataTeamId : int;
        }

    type PlayerCollection = {
            _Links : PlayerLinks;
            Count : int;
            Players : seq<Player>;
        }

    [<StructuredFormatDisplay("{Code} - {Name}")>]
    type Team = {
            _Links : TeamLinks;
            Name : string;
            ShortName : string;
            Code : string;
            SquadMarketValue : string;
            CrestUrl : string;
            Players : seq<Player>;
            FootballDataId : int;
        }

    type Competition = {
            _Links : CompetitionLinks;
            Count : int;
            Teams : seq<Team>;
        }

    type FixtureLinks = {
            Self : Link;
            Competition : Link;
            HomeTeam : Link;
            AwayTeam : Link;
        }

    type Result = {
            GoalsHomeTeam : string;
            GoalsAwayTeam : string;
        }

    type Odds = {
            HomeWin : double;
            Draw : double;
            AwayWin : double;
        }

    type Fixture = {
            _Links : FixtureLinks;
            Date : DateTime;
            Status : string;
            MatchDay : int;
            HomeTeamName : int;
            AwayTeamName : int;
            Result : Result;
            Odds : Odds;
        }

    type SeasonFixturesLinks = {
            Self : Link;
            Competition : Link;
        }

    type SeasonFixtures = { 
            _Links : SeasonFixturesLinks;
            Count : int;
            Fixtures : seq<Fixture>;
        }