namespace ElKunzo.FantaFootball.Components

module Constants = 
    let commandTimeout = 10
    let connectionString = "Server=tcp:localhost,1433;Integrated Security=SSPI;Database=BLFData16;Timeout=15;Max Pool Size=500"
    let serieA_16_17_Id = 438
    let competitionUrlTemplate = "http://api.football-data.org/v1/competitions/438"
    let matchReportUrlTemplate = "https://www.whoscored.com/Matches/{0}/Live"

