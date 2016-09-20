namespace ElKunzo.FantaFootball

open System
open System.Threading
open ElKunzo.FantaFootball.External.WhoScoredTypes
open ElKunzo.FantaFootball.Internal
open ElKunzo.FantaFootball.DataAccess

module WhoScoredCalendarData = 
    let PrematchMatchDataUrlTemplate = "https://www.whoscored.com/Matches/{0}";



    let updateFixtureIdsAsync (fixtureCache:FixtureData.Cache) (whoScoredCalendarData:seq<CalendarData>) = async {
        let tryFindInternalId (calendarData:CalendarData) (fixtureData:seq<FixtureData.T>) =
            fixtureData |> Seq.tryFind (fun d -> d.HomeTeamId = calendarData.InternalHomeId && d.AwayTeamId = calendarData.InternalAwayId)

        let missingIdFixtures = fixtureCache.PublicData |> Seq.filter (fun t -> t.WhoScoredId = -1)
        if (missingIdFixtures |> Seq.length = 0) then return ()
        let updateableFixtures = whoScoredCalendarData 
                                 |> Seq.map (fun d -> let internalId = missingIdFixtures |> tryFindInternalId d
                                                      match internalId with
                                                      | Some x -> Some (x.Id, d.WhoScoredId)
                                                      | None -> None)
                                 |> Seq.toArray
                                 |> Array.filter (fun x -> x.IsSome)
                                 |> Array.map (fun x -> x.Value)
        let sqlParameter = DatabaseDataAccess.createTableValuedParameter "@WhoScoredIdData" mapIdTupleToSqlType updateableFixtures
        return! DatabaseDataAccess.executeWriteOnlyStoredProcedureAsync "usp_FixtureData_UpdateWhoScoredId" [| sqlParameter |]
    }



    let parseCalendarData (teamCache:TeamStaticData.Cache) (htmlString:string) =
        let data = htmlString
                    .Split([| "matchHeader.load([" |], StringSplitOptions.RemoveEmptyEntries).[1]
                    .Split(']').[0]
                    .Replace("'", "")
                    .Split([| "," |], StringSplitOptions.RemoveEmptyEntries)

        let homeId = Convert.ToInt32(data.[0])
        let awayId = Convert.ToInt32(data.[1])
        let dateValues = data.[4].Split([| '/'; ':'; ' ' |], StringSplitOptions.RemoveEmptyEntries) |> Array.map (fun x -> int x)
        let dateObject = new DateTime(dateValues.[2], dateValues.[1], dateValues.[0], dateValues.[3], dateValues.[4], dateValues.[5])
        let gmtTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")
        let dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateObject, gmtTimeZone)
        let homeTeam = teamCache.PublicData |> Seq.tryFind (fun t -> t.WhoScoredId = homeId)
        let awayTeam = teamCache.PublicData |> Seq.tryFind (fun t -> t.WhoScoredId = awayId)
        
        match homeTeam, awayTeam with
        | Some home, Some away -> Success 
                                    { 
                                        WhoScoredId = -1; 
                                        MatchDateUtc = dateUtc; 
                                        WhoScoredHomeId = homeId; 
                                        WhoScoredAwayId = awayId; 
                                        InternalHomeId = home.Id; 
                                        InternalAwayId = away.Id; 
                                    }
        | _, _ -> Failure "Home or Away team not recognized"



    let downloadDataAsync (baseUrl:string) (teamIdCache:TeamStaticData.Cache) (id:int) = 
        async {
            let url = String.Format(baseUrl, id)
            //added sleep so that who scored does not block 
            //due to DDOS prevention
            Thread.Sleep(Common.genRandomNumber ())
            let! result = downloadAsync url buildDefaultHttpClient

            match result with 
            | Failure x -> return Failure x
            | Success x -> let fixtureData = parseCalendarData teamIdCache x
                           match fixtureData with
                           | Failure y -> return Failure y
                           | Success y -> return Success { y with WhoScoredId = id }
        }



    let updateWhoScoredFixtureIdsAsync (startingMatchId:int) (fixtureDataCache:FixtureData.Cache) (teamIdCache:TeamStaticData.Cache) = async {
        let downloader = downloadDataAsync PrematchMatchDataUrlTemplate teamIdCache
        let knownWhoScoredIds = fixtureDataCache.PublicData |> Seq.filter (fun f -> f.WhoScoredId <> -1) |> Seq.map (fun f -> f.WhoScoredId)
        let possibleIds = new ResizeArray<int>(seq { for i in startingMatchId .. (startingMatchId + 379) do yield i })
        for i in knownWhoScoredIds do possibleIds.Remove i |> ignore

        if (possibleIds |> Seq.length = 0) then 
            return Failure "No fixture found to update."
        else
            let resultList = possibleIds 
                             |> Seq.toList
                             |> List.map downloader
                             |> Common.asyncThrottle 2
                             |> Async.Parallel
                             |> Async.RunSynchronously
            let availableCalendarData = seq { for i in resultList do match i with | Success x -> yield Some x | Failure x -> yield None }
                                        |> Seq.filter (fun x -> x.IsSome)
                                        |> Seq.map (fun x -> x.Value)

            match (Seq.length availableCalendarData) with
            | 0 -> return Failure "No WhoScored Calendar Data found"
            | _ -> return! updateFixtureIdsAsync fixtureDataCache availableCalendarData
    }
