IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'TeamDataType')
	CREATE TYPE dbo.TeamDataType AS TABLE(
	Id INT NOT NULL,
	FootballDataId INT NOT NULL,
	WhoScoredId INT NOT NULL,
	Name NVARCHAR(500),
	FullName NVARCHAR(500),
    Code NVARCHAR(5),
    SquadMarketValue INT,
    CrestUrl NVARCHAR(500)
)
GO

GRANT EXECUTE ON TYPE::dbo.TeamDataType TO FantaFootballRole;
GO