open System
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

    UpdateTeams () |> ignore
    UpdateFixtures () |> ignore
    UpdatePlayers () |> ignore
    UpdateFixtureWhoScoredIds () |> ignore
    UpdateTeamAndPlayerWhoScoredIds () |> ignore
    //UpdatePlayerScoreData (1115237)

    printfn "Done!"
    let a = Console.ReadLine()
    0

