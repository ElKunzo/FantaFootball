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
        Ratings : Dictionary<int, float>;
        TotalSaves : Dictionary<int, float>;
        ClaimsHigh : Dictionary<int, float>;
        Collected : Dictionary<int, float>;
        ShotsTotal : Dictionary<int, float>;
        ShotsOnPost : Dictionary<int, float>;
        ShotsOnTarget : Dictionary<int, float>;
        ShotsOffTarget : Dictionary<int, float>;
        ShotsBlocked : Dictionary<int, float>;
        Clearances : Dictionary<int, float>;
        Interceptions : Dictionary<int, float>;
        Possession : Dictionary<int, float>;
        Touches : Dictionary<int, float>;
        PassesTotal : Dictionary<int, float>;
        PassesAccurate : Dictionary<int, float>;
        PassesKey : Dictionary<int, float>;
        PassSuccess : Dictionary<int, float>;
        AerialsTotal : Dictionary<int, float>;
        AerialsWon : Dictionary<int, float>;
        AerialSuccess : Dictionary<int, float>;
        OffensiveAerials : Dictionary<int, float>;
        DefensiveAerials : Dictionary<int, float>;
        CornersTotal : Dictionary<int, float>;
        CornersAccurate : Dictionary<int, float>;
        ThrowInsTotal : Dictionary<int, float>;
        ThrowInsAccurate : Dictionary<int, float>;
        ThrowInAccuracy : Dictionary<int, float>;
        OffsidesCaught : Dictionary<int, float>;
        FoulsCommited : Dictionary<int, float>;
        TacklesTotal : Dictionary<int, float>;
        TackleSuccessful : Dictionary<int, float>;
        TackleUnsuccesful : Dictionary<int, float>;
        TackleSuccess : Dictionary<int, float>;
        DribbledPast : Dictionary<int, float>;
        DribblesWon : Dictionary<int, float>;
        DribblesAttempted : Dictionary<int, float>;
        DribblesLost : Dictionary<int, float>;
        DribbleSuccess : Dictionary<int, float>;
        Dispossessed : Dictionary<int, float>;
        Errors : Dictionary<int, float>;
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
        PlayerIdNameDictionary : Dictionary<int, string>;
        PeriodMinuteLimits : Dictionary<int, int>;
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
    }