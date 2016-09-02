namespace ElKunzo.FantaFootball.Components

open System
open Newtonsoft.Json
open ElKunzo.FantaFootball.DataTransferObjects.External
open ElKunzo.FantaFootball.DataTransferObjects.WhoScoredExternal

module JsonParser = 

    let parseJson<'T> jsonString = 
        let result = JsonConvert.DeserializeObject<'T>(jsonString)
        result

    let parseCompetitionDataJson jsonString = 
        parseJson<Competition> jsonString

    let parsePlayerDataJson jsonString = 
        parseJson<PlayerCollection> jsonString

    let parseSeasonFixtures jsonString = 
        parseJson<SeasonFixtures> jsonString

    let parseMatchReport (jsonString:string) = 
        let a = jsonString.Split([| "var matchCentreData = " |], StringSplitOptions.RemoveEmptyEntries).[1]
        let b = a.Split([| "\"events\":" |], StringSplitOptions.None).[0]
        let matchAndTeamData = b.Remove(b.Length - 1, 1) + "}"
        parseJson<MatchReport> matchAndTeamData