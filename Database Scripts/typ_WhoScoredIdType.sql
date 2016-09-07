IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'WhoScoredIdType')
	CREATE TYPE dbo.WhoScoredIdType AS TABLE(
	Id INT NOT NULL,
	WhoScoredId INT NOT NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.WhoScoredIdType TO FantaFootballRole;
GO