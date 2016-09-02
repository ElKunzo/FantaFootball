open System
open ElKunzo.FantaFootball.TestApp.TestRunners


[<EntryPoint>]
let main argv = 

    //GetCompetitionTest ()
    DatabaseIOTest ()
    GetMatchReportTest ()
    
    printfn "\nDone!"
    let a = Console.ReadLine()
    0
