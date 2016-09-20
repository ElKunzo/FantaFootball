open System
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

//    UpdateTeams () |> ignore
//    UpdatePlayers () |> ignore
//    UpdateFixtures () |> ignore
//    UpdateFixtureWhoScoredIds () |> ignore
    UpdateTeamAndPlayerWhoScoredIds () |> ignore
    
    printfn "Done!"
    let a = Console.ReadLine()
    0

