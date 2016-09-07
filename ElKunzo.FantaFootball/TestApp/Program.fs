open System
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

//    UpdatePlayers () |> ignore
    GetMatchReportTest () |> ignore
//    UpdateTest () |> ignore
    
    printfn "Done!"
    let a = Console.ReadLine()
    0

