open System
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

//    UpdateTeams () |> ignore
//    UpdateFixtures () |> ignore
//    UpdatePlayers () |> ignore
//    UpdateFixtureWhoScoredIds () |> ignore
//    UpdateTeamAndPlayerWhoScoredIds () |> ignore
    UpdatePlayerScoreData (1115153) |> ignore //(1115234) |> ignore
    
    printfn "Done!"
    let a = Console.ReadLine()
    0

