open System
open FSharp.Configuration
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

    GetCompetitionTest () |> ignore
    DatabaseIOTest () |> ignore
    GetMatchReportTest () |> ignore
    
    printfn "\nDone!"
    let a = Console.ReadLine()
    0
