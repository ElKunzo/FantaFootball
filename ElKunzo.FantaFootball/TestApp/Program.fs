open System
open ElKunzo.FantaFootball.TestApp.TestRunners

[<EntryPoint>]
let main argv = 

    UpdateStaticDataAsync () |> Async.RunSynchronously
    UpdateMatchReportDataAsync () |> Async.RunSynchronously

    printfn "Done!"
    let a = Console.ReadLine()
    0

