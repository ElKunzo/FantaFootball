namespace ElKunzo.FantaFootball.Internal

open System
open Newtonsoft.Json

open ElKunzo.FantaFootball.External.WhoScoredTypes

module MatchReport = 

    let parseMatchReport (jsonString:string) = 
        let a = jsonString.Split([| "var matchCentreData = " |], StringSplitOptions.RemoveEmptyEntries).[1]
        let b = a.Split([| "\"events\":" |], StringSplitOptions.None).[0]
        let matchAndTeamData = b.Remove(b.Length - 1, 1) + "}"
        let result = JsonConvert.DeserializeObject<MatchReport>(matchAndTeamData)
        result



    let downloadDataAsync (baseUrl:string) (id:int) = 
        async {
            let url = String.Format(baseUrl, id)
            let! result = downloadAsync url buildDefaultHttpClient

            match result with 
            | None -> return None
            | Some x -> return Some (parseMatchReport x)
        }
    



