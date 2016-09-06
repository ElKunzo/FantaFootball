IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'FixtureDataType')
	CREATE TYPE dbo.FixtureDataType AS TABLE(
	Id INT NOT NULL,
	WhoScoredId INT,
	FootballDataId INT,
	StatusId INT NOT NULL,
	KickOffUtc DATETIME,
	MatchDay INT,
	HomeTeamId INT NOT NULL,
	AwayTeamId INT NOT NULL,
	HomeScore INT,
	AwayScore INT
)
GO

GRANT EXECUTE ON TYPE::dbo.FixtureDataType TO FantaFootballRole;
GO