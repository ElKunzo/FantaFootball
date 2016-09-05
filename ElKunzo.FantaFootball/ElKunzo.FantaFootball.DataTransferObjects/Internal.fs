namespace ElKunzo.FantaFootball.DataTransferObjects

open System

module Internal = 

    type Position = Goalkeeper = 1 | Defender = 2 | Midfielder = 3 | Forward = 4 | Unknown = 5


    [<StructuredFormatDisplay("Id: {Id} - Name: {FullName}")>]
    type TeamStaticData = {
            Id : int;
            FootballDataId : int;
            WhoScoredId : int;
            Name : string;
            FullName : string;
            Code : string option;
            SquadMarketValue : int option;
            CrestUrl : string option;
        }

    type PlayerStaticData = {
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