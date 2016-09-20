namespace ElKunzo.FantaFootball.External

open System
open System.Collections.Generic

module WhoScoredTypes = 

    type FormationInfo = {
        JerseyNumber : int;
        FormationSlot : int;
        PlayerId : int;
    }



    type FormationPositions = {
        Vertical : decimal;
        Horizontal : decimal;
    }



    type FormationData = {
        FormationId : int;
        FormationName : string; 
        CaptainPlayerId : int;
        Period : int;
        StartMinuteExpanded : int; 
        EndMinuteExpanded : int;
        FormationInfos : seq<FormationInfo>; 
        FormationPositions : seq<FormationPositions>; 
    }



    type IncidentEventType = {
        Value : int;
        DisplayName : string;
    }



    type IncidentEventQualifier = {
        Type : IncidentEventType;
        Value : string;
    }



    type IncidentEvent = {
        Id : float;
        EventId : int;
        Minute : int;
        Second : int;
        TeamId : int;
        PlayerId : int;
        RelatedEventId : int;
        RelatedPlayerId : int;
        X : float;
        Y : float;
        ExpandedMinute : int;
        Period : IncidentEventType;
        Type : IncidentEventType;
        OutcomeType : IncidentEventType;
        Qualifiers : seq<IncidentEventQualifier>;
        SatisfiedEventsTypes : seq<int>;
        IsTouch : bool;
        GoalMouthZ : float;
        GoalMouthY : float;
        IsGoal : bool;
        IsShot : bool;
        EndX : float;
        EndY : float;
    }



    type Statistics = {
        MinutesWithStats : seq<int>;
        Ratings : Map<int, float>;
        TotalSaves : Map<int, float>;
        ClaimsHigh : Map<int, float>;
        Collected : Map<int, float>;
        ShotsTotal : Map<int, float>;
        ShotsOnPost : Map<int, float>;
        ShotsOnTarget : Map<int, float>;
        ShotsOffTarget : Map<int, float>;
        ShotsBlocked : Map<int, float>;
        Clearances : Map<int, float>;
        Interceptions : Map<int, float>;
        Possession : Map<int, float>;
        Touches : Map<int, float>;
        PassesTotal : Map<int, float>;
        PassesAccurate : Map<int, float>;
        PassesKey : Map<int, float>;
        PassSuccess : Map<int, float>;
        AerialsTotal : Map<int, float>;
        AerialsWon : Map<int, float>;
        AerialSuccess : Map<int, float>;
        OffensiveAerials : Map<int, float>;
        DefensiveAerials : Map<int, float>;
        CornersTotal : Map<int, float>;
        CornersAccurate : Map<int, float>;
        ThrowInsTotal : Map<int, float>;
        ThrowInsAccurate : Map<int, float>;
        ThrowInAccuracy : Map<int, float>;
        OffsidesCaught : Map<int, float>;
        FoulsCommited : Map<int, float>;
        TacklesTotal : Map<int, float>;
        TackleSuccessful : Map<int, float>;
        TackleUnsuccesful : Map<int, float>;
        TackleSuccess : Map<int, float>;
        DribbledPast : Map<int, float>;
        DribblesWon : Map<int, float>;
        DribblesAttempted : Map<int, float>;
        DribblesLost : Map<int, float>;
        DribbleSuccess : Map<int, float>;
        Dispossessed : Map<int, float>;
        Errors : Map<int, float>;
    }



    type PlayerData = {
        PlayerId : int;
        ShirtNo : int;
        Name : string;
        Position : string;
        Height : int;
        Weight : int;
        Age : int;
        IsFirstEleven : bool;
        SubbedInPlayerId : int;
        SubbedInPeriod : IncidentEventType;
        SubbedInExpandedMinute : int;
        SubbedOutPlayerId : int;
        SubbedOutPeriod : IncidentEventType;
        SubbedOutExpandedMinute : int;
        IsManOfTheMatch : bool;
        Stats : Statistics;
        IsHomeTeam : bool;
    }



    type TeamScoreData = {
        Halftime : int;
        Fulltime : int;
        Running : int;
    }



    type TeamData = {
        TeamId : int;
        Formations : seq<FormationData>; 
        IncidentEvents : seq<IncidentEvent>; 
        Players : seq<PlayerData>; 
        Stats : Statistics;
        Name : string;
        ManagerName : string;
        Scores : TeamScoreData; 
        AverageAge : float;
        IsHomeTeam : bool;
    }



    type MatchReport = {
        PlayerIdNameDictionary : Map<int, string>;
        PeriodMinuteLimits : Map<int, int>;
        TimeStamp : DateTime;
        RefereeName : string;
        Attendance : int;
        VenueName : string;
        WeatherCode : string;
        Elapsed : string;
        StartTime : DateTime;
        StartDate : DateTime;
        Score : string;
        HtScore : string;
        FtScore : string;
        EtScore : string;
        PkScore : string;
        StatusCode : int;
        PeriodCode : int;
        Home : TeamData;
        Away : TeamData;
        WhoScoredId : int;
    }



    type CalendarData = {
        WhoScoredId : int;
        MatchDateUtc : DateTime;
        WhoScoredHomeId : int;
        WhoScoredAwayId : int;
        InternalHomeId : int;
        InternalAwayId : int;
    }