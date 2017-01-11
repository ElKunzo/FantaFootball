namespace ElKunzo.FantaFootball.TestApp

open System.Threading

open ElKunzo.FantaFootball
open ElKunzo.FantaFootball.Internal


module TestRunners = 
    let refreshCaches () = 
        PlayerStaticData.Cache.Update () |> ignore
        PlayerScoreData.Cache.Update () |> ignore
        TeamStaticData.Cache.Update () |> ignore
        FixtureData.Cache.Update () |> ignore

    let UpdateStaticDataAsync () = async {
        printfn "Updating static data...   "
        refreshCaches |> ignore

        let! teams = FootballDataOrg.Processor.processTeamsAsync
        TeamStaticData.Cache.Update () |> ignore
        let! players = FootballDataOrg.Processor.processPlayersAsync
        PlayerStaticData.Cache.Update () |> ignore
        let! fixtures = FootballDataOrg.Processor.processFixturesAsync
        FixtureData.Cache.Update () |> ignore
        let! whoScoredIds = WhoScoredCom.Processor.processPreMatchFixturesAsync 1115149

        refreshCaches ()

        match teams with
        | Success _ -> printfn "Team update successful"
        | Failure x -> printfn "Team update failed: %s" x

        match players with
        | Success _ -> printfn "Player update successful"
        | Failure x -> printfn "Player update failed: %s" x

        match fixtures with
        | Success _ -> printfn "Fixture update successful"
        | Failure x -> printfn "Fixture update failed: %s" x

        match (fst whoScoredIds) with
        | Success _ -> printfn "WhoScored fixture Id update successful"
        | Failure x -> printfn "WhoScored fixture Id update failed: %s" x

        match (snd whoScoredIds) with
        | Success _ -> printfn "WhoScored team Id update successful"
        | Failure x -> printfn "WhoScored team Id update failed: %s" x
    }



    let UpdateMatchReportDataAsync () = async {
        refreshCaches ()
        let updateableFixtures = FixtureData.Cache.GetData 
                                 |> Seq.filter (fun f -> f.Status = FixtureStatus.Finished && f.KickOff >= new System.DateTime(2016, 12, 20))
                                 |> Seq.map (fun x -> x.WhoScoredId)

        printfn "Found %i updateable fixtures..." (updateableFixtures |> Seq.length)

        let! result = WhoScoredCom.Processor.processMatchReportCollectionAsync updateableFixtures 

        result 
        |> Seq.map (fun op -> match op with
                              | Failure x -> printfn "Could not update score for fixture: %s" x
                              | Success _ -> printfn "Fixture updated.")
        |> ignore
    }
        
        
        
