IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'PlayerScoreDataType')
	CREATE TYPE dbo.PlayerScoreDataType AS TABLE(
	Id INT NOT NULL,
	FixtureId INT NOT NULL,
	PlayerId INT NOT NULL,
	TotalPoints INT,
	MinutesPlayed INT,
	GoalsScored INT,
	Assists INT,
	CleanSheet BIT,
	ShotsSaved INT,
	PenaltiesSaved INT,
	PenaltiesMissed INT,
	GoalsConceded INT,
	YellowCards INT,
	RedCard INT,
	OwnGoals INT
)
GO

GRANT EXECUTE ON TYPE::dbo.PlayerScoreDataType TO FantaFootballRole;
GO