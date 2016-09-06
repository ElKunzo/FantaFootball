open System
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

    StaticDataCacheTest () |> ignore
    FixtureDataTest () |> ignore
    GetMatchReportTest () |> ignore
    
    printfn "Done!"
    let a = Console.ReadLine()
    0

