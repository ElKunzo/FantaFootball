open System
open FSharp.Configuration
open ElKunzo.FantaFootball.TestApp.TestRunners

type Settings = AppSettings<"App.config">

[<EntryPoint>]
let main argv = 

    GetCompetitionTest () |> ignore
    //DatabaseIOTest () |> ignore
    //GetMatchReportTest () |> ignore
    
    printfn "\nDone!"
    let a = Console.ReadLine()
    0
