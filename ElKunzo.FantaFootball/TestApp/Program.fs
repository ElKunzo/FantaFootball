open System
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

    //UpdateTeamsTest () |> ignore
    StaticDataCacheTest () |> ignore
    //DatabaseIOTest () |> ignore
    //GetMatchReportTest () |> ignore
    
    printfn "\nDone!"
    let a = Console.ReadLine()
    0

