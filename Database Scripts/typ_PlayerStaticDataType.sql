IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'PlayerStaticDataType')
	CREATE TYPE dbo.PlayerStaticDataType AS TABLE(
	Id INT NOT NULL,
	WhoScoredId INT NOT NULL,
	FootballDataTeamId INT,
	TeamId INT NOT NULL,
	JerseyNumber INT,
	Position INT NOT NULL,
	Name NVARCHAR(500),
	FullName NVARCHAR(500),
	DateOfBirth DATETIME,
	Nationality NVARCHAR(50),
	ContractUntil DATETIME,
    MarketValue INT
)
GO

GRANT EXECUTE ON TYPE::dbo.PlayerStaticDataType TO FantaFootballRole;
GO